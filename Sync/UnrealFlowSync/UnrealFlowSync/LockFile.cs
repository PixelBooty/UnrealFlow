using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace UnrealFlow {

  struct LockEntry {
    public string user;
    public DateTime lockDate;
    public DateTime unlockDate;

    public bool isLocked => this.unlockDate.Year == 1;
  }

  class LockFile {

    private static readonly DateTime _unlockedSentinel = new DateTime( 1, 1, 1, 0, 0, 0 );
    private const string _dateFormat = "yyyy.MM.dd-HH.mm.ss";

    public string filePath;
    public List<LockEntry> entries = new List<LockEntry>();

    public bool isLocked => this.entries.Count > 0 && this.entries.Last().isLocked;
    public string lockedBy => this.isLocked ? this.entries.Last().user : null;

    public bool HasConflict( string currentUser, DateTime remoteModTime ) {
      foreach( LockEntry entry in this.entries ) {
        if( entry.user != currentUser ) {
          continue;
        }
        DateTime effectiveUnlock = entry.isLocked ? DateTime.MaxValue : entry.unlockDate;
        if( remoteModTime > entry.lockDate && remoteModTime < effectiveUnlock ) {
          return true;
        }
      }
      return false;
    }

    public string LockedByOtherDuring( string currentUser, DateTime time ) {
      foreach( LockEntry entry in this.entries ) {
        if( entry.user == currentUser ) {
          continue;
        }
        DateTime effectiveUnlock = entry.isLocked ? DateTime.MaxValue : entry.unlockDate;
        if( time > entry.lockDate && time < effectiveUnlock ) {
          return entry.user;
        }
      }
      return null;
    }

    public static string ToReconcilePath( string filePath ) {
      string ext = Path.GetExtension( filePath );
      return filePath.Substring( 0, filePath.Length - ext.Length ) + ".reconcile" + ext;
    }

    public static string GetLockPath( string syncRoot, string relativeFilePath ) =>
      Path.Combine( syncRoot, "__sync.locks", relativeFilePath ).Replace( "\\", "/" );

    public static LockFile Read( string syncRoot, string relativeFilePath ) {
      string lockPath = GetLockPath( syncRoot, relativeFilePath );
      LockFile lockFile = new LockFile() { filePath = lockPath };

      if( !File.Exists( lockPath ) ) {
        return lockFile;
      }

      foreach( string line in File.ReadAllLines( lockPath ) ) {
        string trimmed = line.Trim();
        if( string.IsNullOrEmpty( trimmed ) ) {
          continue;
        }

        string[] parts = trimmed.Split( ',' );
        if( parts.Length != 3 ) {
          continue;
        }

        lockFile.entries.Add( new LockEntry() {
          user = parts[0],
          lockDate = DateTime.ParseExact( parts[1], _dateFormat, CultureInfo.InvariantCulture ),
          unlockDate = DateTime.ParseExact( parts[2], _dateFormat, CultureInfo.InvariantCulture ),
        } );
      }

      return lockFile;
    }
  }
}
