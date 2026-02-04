// Fill out your copyright notice in the Description page of Project Settings.


#include "CoreBlueprintLibrary.h"
#include "AppManager.h"
#include "CommandRunner.h"
#include "DesktopPlatformModule.h"
#include "ThemeDB.h"
#include "Tween/TweenManager.h"
#include "Kismet/KismetSystemLibrary.h"
#include "HAL/PlatformFileManager.h"
#include "Engine/Texture2D.h"
#include "IImageWrapper.h"
#include "IImageWrapperModule.h"
#include "AssetSelection.h"

#if PLATFORM_WINDOWS
#include "Windows/AllowWindowsPlatformTypes.h"
#include <windows.h>
#include <commdlg.h>
#include <shlobj.h>
#include <commoncontrols.h>
#include "Windows/HideWindowsPlatformTypes.h"
#elif PLATFORM_MAC
#include <Cocoa/Cocoa.h>
#elif PLATFORM_LINUX
#include <gio/gio.h>
#include <gtk/gtk.h>
#endif

UThemeDB* UCoreBlueprintLibrary::ThemeDB(){
  return UThemeDB::Get();
}

bool UCoreBlueprintLibrary::IsEditorMode( UObject *worldContextContainer ){
  return worldContextContainer->GetWorld()->IsPlayInEditor() || worldContextContainer->GetWorld()->IsEditorWorld();
}

FString UCoreBlueprintLibrary::OpenFileBrowserWindow( FString title, FString fileTypes, FString cancelValue, int trimCount ){
  FString result = cancelValue;

#if PLATFORM_WINDOWS
  OPENFILENAMEW ofn;
  WCHAR szFile[MAX_PATH] = { 0 };

  ZeroMemory(&ofn, sizeof(ofn));
  ofn.lStructSize = sizeof(ofn);
  ofn.hwndOwner = nullptr;
  ofn.lpstrFile = szFile;
  ofn.nMaxFile = sizeof( szFile ) / sizeof(WCHAR);

  FString FilterStr = fileTypes;
  FilterStr.ReplaceInline(TEXT("|"), TEXT("\0"), ESearchCase::IgnoreCase);
  FilterStr += TEXT("\0");

  ofn.lpstrFilter = *FilterStr;
  ofn.nFilterIndex = 1;
  ofn.lpstrTitle = *title;
  ofn.lpstrInitialDir = *FPlatformProcess::GetCurrentWorkingDirectory();
  ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST | OFN_NOCHANGEDIR;

  if( GetOpenFileNameW( &ofn ) ){
    result = FString( szFile );
    result = FPaths::ConvertRelativePathToFull( result );
    result = TrimmedPaths( result, trimCount );
  }

#elif PLATFORM_MAC
  @autoreleasepool{
    NSOpenPanel* openPanel = [NSOpenPanel openPanel];
    [openPanel setTitle:[NSString stringWithFString:title]];
    [openPanel setCanChooseFiles:YES];
    [openPanel setCanChooseDirectories:NO];
    [openPanel setAllowsMultipleSelection:NO];

    // Parse file types (format: "Description|*.ext1;*.ext2")
    TArray<FString> typePairs;
    fileTypes.ParseIntoArray(typePairs, TEXT("|"));

    if( typePairs.Num() >= 2 ){
      NSMutableArray* allowedTypes = [NSMutableArray array];
      TArray<FString> extensions;
      typePairs[1].ParseIntoArray( extensions, TEXT(";") );

      for( const FString& extension : extensions ){
        FString cleanExtension = extension;
        cleanExtension.ReplaceInline(TEXT("*."), TEXT(""));
        [allowedTypes addObject:[NSString stringWithFString:cleanExtension]];
      }

      [openPanel setAllowedFileTypes:allowedTypes];
    }

    if( [openPanel runModal] == NSModalResponseOK ){
      NSURL* url = [[openPanel URLs] objectAtIndex:0];
      result = FString([url path]);
      result = TrimmedPaths( result, trimCount );
    }
  }

#elif PLATFORM_LINUX

  FString scriptPath = FPaths::Combine( FPaths::ProjectDir(), TEXT("Content/Sync/unreal_file_dialog.sh") );

  if( FPaths::FileExists( scriptPath ) ){
    // Build command arguments
    FString args = FString::Printf( TEXT("--mode file --title \"%s\""), *title );

    // Add file type filters
    TArray<FString> typePairs;
    fileTypes.ParseIntoArray( typePairs, TEXT("|") );
    if( typePairs.Num() >= 2 ){
      args += FString::Printf( TEXT(" --filter \"%s\" --extensions \"%s\""), *typePairs[0], *typePairs[1] );
    }

    // Execute the script
    FString stdOut;
    int32 returnCode;
    FPlatformProcess::ExecProcess(*scriptPath, *args, &returnCode, &stdOut, nullptr);

    if( returnCode == 0 && !stdOut.IsEmpty() ){
      result = stdOut.TrimStartAndEnd();
      result = TrimmedPaths( result, trimCount );
    }
  }
  else{
    UE_LOG(LogTemp, Error, TEXT("Linux file dialog script not found at: %s"), *scriptPath );
  }

#endif

  return result;
}

