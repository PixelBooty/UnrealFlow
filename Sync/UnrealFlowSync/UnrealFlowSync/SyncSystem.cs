using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;

namespace UnrealFlow {

  class SyncSystem {

    private JObject localSyncConfig = null;

    private void _UpdateStatus( string status ) {
      Console.WriteLine( status );
      this._SetRetry( 0 );
    }

    private bool _IgnoreFilePatternsTests( string fileName ) {
      if( this.localSyncConfig != null && this.localSyncConfig.ContainsKey( "ignore-files" ) ) {
        foreach( string pattern in this.localSyncConfig["ignore-files"] as JArray ) {
          if( ( new Regex( pattern ) ).IsMatch( fileName ) ) { 
            return true;
          }
        }
        return false;
      }
      return false;
    }

    private bool _IgnoreFolderPatternsTests( string fileName ) {
      if( this.localSyncConfig != null && this.localSyncConfig.ContainsKey( "ignore-folders" ) ) {
        foreach( string pattern in this.localSyncConfig["ignore-folders"] as JArray ) {
          if( ( new Regex( pattern ) ).IsMatch( fileName ) ) {
            return true;
          }
        }
        return false;
      }
      return false;
    }

    private bool _ValidFileName( FileInfo info ) =>
      ( !info.Name.StartsWith( "." ) && !info.Name.StartsWith( "_" ) && !info.Name.Contains( ".reconcile." ) && !this._IgnoreFilePatternsTests( info.Name ) ) || info.Name == "__sync-config.json";

    private bool _ValidDirectoryName( DirectoryInfo info ) =>
      ( !info.Name.StartsWith( "." ) && !info.Name.StartsWith( "_" ) && !this._IgnoreFolderPatternsTests( info.Name ) ) || info.Name == "__scripts";

    private IEnumerable<FileInfo> _GetBucketLocalFiles( string path ) {
      List<FileInfo> files = new List<FileInfo>();
      if( !Directory.Exists( path ) ) {
        Directory.CreateDirectory( path );
      }
      foreach( string fileName in Directory.GetFiles( path ) ) {
        FileInfo fileInfo = new FileInfo( fileName );
        if( this._ValidFileName( fileInfo ) ) {
          files.Add( fileInfo );
        }
      }
      foreach( string directoryName in Directory.GetDirectories( path ) ) {
        if( this._ValidDirectoryName( new DirectoryInfo( directoryName ) ) ) {
          files.AddRange( this._GetBucketLocalFiles( directoryName ) );
        }
      }

      return files;

    }

    private async Task<IEnumerable<S3Object>> _GetBucketFileList( string bucket, Amazon.S3.AmazonS3Client client ) {
      List<S3Object> s3FileList = new List<S3Object>();
      int timeout = 30;
      int retry = 0;
      ListObjectsV2Request listRequest = new ListObjectsV2Request() {
        BucketName = bucket
      };
      bool continueLoop = false;
      ListObjectsV2Response listResponse;
      do {
        continueLoop = false;
        try {
          listResponse = await client.ListObjectsV2Async( listRequest );
          foreach( S3Object fileObject in listResponse.S3Objects ) {
            s3FileList.Add( fileObject );
          }
          if( listResponse.IsTruncated ) {
            await Task.Delay( 250 );
          }

          if( s3FileList.Count > 0 ) {
            listRequest.StartAfter = s3FileList.Last().Key;
          }
          continueLoop = listResponse.IsTruncated;
        }
        catch( Exception ) {
          if( retry < timeout ) {
            retry++;
            this._SetRetry( retry );
            await Task.Delay( 10000 );
            continueLoop = true;
          }
          else {
            throw new Exception();
          }
        }

      } while( continueLoop );

      return s3FileList;
    }

    private string _TrunkFileName( string filePath ) =>
    filePath.Split( '/' ).Last().Substring( 0, filePath.Split( '/' ).Last().Count() > 50 ? 50 : filePath.Split( '/' ).Last().Count() );

