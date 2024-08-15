using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace UnrealFlow{

  class SyncSystem {
    public SyncSystem( Label statusLabel, string projectDirectory) {
      this._statusLabel = statusLabel;
      this._projectDirectory = projectDirectory.Replace( "\\", "/" );
    }

    private string _projectDirectory;
    private Label _statusLabel;

    private void _UpdateStatus( string status ) {
      this._statusLabel.Text = "Sync Status: " + status;
      this._SetRetry( 0 );
    }

    private bool _ValidFileName( FileInfo info ) =>
      !info.Name.StartsWith( "." ) && !info.Name.StartsWith( "_" );

    private bool _ValidDirectoryName( DirectoryInfo info ) =>
      !info.Name.StartsWith( "." ) && !info.Name.StartsWith( "_" );

    private IEnumerable<FileInfo> _GetBucketLocalFiles( string path ) {
      List<FileInfo> files = new List<FileInfo>();
      if( !Directory.Exists( path ) ){
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

    public async Task SyncBucket( ProjectSettings projectSettings ) {
      Bucket bucket = projectSettings.bucket;
      this._UpdateStatus( "Sync Started:\nGetting bucket file tree" );
      string[] paths = projectSettings.syncPaths.ToArray();
      //new string[] { "ImportsLarge", "Content/AssetsLarge", "Content/Megascans", "CarnalAssets" };

      IEnumerable<S3Object> bucketList = await this._GetBucketFileList( bucket.name, bucket.client );
      SyncTable syncTable = SyncTable.GetSyncTable( bucket.name );

      List<string> localFilePaths = new List<string>();
      List<string> updateList = new List<string>();
      List<string> deleteList = new List<string>();

      foreach( string path in paths ) {
        string folderPath = Path.Combine( this._projectDirectory, path );
        IEnumerable<FileInfo> localFiles = this._GetBucketLocalFiles( folderPath );
        foreach( FileInfo localFile in localFiles ) {
          this._UpdateStatus( $"Checking:\n{path}" );

          string filePath = localFile.FullName.Replace( "\\", "/" ).Replace( this._projectDirectory + "/", "" );
          localFilePaths.Add( filePath );

          if( !bucketList.Any( x => x.Key == filePath ) && syncTable.HasFile( filePath ) ) {
            syncTable.RemoveFile( filePath );
            File.Delete( localFile.FullName );
            syncTable.Save();
          }
          else {
            if( !syncTable.HasFile( filePath ) ) {
              //Newly created file//
              this._UpdateStatus( "Uploading New File:\n" + this._TrunkFileName( filePath ) );
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
            else if( syncTable.ModTime( filePath ) != localFile.LastWriteTimeUtc.ToUnixSeconds() ) {
              this._UpdateStatus( "Uploading File Update:\n" + this._TrunkFileName( filePath ) );
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

      this._UpdateStatus( "Validating Removed Files" );

      foreach( string filePath in syncTable.GetPathList() ) {
        if( !localFilePaths.Contains( filePath ) ) {
          await this._ExecuteRequest( async () => await bucket.client.DeleteObjectAsync( new DeleteObjectRequest() {
            BucketName = bucket.name,
            Key = filePath
          } ), 30 );
          deleteList.Add( filePath );
          syncTable.RemoveFile( filePath );
          syncTable.Save();
        }
      }

      this._UpdateStatus( "Validating Bucket Files" );

      foreach( S3Object bucketFile in bucketList ) {
        string filePath = bucketFile.Key;
        DateTime convertedBucketTime = bucketFile.LastModified;
        long fileModTime = syncTable.ModTime( filePath );
        if( !deleteList.Contains( filePath )
          && (
            !syncTable.HasFile( filePath )
            || ( fileModTime != convertedBucketTime.ToUnixSeconds() && !updateList.Contains( filePath ) )
          )
        ) {
          string absoluteFilePath = Path.Combine( this._projectDirectory, filePath );
          if( filePath.EndsWith("/") ) {
            Directory.CreateDirectory( ( new FileInfo( absoluteFilePath ) ).Directory.FullName );
          }
          else {
            this._UpdateStatus( "Downloading:\n" + this._TrunkFileName( filePath ) );
            long syncTimeHere = syncTable.ModTime( filePath );
            long bucketFileTime = bucketFile.LastModified.ToUnixSeconds();

            GetObjectResponse response = await this._ExecuteRequest(
              async () => await bucket.client.GetObjectAsync(
                new GetObjectRequest() {
                  BucketName = bucket.name,
                  Key = bucketFile.Key,
                }
              ), 3
            );
            Directory.CreateDirectory( ( new FileInfo( absoluteFilePath ) ).Directory.FullName );

            using( Stream inputStream = response.ResponseStream )
            using( FileStream fileStream = new FileStream( absoluteFilePath, FileMode.Create ) ) {
              inputStream.CopyTo( fileStream );
            }

            syncTable.SetModTime( filePath, convertedBucketTime.ToUnixSeconds() );
            File.SetLastWriteTimeUtc( absoluteFilePath, convertedBucketTime );
            syncTable.Save();
          }

        }
      }

      this._UpdateStatus( "Standby" );
    }
  }
}