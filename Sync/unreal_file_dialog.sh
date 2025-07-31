#!/bin/bash
# unreal_file_dialog.sh - Cross-desktop file dialog for Unreal Engine on Linux

# Parse command line arguments
MODE="file"
TITLE="Select File"
FILTER_NAME=""
EXTENSIONS=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --mode)
            MODE="$2"
            shift 2
            ;;
        --title)
            TITLE="$2"
            shift 2
            ;;
        --filter)
            FILTER_NAME="$2"
            shift 2
            ;;
        --extensions)
            EXTENSIONS="$2"
            shift 2
            ;;
        *)
            shift
            ;;
    esac
done

# Temporary file to store result
RESULT_FILE="/tmp/unreal_dialog_result_$$"
rm -f "$RESULT_FILE"

# Function for terminal-based file browser
terminal_file_browser() {
    clear
    echo "=================================="
    echo "$TITLE"
    echo "=================================="
    
    if [ "$MODE" = "folder" ]; then
        echo "Current directory: $(pwd)"
        echo ""
        echo "Folders:"
        ls -d */ 2>/dev/null | sed 's|/$||' | nl -nln -w2 -s'. '
        echo ""
        echo "Enter folder number, path, or:"
        echo "  .. - Parent directory"
        echo "  .  - Select current directory"
        echo "  q  - Cancel"
        echo ""
        read -r -p "Selection: " SELECTION
        
        if [ "$SELECTION" = "q" ]; then
            return 1
        elif [ "$SELECTION" = "." ]; then
            pwd > "$RESULT_FILE"
            return 0
        elif [ "$SELECTION" = ".." ]; then
            cd ..
            terminal_file_browser
            return $?
        elif [[ "$SELECTION" =~ ^[0-9]+$ ]]; then
            FOLDER=$(ls -d */ 2>/dev/null | sed 's|/$||' | sed -n "${SELECTION}p")
            if [ -n "$FOLDER" ] && [ -d "$FOLDER" ]; then
                cd "$FOLDER"
                terminal_file_browser
                return $?
            fi
        elif [ -d "$SELECTION" ]; then
            realpath "$SELECTION" > "$RESULT_FILE"
            return 0
        fi
        
        echo "Invalid selection"
        read -p "Press Enter to continue..."
        terminal_file_browser
        
    else  # File mode
        echo "Current directory: $(pwd)"
        echo ""
        echo "Files:"
        
        if [ -n "$EXTENSIONS" ]; then
            # Convert extensions to find pattern
            FIND_PATTERN=""
            for ext in $(echo "$EXTENSIONS" | tr ';' ' '); do
                ext=${ext#\*.}  # Remove *. prefix
                if [ -z "$FIND_PATTERN" ]; then
                    FIND_PATTERN="-name \"*.$ext\""
                else
                    FIND_PATTERN="$FIND_PATTERN -o -name \"*.$ext\""
                fi
            done
            eval "find . -maxdepth 1 -type f \( $FIND_PATTERN \) -printf '%f\n' | sort | nl -nln -w2 -s'. '"
        else
            ls -1 *.* 2>/dev/null | nl -nln -w2 -s'. '
        fi
        
        echo ""
        echo "Enter file number, path, or:"
        echo "  .. - Parent directory"
        echo "  q  - Cancel"
        echo ""
        read -r -p "Selection: " SELECTION
        
        if [ "$SELECTION" = "q" ]; then
            return 1
        elif [ "$SELECTION" = ".." ]; then
            cd ..
            terminal_file_browser
            return $?
        elif [[ "$SELECTION" =~ ^[0-9]+$ ]]; then
            if [ -n "$EXTENSIONS" ]; then
                FILE=$(eval "find . -maxdepth 1 -type f \( $FIND_PATTERN \) -printf '%f\n' | sort | sed -n '${SELECTION}p'")
            else
                FILE=$(ls -1 *.* 2>/dev/null | sed -n "${SELECTION}p")
            fi
            if [ -n "$FILE" ] && [ -f "$FILE" ]; then
                realpath "$FILE" > "$RESULT_FILE"
                return 0
            fi
        elif [ -f "$SELECTION" ]; then
            realpath "$SELECTION" > "$RESULT_FILE"
            return 0
        fi
        
        echo "Invalid selection"
        read -p "Press Enter to continue..."
        terminal_file_browser
    fi
}

# Try GUI options first if DISPLAY is set
if [ -n "$DISPLAY" ]; then
    # Try kdialog (KDE)
    if command -v kdialog >/dev/null 2>&1; then
        if [ "$MODE" = "folder" ]; then
            RESULT=$(kdialog --getexistingdirectory "$HOME" --title "$TITLE" 2>/dev/null)
        else
            # Build kdialog filter string
            KDIALOG_FILTER=""
            if [ -n "$FILTER_NAME" ] && [ -n "$EXTENSIONS" ]; then
                KDIALOG_FILTER="$FILTER_NAME ($EXTENSIONS)"
            fi
            RESULT=$(kdialog --getopenfilename "$HOME" "$KDIALOG_FILTER" --title "$TITLE" 2>/dev/null)
        fi
        
        if [ $? -eq 0 ] && [ -n "$RESULT" ]; then
            echo "$RESULT"
            exit 0
        fi
    fi
    
    # Try zenity (GNOME and others)
    if command -v zenity >/dev/null 2>&1; then
        ZENITY_ARGS="--title=\"$TITLE\""
        
        if [ "$MODE" = "folder" ]; then
            ZENITY_ARGS="$ZENITY_ARGS --file-selection --directory"
        else
            ZENITY_ARGS="$ZENITY_ARGS --file-selection"
            if [ -n "$FILTER_NAME" ] && [ -n "$EXTENSIONS" ]; then
                # Zenity file filter format
                ZENITY_ARGS="$ZENITY_ARGS --file-filter=\"$FILTER_NAME | $EXTENSIONS\""
                ZENITY_ARGS="$ZENITY_ARGS --file-filter=\"All files | *\""
            fi
        fi
        
        RESULT=$(eval "zenity $ZENITY_ARGS 2>/dev/null")
        if [ $? -eq 0 ] && [ -n "$RESULT" ]; then
            echo "$RESULT"
            exit 0
        fi
    fi
fi

# Try dialog for terminal UI
if command -v dialog >/dev/null 2>&1; then
    if [ "$MODE" = "folder" ]; then
        RESULT=$(dialog --stdout --title "$TITLE" --dselect "$HOME/" 20 60 2>/dev/null)
    else
        RESULT=$(dialog --stdout --title "$TITLE" --fselect "$HOME/" 20 60 2>/dev/null)
    fi
    
    if [ $? -eq 0 ] && [ -n "$RESULT" ]; then
        echo "$RESULT"
        exit 0
    fi
fi

# Fallback to custom terminal browser
if [ -t 0 ]; then
    # We have a terminal
    terminal_file_browser
    if [ $? -eq 0 ] && [ -f "$RESULT_FILE" ]; then
        cat "$RESULT_FILE"
        rm -f "$RESULT_FILE"
        exit 0
    fi
else
    # No terminal, try to create one
    if [ -n "$DISPLAY" ]; then
        # Create a temporary script for the terminal
        TERM_SCRIPT="/tmp/unreal_term_dialog_$$.sh"
        cat > "$TERM_SCRIPT" << 'EOF'
#!/bin/bash
source "$1"
terminal_file_browser
if [ $? -eq 0 ] && [ -f "$RESULT_FILE" ]; then
    cat "$RESULT_FILE"
fi
rm -f "$RESULT_FILE"
read -p "Press Enter to close..."
EOF
        chmod +x "$TERM_SCRIPT"
        
        # Try different terminal emulators
        if command -v xterm >/dev/null 2>&1; then
            xterm -title "$TITLE" -e bash "$TERM_SCRIPT" "$0" 2>/dev/null
        elif command -v gnome-terminal >/dev/null 2>&1; then
            gnome-terminal --title="$TITLE" -- bash "$TERM_SCRIPT" "$0" 2>/dev/null
        elif command -v konsole >/dev/null 2>&1; then
            konsole --title "$TITLE" -e bash "$TERM_SCRIPT" "$0" 2>/dev/null
        elif command -v xfce4-terminal >/dev/null 2>&1; then
            xfce4-terminal --title="$TITLE" -e "bash $TERM_SCRIPT $0" 2>/dev/null
        fi
        
        # Check if result was written
        if [ -f "$RESULT_FILE" ]; then
            cat "$RESULT_FILE"
            rm -f "$RESULT_FILE"
            rm -f "$TERM_SCRIPT"
            exit 0
        fi
        
        rm -f "$TERM_SCRIPT"
    fi
fi

# If we get here, all methods failed
exit 1