    private async Task<T> _ExecuteRequest<T>( Func<Task<T>> requestAction, int retryCount ) where T : class {
      int retry = 0;
      while( retry < retryCount ) {
        try {
          return await requestAction() as T;
        }
        catch( Exception ) {
          await Task.Delay( 1000 );
          retry++;
          this._SetRetry( retry );
        }
      }

      if( retryCount <= 0 ) {
        throw new Exception( "Out of retry counts" );
      }

      return null;
    }

    private async Task _ExecuteRequest( Func<Task> requestAction, int retryCount ) {
      int retry = 0;
      while( retry < retryCount ) {
        try {
          await requestAction();
        }
        catch( Exception ) {
          await Task.Delay( 1000 );
          retry++;
          this._SetRetry( retry );
        }
      }

      if( retryCount <= 0 ) {
        throw new Exception( "Out of retry counts" );
      }
    }

    private void _SetRetry( int count ) {
      //this._statusLabel.Text = "Retry: " + count;
    }

    private async Task LocalFiles(
      string folderPath,
      List<string> localFilePaths,
      List<string> updateList,
      SyncTable syncTable,
      Bucket bucket,
      IEnumerable<S3Object> bucketList,
      SyncOverrides syncOverrides
    ) {
      IEnumerable<FileInfo> localFiles = this._GetBucketLocalFiles( folderPath );
      this._UpdateStatus( $"Checking:\n{folderPath}" );
      foreach( FileInfo localFile in localFiles ) {

        string filePath = localFile.FullName.Replace( "\\", "/" ).Replace( folderPath + "/", "" );
        localFilePaths.Add( filePath );

        if( !bucketList.Any( x => x.Key == filePath ) && syncTable.HasFile( filePath ) ) {
          if( syncOverrides.IsPushOnly( filePath ) ) {
            syncTable.RemoveFile( filePath );
            syncTable.Save();
          }
          else {
            syncTable.RemoveFile( filePath );
            File.Delete( localFile.FullName );
            syncTable.Save();
          }
        }
        else {
          LockFile lockFile = LockFile.Read( folderPath, filePath );
          string conflictUser = lockFile.LockedByOtherDuring( AppSettings.instance.user, localFile.LastWriteTimeUtc );
          if( conflictUser != null ) {
            this._UpdateStatus( "Conflict with lock by " + conflictUser + ", reconciling:\n" + this._TrunkFileName( filePath ) );
            string reconcilePath = Path.Combine( folderPath, LockFile.ToReconcilePath( filePath ) ).Replace( "\\", "/" );
            File.Move( localFile.FullName, reconcilePath, true );

            S3Object bucketFile = bucketList.FirstOrDefault( x => x.Key == filePath );
            if( bucketFile != null ) {
              GetObjectResponse response = await this._ExecuteRequest(
                async () => await bucket.client.GetObjectAsync(
                  new GetObjectRequest() {
                    BucketName = bucket.name,
                    Key = filePath,
                  }
                ), 3
              );
              using( Stream inputStream = response.ResponseStream )
              using( FileStream fileStream = new FileStream( localFile.FullName, FileMode.Create ) ) {
                await inputStream.CopyToAsync( fileStream );
              }
              DateTime modTime = bucketFile.LastModified.ToUniversalTime();
              syncTable.SetModTime( filePath, modTime.ToUnixSeconds() );
              File.SetLastWriteTimeUtc( localFile.FullName, modTime );
              syncTable.Save();
            }
          }
          else if( !syncTable.HasFile( filePath ) ) {
            if( !syncOverrides.IsPullOnly( filePath ) ) {
              //Newly created file//
              this._UpdateStatus( "Uploading New File: \n" + this._TrunkFileName( filePath ) + "\n" + filePath );
              await this._ExecuteRequest( async () => await bucket.client.PutObjectAsync( new PutObjectRequest() {
                BucketName = bucket.name,
                FilePath = localFile.FullName,
                Key = filePath,
              } ), 10 );
              DateTime modTime = (
                await this._ExecuteRequest(
                  async () => await bucket.client.GetObjectMetadataAsync(
                    new GetObjectMetadataRequest() {
                      BucketName = bucket.name,
                      Key = filePath,
                    } ), 50
                  )
              ).LastModified.ToUniversalTime();
              syncTable.SetModTime( filePath, modTime.ToUnixSeconds() );
              File.SetLastWriteTimeUtc( localFile.FullName, modTime );
              syncTable.Save();
            }
          }
          else if( Math.Abs( syncTable.ModTime( filePath ) - localFile.LastWriteTimeUtc.ToUnixSeconds() ) > 15 ) {
            if( !syncOverrides.IsPullOnly( filePath ) ) {
              this._UpdateStatus( "Uploading File Update:\n" + this._TrunkFileName( filePath ) );
              this._UpdateStatus( "File sync time " + syncTable.ModTime( filePath ) + " last write " + localFile.LastWriteTimeUtc.ToUnixSeconds() );
              await this._ExecuteRequest( async () => await bucket.client.PutObjectAsync( new PutObjectRequest() {
                BucketName = bucket.name,
                FilePath = localFile.FullName,
                Key = filePath
              } ), 30 );
              DateTime modTime = (
                await this._ExecuteRequest(
                  async () => await bucket.client.GetObjectMetadataAsync( new GetObjectMetadataRequest() {
                    BucketName = bucket.name,
                    Key = filePath,
                  } ), 50
                )
              ).LastModified.ToUniversalTime();

              syncTable.SetModTime( filePath, modTime.ToUnixSeconds() );
              File.SetLastWriteTimeUtc( localFile.FullName, modTime );
              updateList.Add( filePath );
              syncTable.Save();
            }
          }
        }
      }
    }