FString UCoreBlueprintLibrary::OpenFolderBrowserWindow( FString title, FString cancelValue ){
  FString result = cancelValue;

#if PLATFORM_WINDOWS
  BROWSEINFOW bi = { 0 };
  bi.lpszTitle = *title;
  bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;

  LPITEMIDLIST pidl = SHBrowseForFolderW(&bi);
  if (pidl != nullptr)
  {
    WCHAR path[MAX_PATH];
    if (SHGetPathFromIDListW(pidl, path))
    {
      result = FString(path);
      result = FPaths::ConvertRelativePathToFull( result );
    }

    // Free memory used
    IMalloc* imalloc = nullptr;
    if (SUCCEEDED(SHGetMalloc(&imalloc)))
    {
      imalloc->Free(pidl);
      imalloc->Release();
    }
  }
#elif PLATFORM_MAC
  @autoreleasepool
  {
    NSOpenPanel* openPanel = [NSOpenPanel openPanel];
    [openPanel setTitle:[NSString stringWithFString:title]];
    [openPanel setCanChooseFiles:NO];
    [openPanel setCanChooseDirectories:YES];
    [openPanel setAllowsMultipleSelection:NO];

    if ([openPanel runModal] == NSModalResponseOK)
    {
      NSURL* url = [[openPanel URLs] objectAtIndex:0];
      result = FString([url path]);
    }
  }

#elif PLATFORM_LINUX
  FString scriptPath = FPaths::Combine(FPaths::ProjectDir(), TEXT("Content/Sync/unreal_file_dialog.sh"));

  if( FPaths::FileExists( scriptPath ) ){
    // Build command arguments
    FString args = FString::Printf( TEXT("--mode folder --title \"%s\""), *title );

    // Execute the script
    FString stdOut;
    int32 returnCode;
    FPlatformProcess::ExecProcess( *scriptPath, *args, &returnCode, &stdOut, nullptr );

    if( returnCode == 0 && !stdOut.IsEmpty() ){
      result = stdOut.TrimStartAndEnd();
    }
  }
  else{
    UE_LOG(LogTemp, Error, TEXT("Linux file dialog script not found at: %s"), *scriptPath );
  }
#endif

  return result;
}

ATweenManager * UCoreBlueprintLibrary::TweenManager(){
  return ATweenManager::Get();
}

ACommandRunner * UCoreBlueprintLibrary::CommandRunner(){
  return ACommandRunner::Get();
}

TArray<FString> UCoreBlueprintLibrary::SplitAndTrimString( FString string ){
  TArray<FString> resultArray;
  
  if( string.IsEmpty() ){
    return resultArray;
  }
  
  string.ParseIntoArray( resultArray, TEXT(","), true );
  for( FString& resultString : resultArray ){
    resultString = resultString.TrimStartAndEnd();
  }
    
  return resultArray;
}

FString UCoreBlueprintLibrary::TrimmedPaths( FString fullPath, int trimCount ){
  TArray<FString> resultArray;
  
  if( fullPath.IsEmpty() ){
    return "";
  }
  
  fullPath.ParseIntoArray( resultArray, TEXT("/"), true );

  for( int i = 0; i < trimCount; i++ ){
    resultArray.Pop();
  }

  return FString::Join( resultArray, TEXT("/") );
}

void UCoreBlueprintLibrary::SetThemeDB( UThemeDB *themeDB ){
  UThemeDB::SetInstance( themeDB );
}

