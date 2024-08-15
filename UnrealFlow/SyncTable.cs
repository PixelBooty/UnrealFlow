using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace UnrealFlow{

  public class SyncTable {
    struct SyncItem {
      public string path;
      public long lastModified;
    }

    public static SyncTable GetSyncTable( string bucket ) {
      if( !SyncTable._tableList.ContainsKey( bucket ) ) {
        SyncTable._tableList.Add( bucket, new SyncTable( bucket ) );
      }

      return SyncTable._tableList[bucket];
    }

    public static void RemoveSyncTable( string bucket ) {
      if( SyncTable._tableList.ContainsKey( bucket ) ) {
        SyncTable._tableList[bucket]._Cleanup();
        SyncTable._tableList.Remove( bucket );
      }
    }

    private static Dictionary<string, SyncTable> _tableList = new Dictionary<string, SyncTable>();

    private string _fileLocation => Path.Combine( FileSystem.AppDataDirectory, this._bucket + "-sync-table.json" );

    public SyncTable( string bucket ) {
      this._bucket = bucket;
      this._Load();
    }

    public static void CleanAll() {
      foreach( string key in SyncTable._tableList.Keys.ToArray() ) {
        SyncTable.RemoveSyncTable( key );
      }
    }

    private void _Cleanup() {
      if( File.Exists( this._fileLocation ) ) {
        File.Delete( this._fileLocation );
      }
    }

    private readonly string _bucket;

    private void _Load() {
      this._syncList.Clear();
      if( File.Exists( this._fileLocation ) ) {
        try {
          JArray loadedTable = JArray.Parse( File.ReadAllText( this._fileLocation ) );
          foreach( JObject file in loadedTable ) {
            this._syncList.Add( new SyncItem() {
              path = (string)file["path"],
              lastModified = (long)file["lastMod"],
            } );
          }
        }
        catch( Exception ) { }
      }
      else {
        this.Save();
      }
    }

    public void SetModTime( string filePath, long lastModified ) {
      int syncItemIndex = this._syncList.FindIndex( x => x.path == filePath );
      if( syncItemIndex == -1 ) {
        syncItemIndex = this._syncList.Count;
        this._syncList.Add( new SyncItem() );
      }
      this._syncList[syncItemIndex] = new SyncItem() { path = filePath, lastModified = lastModified };
    }

    public void RemoveFile( string filePath ) {
      int syncItemIndex = this._syncList.FindIndex( x => x.path == filePath );
      if( syncItemIndex != -1 ) {
        this._syncList.RemoveAt( syncItemIndex );
      }
    }

    public long ModTime( string filePath ) {
      int syncItemIndex = this._syncList.FindIndex( x => x.path == filePath );
      if( syncItemIndex != -1 ) {
        return this._syncList[syncItemIndex].lastModified;
      }

      return DateTime.Now.ToUnixSeconds();
    }

    public bool HasFile( string filePath ) {
      int syncItemIndex = this._syncList.FindIndex( x => x.path == filePath );
      if( syncItemIndex != -1 ) {
        return true;
      }

      return false;
    }

    public string[] GetPathList() =>
      this._syncList.Select( x => x.path ).ToArray();

    public void Save() => File.WriteAllText( this._fileLocation, this.ToString() );

    public override string ToString() => new JArray( this._syncList.Select( x => new JObject() {
      ["path"] = x.path,
      ["lastMod"] = x.lastModified,
    } ).ToArray() ).ToString();

    //By bucket list of files that existed since last sync to figure out which ones need to be patched, deleted, or created.
    private readonly List<SyncItem> _syncList = new List<SyncItem>();
  }

}