    private async Task _Deletes(
      List<string> localFilePaths,
      List<string> deleteList,
      SyncTable syncTable,
      Bucket bucket,
      SyncOverrides syncOverrides
    ) {
      foreach( string filePath in syncTable.GetPathList() ) {

        if( !localFilePaths.Contains( filePath ) ) {
          if( syncOverrides.IsPullOnly( filePath ) ) {
            syncTable.RemoveFile( filePath );
            syncTable.Save();
            continue;
          }
          Console.WriteLine( "Deleting remote file: " + filePath );
          await this._ExecuteRequest( async () => await bucket.client.DeleteObjectAsync( new DeleteObjectRequest() {
            BucketName = bucket.name,
            Key = filePath
          } ), 30 );
          deleteList.Add( filePath );
          syncTable.RemoveFile( filePath );
          syncTable.Save();
        }
      }
    }

  private async Task _Downloads(
    string folderPath,
    List<string> localFilePaths,
    List<string> updateList,
    List<string> deleteList,
    SyncTable syncTable,
    Bucket bucket,
    IEnumerable<S3Object> bucketList,
    SyncOverrides syncOverrides = null
  ) {
      // Semaphore to limit concurrent downloads to 10
      using( var semaphore = new SemaphoreSlim( 10, 10 ) ) {
        var downloadTasks = new List<Task>();

        foreach( S3Object bucketFile in bucketList ) {
          // Capture the bucketFile in a local variable for the async closure
          var file = bucketFile;

          var downloadTask = Task.Run( async () =>
          {
            // Wait for an available slot
            await semaphore.WaitAsync();

            try {
              string filePath = file.Key;

              if( syncOverrides != null && syncOverrides.IsPushOnly( filePath ) ) {
                return;
              }

              DateTime convertedBucketTime = file.LastModified;
              long fileModTime = syncTable.ModTime( filePath );

              if( !deleteList.Contains( filePath )
                && (
                  !syncTable.HasFile( filePath )
                  || ( fileModTime != convertedBucketTime.ToUnixSeconds() && !updateList.Contains( filePath ) )
                )
              ) {
                string absoluteFilePath = Path.Combine( folderPath, filePath );

                if( filePath.EndsWith( "/" ) ) {
                  Directory.CreateDirectory( ( new FileInfo( absoluteFilePath ) ).Directory.FullName );
                }
                else {
                  this._UpdateStatus( "Downloading:\n" + this._TrunkFileName( filePath ) );
                  long syncTimeHere = syncTable.ModTime( filePath );
                  long bucketFileTime = file.LastModified.ToUnixSeconds();

                  if( syncOverrides != null && syncOverrides.IsPullOnly( filePath )
                    && File.Exists( absoluteFilePath ) && syncTable.HasFile( filePath )
                  ) {
                    long localModTime = new FileInfo( absoluteFilePath ).LastWriteTimeUtc.ToUnixSeconds();
                    if( Math.Abs( syncTimeHere - localModTime ) > 15 ) {
                      string reconcilePath = Path.Combine( folderPath, LockFile.ToReconcilePath( filePath ) ).Replace( "\\", "/" );
                      Directory.CreateDirectory( ( new FileInfo( reconcilePath ) ).Directory.FullName );
                      File.Copy( absoluteFilePath, reconcilePath, true );
                      this._UpdateStatus( "Pull-only reconcile backup:\n" + this._TrunkFileName( filePath ) );
                    }
                  }

                  LockFile lockFile = LockFile.Read( folderPath, filePath );
                  bool isConflict = lockFile.HasConflict( AppSettings.instance.user, convertedBucketTime );
                  string savePath = isConflict
                    ? Path.Combine( folderPath, LockFile.ToReconcilePath( filePath ) )
                    : absoluteFilePath;

                  if( isConflict ) {
                    this._UpdateStatus( "Conflict detected, saving as reconcile:\n" + this._TrunkFileName( filePath ) );
                  }

                  GetObjectResponse response = await this._ExecuteRequest(
                    async () => await bucket.client.GetObjectAsync(
                      new GetObjectRequest() {
                        BucketName = bucket.name,
                        Key = file.Key,
                      }
                    ), 3
                  );

                  Directory.CreateDirectory( ( new FileInfo( savePath ) ).Directory.FullName );

                  using( Stream inputStream = response.ResponseStream )
                  using( FileStream fileStream = new FileStream( savePath, FileMode.Create ) ) {
                    await inputStream.CopyToAsync( fileStream );
                  }

                  // Use a lock to ensure thread-safe operations on syncTable
                  lock( syncTable ) {
                    syncTable.SetModTime( filePath, convertedBucketTime.ToUnixSeconds() );
                    syncTable.Save();
                  }

                  File.SetLastWriteTimeUtc( savePath, convertedBucketTime );
                }
              }
            }
            finally {
              // Release the semaphore slot
              semaphore.Release();
            }
          } );

          downloadTasks.Add( downloadTask );
        }

        // Wait for all downloads to complete
        await Task.WhenAll( downloadTasks );
      }
    }