void UCoreBlueprintLibrary::ActivateAction( UObject* worldContext ){
  UKismetSystemLibrary::ExecuteConsoleCommand( worldContext, "t.maxfps 60" );
  AAppManager::lastActionTime = FPlatformTime::Seconds();
  AAppManager::currentFPS = 60;
}

TArray<FFileInfo> UCoreBlueprintLibrary::GetFilesAtPath( FString path, bool bIncludeDirectories, bool bRecursive ){
  TArray<FFileInfo> result;

  if( path.IsEmpty() ){
    return result;
  }

  // Convert to absolute path
  path = FPaths::ConvertRelativePathToFull( path );

  // Check if path exists
  IPlatformFile& platformFile = FPlatformFileManager::Get().GetPlatformFile();
  if( !platformFile.DirectoryExists( *path ) ){
    UE_LOG(LogTemp, Warning, TEXT("Directory does not exist: %s"), *path);
    return result;
  }

  // Visitor class to collect files
  class FFileVisitor : public IPlatformFile::FDirectoryVisitor{
  public:
    TArray<FFileInfo>& Files;
    bool bIncludeDirs;

    FFileVisitor( TArray<FFileInfo>& InFiles, bool InIncludeDirs )
      : Files( InFiles ), bIncludeDirs( InIncludeDirs ){
    }

    virtual bool Visit( const TCHAR* FilenameOrDirectory, bool bIsDirectory ) override{
      FFileInfo info;
      info.FullPath = FString( FilenameOrDirectory );
      info.FileName = FPaths::GetCleanFilename( info.FullPath );
      info.bIsDirectory = bIsDirectory;
      info.DateModified = IFileManager::Get().GetTimeStamp( *info.FullPath );

      if( bIsDirectory ){
        info.FileSize = 0;
        if( bIncludeDirs ){
          info.Icon = GetFileIcon( info.FullPath, true );
          Files.Add( info );
        }
      }
      else{
        info.FileSize = IFileManager::Get().FileSize( *info.FullPath );
        info.Icon = GetFileIcon( info.FullPath, false );
        Files.Add( info );
      }

      return true; // Continue iteration
    }
  };

  FFileVisitor visitor( result, bIncludeDirectories );

  if( bRecursive ){
    platformFile.IterateDirectoryRecursively( *path, visitor );
  }
  else{
    platformFile.IterateDirectory( *path, visitor );
  }

  // Sort: directories first (alphabetically), then files (alphabetically)
  result.Sort( []( const FFileInfo& A, const FFileInfo& B ){
    if( A.bIsDirectory != B.bIsDirectory ){
      return A.bIsDirectory; // Directories come first
    }
    return A.FileName.ToLower() < B.FileName.ToLower(); // Case-insensitive alphabetical order
  });

  return result;
}

