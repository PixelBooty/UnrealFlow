using System;

namespace UnrealFlow{

  public class Bucket{
    public string apiKey;
    public string secret;
    public string name;

    public Amazon.S3.AmazonS3Client client => new Amazon.S3.AmazonS3Client( this.apiKey, this.secret, new Amazon.S3.AmazonS3Config() {
      ServiceURL = AppSettings.instance.serviceUri
    } );

  }

}