    private void _LoadLocalSyncSettings( string path ) {
      if( File.Exists( Path.Combine( path, "__sync-config.json" ) ) ) {
        this.localSyncConfig = JObject.Parse( File.ReadAllText( Path.Combine( path, "__sync-config.json" ) ) );
      }
    }

    public async Task SyncFolder( FolderSettings folderSettings ) {
      string syncPath = folderSettings.folderPath.Replace( "\\", "/" );
      Bucket bucket = folderSettings.bucket;
      this._LoadLocalSyncSettings( syncPath );
      this._UpdateStatus( "Sync Started:\nGetting bucket file tree for '" + AppSettings.instance.serviceUri + "'" );
      IEnumerable<S3Object> fullBucketList = await this._GetBucketFileList( bucket.name, bucket.client );
      SyncTable syncTable = SyncTable.GetSyncTable( bucket.name );

      // Step 1: Sync locks first
      this._UpdateStatus( "Syncing locks" );
      List<string> lockFilePaths = await this._SyncLocksInternal( syncPath, bucket, fullBucketList, syncTable );

      // Step 2: Sync everything else (excluding locks)
      IEnumerable<S3Object> bucketList = fullBucketList.Where( x => !x.Key.StartsWith( "__sync.locks/" ) );

      List<string> localFilePaths = new List<string>( lockFilePaths );
      List<string> updateList = new List<string>();
      List<string> deleteList = new List<string>();

      SyncOverrides syncOverrides = folderSettings.syncOverrides;

      await this.LocalFiles( syncPath, localFilePaths, updateList, syncTable, bucket, bucketList, syncOverrides );

      this._UpdateStatus( "Validating Removed Files" );

      await this._Deletes( localFilePaths, deleteList, syncTable, bucket, syncOverrides );

      this._UpdateStatus( "Validating Bucket Files" );

      await this._Downloads( syncPath, localFilePaths, updateList, deleteList, syncTable, bucket, bucketList, syncOverrides );

      this._UpdateStatus( "Standby" );

    }