UTexture2D* UCoreBlueprintLibrary::GetFileIcon( const FString& filePath, bool bIsDirectory ){
  UTexture2D* iconTexture = nullptr;

#if PLATFORM_WINDOWS
  SHFILEINFOW fileInfo;
  ZeroMemory( &fileInfo, sizeof( fileInfo ) );

  // Always use SHGFI_USEFILEATTRIBUTES to get icon based on extension/attributes
  // This is more reliable than accessing the actual file (avoids path format issues)
  DWORD flags = SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES;

  DWORD_PTR result = SHGetFileInfoW(
    *filePath,
    bIsDirectory ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL,
    &fileInfo,
    sizeof( fileInfo ),
    flags
  );

  if( result && fileInfo.hIcon ){
    // Get icon info
    ICONINFO iconInfo;
    if( GetIconInfo( fileInfo.hIcon, &iconInfo ) ){
      BITMAP bmp;
      if( GetObjectW( iconInfo.hbmColor, sizeof( BITMAP ), &bmp ) ){
        int32 width = bmp.bmWidth;
        int32 height = bmp.bmHeight;

        // Get icon bitmap data
        HDC hdc = GetDC( nullptr );
        HDC hdcMem = CreateCompatibleDC( hdc );
        HBITMAP hbmOld = (HBITMAP)SelectObject( hdcMem, iconInfo.hbmColor );

        BITMAPINFO bmi;
        ZeroMemory( &bmi, sizeof( BITMAPINFO ) );
        bmi.bmiHeader.biSize = sizeof( BITMAPINFOHEADER );
        bmi.bmiHeader.biWidth = width;
        bmi.bmiHeader.biHeight = -height; // Top-down DIB
        bmi.bmiHeader.biPlanes = 1;
        bmi.bmiHeader.biBitCount = 32;
        bmi.bmiHeader.biCompression = BI_RGB;

        TArray<uint8> pixelData;
        pixelData.SetNum( width * height * 4 );

        if( GetDIBits( hdcMem, iconInfo.hbmColor, 0, height, pixelData.GetData(), &bmi, DIB_RGB_COLORS ) ){
          // Get alpha channel from the mask if color bitmap alpha is zero
          // Windows icons often have alpha=0 in the color bitmap
          TArray<uint8> maskData;
          bool bNeedAlphaFromMask = true;

          // Check if alpha channel has valid data
          for( int32 i = 0; i < width * height; i++ ){
            if( pixelData[i * 4 + 3] != 0 ){
              bNeedAlphaFromMask = false;
              break;
            }
          }

          if( bNeedAlphaFromMask && iconInfo.hbmMask ){
            // Get mask bitmap for alpha
            maskData.SetNum( width * height * 4 );
            HBITMAP hbmOldMask = (HBITMAP)SelectObject( hdcMem, iconInfo.hbmMask );
            GetDIBits( hdcMem, iconInfo.hbmMask, 0, height, maskData.GetData(), &bmi, DIB_RGB_COLORS );
            SelectObject( hdcMem, hbmOldMask );

            // Apply mask as alpha (mask is inverted: 0 = opaque, 255 = transparent)
            for( int32 i = 0; i < width * height; i++ ){
              pixelData[i * 4 + 3] = 255 - maskData[i * 4]; // Invert mask for alpha
            }
          }
          else if( bNeedAlphaFromMask ){
            // No mask available, set all pixels to fully opaque
            for( int32 i = 0; i < width * height; i++ ){
              pixelData[i * 4 + 3] = 255;
            }
          }

          // Create texture - data is already in BGRA format matching PF_B8G8R8A8
          iconTexture = UTexture2D::CreateTransient( width, height, PF_B8G8R8A8 );
          if( iconTexture ){
            void* textureData = iconTexture->GetPlatformData()->Mips[0].BulkData.Lock( LOCK_READ_WRITE );
            FMemory::Memcpy( textureData, pixelData.GetData(), pixelData.Num() );
            iconTexture->GetPlatformData()->Mips[0].BulkData.Unlock();
            iconTexture->UpdateResource();
          }
        }

        SelectObject( hdcMem, hbmOld );
        DeleteDC( hdcMem );
        ReleaseDC( nullptr, hdc );
      }

      if( iconInfo.hbmColor ){
        DeleteObject( iconInfo.hbmColor );
      }
      if( iconInfo.hbmMask ){
        DeleteObject( iconInfo.hbmMask );
      }
    }

    DestroyIcon( fileInfo.hIcon );
  }

#elif PLATFORM_MAC
  @autoreleasepool{
    NSString* nsFilePath = [NSString stringWithFString:filePath];
    NSImage* icon = [[NSWorkspace sharedWorkspace] iconForFile:nsFilePath];

    if( icon ){
      // Set to small icon size
      [icon setSize:NSMakeSize( 16, 16 )];

      // Convert NSImage to bitmap
      NSBitmapImageRep* bitmap = [[NSBitmapImageRep alloc]
        initWithBitmapDataPlanes:NULL
        pixelsWide:16
        pixelsHigh:16
        bitsPerSample:8
        samplesPerPixel:4
        hasAlpha:YES
        isPlanar:NO
        colorSpaceName:NSCalibratedRGBColorSpace
        bytesPerRow:16 * 4
        bitsPerPixel:32];

      [NSGraphicsContext saveGraphicsState];
      NSGraphicsContext* context = [NSGraphicsContext graphicsContextWithBitmapImageRep:bitmap];
      [NSGraphicsContext setCurrentContext:context];
      [icon drawInRect:NSMakeRect( 0, 0, 16, 16 )];
      [NSGraphicsContext restoreGraphicsState];

      // Get pixel data
      unsigned char* bitmapData = [bitmap bitmapData];
      if( bitmapData ){
        TArray<uint8> pixelData;
        pixelData.SetNum( 16 * 16 * 4 );

        // Convert RGBA (NSBitmapImageRep format) to BGRA (PF_B8G8R8A8 format)
        for( int32 i = 0; i < 16 * 16; i++ ){
          int32 srcIndex = i * 4;
          int32 dstIndex = i * 4;
          pixelData[dstIndex + 0] = bitmapData[srcIndex + 2]; // B from R
          pixelData[dstIndex + 1] = bitmapData[srcIndex + 1]; // G
          pixelData[dstIndex + 2] = bitmapData[srcIndex + 0]; // R from B
          pixelData[dstIndex + 3] = bitmapData[srcIndex + 3]; // A
        }

        // Create texture
        iconTexture = UTexture2D::CreateTransient( 16, 16, PF_B8G8R8A8 );
        if( iconTexture ){
          void* textureData = iconTexture->GetPlatformData()->Mips[0].BulkData.Lock( LOCK_READ_WRITE );
          FMemory::Memcpy( textureData, pixelData.GetData(), pixelData.Num() );
          iconTexture->GetPlatformData()->Mips[0].BulkData.Unlock();
          iconTexture->UpdateResource();
        }
      }

      [bitmap release];
    }
  }

#elif PLATFORM_LINUX
  // Use GTK/GIO to get actual system icons on Linux
  static bool bGtkInitialized = false;
  if( !bGtkInitialized ){
    // Initialize GTK (without requiring display)
    gtk_init_check( nullptr, nullptr );
    bGtkInitialized = true;
  }

  int32 iconSize = 16;
  GIcon* gicon = nullptr;

  if( bIsDirectory ){
    // Get folder icon
    gicon = g_themed_icon_new( "folder" );
  }
  else{
    // Get icon based on MIME type
    gchar* contentType = g_content_type_guess( TCHAR_TO_UTF8(*filePath), nullptr, 0, nullptr );
    if( contentType ){
      gicon = g_content_type_get_icon( contentType );
      g_free( contentType );
    }
  }

  if( gicon ){
    GtkIconTheme* iconTheme = gtk_icon_theme_get_default();
    GtkIconInfo* iconInfo = gtk_icon_theme_lookup_by_gicon( iconTheme, gicon, iconSize, GTK_ICON_LOOKUP_FORCE_SIZE );

    if( iconInfo ){
      GdkPixbuf* pixbuf = gtk_icon_info_load_icon( iconInfo, nullptr );

      if( pixbuf ){
        int width = gdk_pixbuf_get_width( pixbuf );
        int height = gdk_pixbuf_get_height( pixbuf );
        int channels = gdk_pixbuf_get_n_channels( pixbuf );
        int rowstride = gdk_pixbuf_get_rowstride( pixbuf );
        guchar* pixels = gdk_pixbuf_get_pixels( pixbuf );

        // Create texture
        iconTexture = UTexture2D::CreateTransient( width, height, PF_B8G8R8A8 );
        if( iconTexture ){
          TArray<uint8> pixelData;
          pixelData.SetNum( width * height * 4 );

          // Convert GdkPixbuf format to BGRA
          for( int32 y = 0; y < height; y++ ){
            for( int32 x = 0; x < width; x++ ){
              int32 dstIndex = (y * width + x) * 4;
              int32 srcIndex = y * rowstride + x * channels;

              if( channels >= 3 ){
                pixelData[dstIndex + 0] = pixels[srcIndex + 2]; // B
                pixelData[dstIndex + 1] = pixels[srcIndex + 1]; // G
                pixelData[dstIndex + 2] = pixels[srcIndex + 0]; // R
                pixelData[dstIndex + 3] = (channels == 4) ? pixels[srcIndex + 3] : 255; // A
              }
              else{
                // Grayscale fallback
                pixelData[dstIndex + 0] = pixels[srcIndex];
                pixelData[dstIndex + 1] = pixels[srcIndex];
                pixelData[dstIndex + 2] = pixels[srcIndex];
                pixelData[dstIndex + 3] = 255;
              }
            }
          }

          void* textureData = iconTexture->GetPlatformData()->Mips[0].BulkData.Lock( LOCK_READ_WRITE );
          FMemory::Memcpy( textureData, pixelData.GetData(), pixelData.Num() );
          iconTexture->GetPlatformData()->Mips[0].BulkData.Unlock();
          iconTexture->UpdateResource();
        }

        g_object_unref( pixbuf );
      }

      g_object_unref( iconInfo );
    }

    g_object_unref( gicon );
  }

  // Fallback: create simple colored icon if GTK failed
  if( !iconTexture ){
    iconTexture = UTexture2D::CreateTransient( iconSize, iconSize, PF_B8G8R8A8 );

    if( iconTexture ){
      TArray<uint8> pixelData;
      pixelData.SetNum( iconSize * iconSize * 4 );

      // Determine color based on file type
      FColor iconColor;
      if( bIsDirectory ){
        iconColor = FColor( 255, 200, 100, 255 ); // Orange for directories
      }
      else{
        FString extension = FPaths::GetExtension( filePath ).ToLower();
        if( extension == TEXT("txt") || extension == TEXT("md") || extension == TEXT("log") ){
          iconColor = FColor( 200, 200, 200, 255 ); // Gray for text files
        }
        else if( extension == TEXT("cpp") || extension == TEXT("h") || extension == TEXT("cs") || extension == TEXT("py") ){
          iconColor = FColor( 100, 150, 255, 255 ); // Blue for code files
        }
        else if( extension == TEXT("png") || extension == TEXT("jpg") || extension == TEXT("jpeg") || extension == TEXT("bmp") ){
          iconColor = FColor( 100, 255, 150, 255 ); // Green for images
        }
        else{
          iconColor = FColor( 180, 180, 180, 255 ); // Default gray
        }
      }

      // Create a simple filled square icon
      for( int32 y = 0; y < iconSize; y++ ){
        for( int32 x = 0; x < iconSize; x++ ){
          int32 index = (y * iconSize + x) * 4;
          // Add a border
          if( x == 0 || x == iconSize - 1 || y == 0 || y == iconSize - 1 ){
            pixelData[index + 0] = 50;
            pixelData[index + 1] = 50;
            pixelData[index + 2] = 50;
            pixelData[index + 3] = 255;
          }
          else{
            pixelData[index + 0] = iconColor.B;
            pixelData[index + 1] = iconColor.G;
            pixelData[index + 2] = iconColor.R;
            pixelData[index + 3] = iconColor.A;
          }
        }
      }

      void* textureData = iconTexture->GetPlatformData()->Mips[0].BulkData.Lock( LOCK_READ_WRITE );
      FMemory::Memcpy( textureData, pixelData.GetData(), pixelData.Num() );
      iconTexture->GetPlatformData()->Mips[0].BulkData.Unlock();
      iconTexture->UpdateResource();
    }
  }

#endif

  return iconTexture;
}

void UCoreBlueprintLibrary::OpenInSystemExplorer( FString path, bool bSelectFile ){
  if( path.IsEmpty() ){
    return;
  }

  // Convert to platform-specific path format
  FPaths::MakePlatformFilename( path );

#if PLATFORM_WINDOWS
  if( bSelectFile && FPaths::FileExists( path ) ){
    // Open explorer and select the file
    FString command = FString::Printf( TEXT("/select,\"%s\""), *path );
    FPlatformProcess::CreateProc( TEXT("explorer.exe"), *command, true, false, false, nullptr, 0, nullptr, nullptr );
  }
  else{
    // Just open the folder
    FString folderPath = path;
    if( !FPaths::DirectoryExists( folderPath ) ){
      folderPath = FPaths::GetPath( path );
    }
    FPlatformProcess::CreateProc( TEXT("explorer.exe"), *folderPath, true, false, false, nullptr, 0, nullptr, nullptr );
  }

#elif PLATFORM_MAC
  @autoreleasepool{
    NSString* nsPath = [NSString stringWithFString:path];

    if( bSelectFile && [[NSFileManager defaultManager] fileExistsAtPath:nsPath] ){
      // Open Finder and select the file
      [[NSWorkspace sharedWorkspace] selectFile:nsPath inFileViewerRootedAtPath:@""];
    }
    else{
      // Just open the folder
      NSString* folderPath = nsPath;
      BOOL isDirectory = NO;
      if( ![[NSFileManager defaultManager] fileExistsAtPath:folderPath isDirectory:&isDirectory] || !isDirectory ){
        folderPath = [nsPath stringByDeletingLastPathComponent];
      }
      [[NSWorkspace sharedWorkspace] openFile:folderPath];
    }
  }

#elif PLATFORM_LINUX
  FString folderPath = path;

  // Determine if path is a file or directory
  IPlatformFile& platformFile = FPlatformFileManager::Get().GetPlatformFile();
  bool bIsFile = platformFile.FileExists( *path );
  bool bIsDirectory = platformFile.DirectoryExists( *path );

  if( bIsFile && !bIsDirectory ){
    folderPath = FPaths::GetPath( path );
  }

  // Try common Linux file managers in order of preference
  // xdg-open is the most universal solution
  FString command = FString::Printf( TEXT("xdg-open \"%s\""), *folderPath );
  int32 returnCode = 0;
  FPlatformProcess::ExecProcess( TEXT("/bin/sh"), *FString::Printf( TEXT("-c \"%s\""), *command ), &returnCode, nullptr, nullptr );

  if( returnCode != 0 ){
    // Fallback: try nautilus (GNOME)
    command = FString::Printf( TEXT("nautilus \"%s\""), *folderPath );
    FPlatformProcess::ExecProcess( TEXT("/bin/sh"), *FString::Printf( TEXT("-c \"%s\""), *command ), &returnCode, nullptr, nullptr );
  }

#endif
}