    public async Task SyncBucket( ProjectSettings projectSettings ) {
      Bucket bucket = projectSettings.bucket;
      this._UpdateStatus( "Sync Started:\nGetting bucket file tree for '" + AppSettings.instance.serviceUri + "'" );
      string path = projectSettings.projectSyncFolder;
      this._LoadLocalSyncSettings( Path.Combine( projectSettings.projectPath, path ) );
      //new string[] { "ImportsLarge", "Content/AssetsLarge", "Content/Megascans", "CarnalAssets" };

      IEnumerable<S3Object> fullBucketList = await this._GetBucketFileList( bucket.name, bucket.client );
      SyncTable syncTable = SyncTable.GetSyncTable( bucket.name );

      string folderPath = Path.Combine( projectSettings.projectPath.Replace( "\\", "/" ), path ).Replace( "\\", "/" );

      // Step 1: Sync locks first
      this._UpdateStatus( "Syncing locks" );
      List<string> lockFilePaths = await this._SyncLocksInternal( folderPath, bucket, fullBucketList, syncTable );

      // Step 2: Sync everything else (excluding locks)
      IEnumerable<S3Object> bucketList = fullBucketList.Where( x => !x.Key.StartsWith( "__sync.locks/" ) );

      List<string> localFilePaths = new List<string>( lockFilePaths );
      List<string> updateList = new List<string>();
      List<string> deleteList = new List<string>();

      SyncOverrides syncOverrides = projectSettings.syncOverrides;

      await this.LocalFiles( folderPath, localFilePaths, updateList, syncTable, bucket, bucketList, syncOverrides );

      this._UpdateStatus( "Validating Removed Files" );

      await this._Deletes( localFilePaths, deleteList, syncTable, bucket, syncOverrides );

      this._UpdateStatus( "Validating Bucket Files" );

      await this._Downloads( folderPath, localFilePaths, updateList, deleteList, syncTable, bucket, bucketList, syncOverrides );

      this._UpdateStatus( "Standby" );
    }