FString UCoreBlueprintLibrary::GetFileContentsAsText( FString filePath ){
  FString fileContents;

  // Check if file exists
  IPlatformFile& platformFile = FPlatformFileManager::Get().GetPlatformFile();
  if( !platformFile.FileExists( *filePath ) ){
    UE_LOG( LogTemp, Warning, TEXT("File does not exist: %s"), *filePath );
    return FString();
  }

  // Load the file contents
  bool success = FFileHelper::LoadFileToString( fileContents, *filePath );
  
  if( !success ){
    UE_LOG( LogTemp, Error, TEXT("Failed to read file: %s"), *filePath );
    return FString();
  }

  return fileContents;
}

bool UCoreBlueprintLibrary::WriteTextToFile( FString filePath, FString textContent, bool bAppend ){
  // Ensure the directory exists
  FString directoryPath = FPaths::GetPath( filePath );
  IPlatformFile& platformFile = FPlatformFileManager::Get().GetPlatformFile();
  
  if( !directoryPath.IsEmpty() && !platformFile.DirectoryExists( *directoryPath ) ){
    if( !platformFile.CreateDirectoryTree( *directoryPath ) ){
      UE_LOG( LogTemp, Error, TEXT("Failed to create directory: %s"), *directoryPath );
      return false;
    }
  }

  // Determine write flags
  uint32 writeFlags = FILEWRITE_None;
  
  if( bAppend ){
    writeFlags |= FILEWRITE_Append;
  }

  // Write the text to file
  bool bSuccess = FFileHelper::SaveStringToFile( textContent, *filePath, FFileHelper::EEncodingOptions::AutoDetect, &IFileManager::Get(), writeFlags );
  
  if( !bSuccess ){
    UE_LOG( LogTemp, Error, TEXT("Failed to write to file: %s"), *filePath );
    return false;
  }

  UE_LOG( LogTemp, Log, TEXT("Successfully wrote to file: %s"), *filePath );
  return true;
}

void UCoreBlueprintLibrary::WriteSyncLock( const FString &lockFilePath, const TArray<FLockRow> &lockRows ){
  TArray<FString> lines;
  for( const FLockRow& row : lockRows ){
    lines.Add( row.GetAsString() );
  }
  
  FString fileContent = FString::Join( lines, TEXT("\n") );
  WriteTextToFile( lockFilePath, fileContent, false );
}

TArray<FLockRow> UCoreBlueprintLibrary::ReadSyncLock( const FString &lockFilePath ){
  FString fileContent = GetFileContentsAsText( lockFilePath );
  TArray<FLockRow> lockRows;
  if( fileContent.IsEmpty() ){
    return lockRows;
  }
  TArray<FString> lines;
  fileContent.ParseIntoArray( lines, TEXT("\n"), true );
  for( const FString& line : lines ){
    lockRows.Add( FLockRow( line ) );
  }
  
  return lockRows;
}