    private async Task<List<string>> _SyncLocksInternal(
      string folderPath,
      Bucket bucket,
      IEnumerable<S3Object> fullBucketList,
      SyncTable syncTable
    ) {
      string locksPrefix = "__sync.locks/";
      string locksDir = Path.Combine( folderPath, "__sync.locks" ).Replace( "\\", "/" );
      IEnumerable<S3Object> bucketList = fullBucketList.Where( x => x.Key.StartsWith( locksPrefix ) );

      List<string> localFilePaths = new List<string>();
      List<string> updateList = new List<string>();
      List<string> deleteList = new List<string>();

      // Upload/check local lock files only
      if( Directory.Exists( locksDir ) ) {
        IEnumerable<FileInfo> localFiles = this._GetBucketLocalFiles( locksDir );
        foreach( FileInfo localFile in localFiles ) {
          string filePath = localFile.FullName.Replace( "\\", "/" ).Replace( folderPath + "/", "" );
          localFilePaths.Add( filePath );

          if( !bucketList.Any( x => x.Key == filePath ) && syncTable.HasFile( filePath ) ) {
            syncTable.RemoveFile( filePath );
            File.Delete( localFile.FullName );
            syncTable.Save();
          }
          else {
            if( !syncTable.HasFile( filePath ) ) {
              this._UpdateStatus( "Uploading New Lock:\n" + this._TrunkFileName( filePath ) );
              await this._ExecuteRequest( async () => await bucket.client.PutObjectAsync( new PutObjectRequest() {
                BucketName = bucket.name,
                FilePath = localFile.FullName,
                Key = filePath,
              } ), 10 );
              DateTime modTime = (
                await this._ExecuteRequest(
                  async () => await bucket.client.GetObjectMetadataAsync(
                    new GetObjectMetadataRequest() {
                      BucketName = bucket.name,
                      Key = filePath,
                    } ), 50
                  )
              ).LastModified.ToUniversalTime();
              syncTable.SetModTime( filePath, modTime.ToUnixSeconds() );
              File.SetLastWriteTimeUtc( localFile.FullName, modTime );
              syncTable.Save();
            }
            else if( Math.Abs( syncTable.ModTime( filePath ) - localFile.LastWriteTimeUtc.ToUnixSeconds() ) > 15 ) {
              this._UpdateStatus( "Uploading Lock Update:\n" + this._TrunkFileName( filePath ) );
              await this._ExecuteRequest( async () => await bucket.client.PutObjectAsync( new PutObjectRequest() {
                BucketName = bucket.name,
                FilePath = localFile.FullName,
                Key = filePath
              } ), 30 );
              DateTime modTime = (
                await this._ExecuteRequest(
                  async () => await bucket.client.GetObjectMetadataAsync( new GetObjectMetadataRequest() {
                    BucketName = bucket.name,
                    Key = filePath,
                  } ), 50
                )
              ).LastModified.ToUniversalTime();
              syncTable.SetModTime( filePath, modTime.ToUnixSeconds() );
              File.SetLastWriteTimeUtc( localFile.FullName, modTime );
              updateList.Add( filePath );
              syncTable.Save();
            }
          }
        }
      }

      // Only delete remote files within __sync.locks/ that no longer exist locally
      foreach( string filePath in syncTable.GetPathList() ) {
        if( filePath.StartsWith( locksPrefix ) && !localFilePaths.Contains( filePath ) ) {
          Console.WriteLine( "Deleting remote lock: " + filePath );
          await this._ExecuteRequest( async () => await bucket.client.DeleteObjectAsync( new DeleteObjectRequest() {
            BucketName = bucket.name,
            Key = filePath
          } ), 30 );
          deleteList.Add( filePath );
          syncTable.RemoveFile( filePath );
          syncTable.Save();
        }
      }

      // Download new/updated lock files (bucketList is already filtered to locks only)
      await this._Downloads( folderPath, localFilePaths, updateList, deleteList, syncTable, bucket, bucketList );

      this._UpdateStatus( "Locks synced" );
      return localFilePaths;
    }

    public async Task SyncLocks( ProjectSettings projectSettings ) {
      Bucket bucket = projectSettings.bucket;
      string path = projectSettings.projectSyncFolder;
      string folderPath = Path.Combine( projectSettings.projectPath.Replace( "\\", "/" ), path ).Replace( "\\", "/" );

      this._UpdateStatus( "Syncing locks" );

      IEnumerable<S3Object> fullBucketList = await this._GetBucketFileList( bucket.name, bucket.client );
      SyncTable syncTable = SyncTable.GetSyncTable( bucket.name );

      await this._SyncLocksInternal( folderPath, bucket, fullBucketList, syncTable );
    }
  }
}