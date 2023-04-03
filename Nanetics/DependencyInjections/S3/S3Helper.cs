using System;
using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Contracts.Media;
using Microsoft.Identity.Client;
using SilverMenu.DependencyInjections.Azure;

namespace SilverMenu.DependencyInjections.S3
{
    //https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/S3/MIS3GetPreSignedURLGetPreSignedUrlRequest.html
    /// <summary>
    /// https://developers.cloudflare.com/r2/examples/aws/aws-sdk-net/
    /// </summary>
	public class S3Helper : IS3Helper
    {

        private readonly IConfiguration _config;
        private readonly IKeyVaultRetriever keyVaultRetriever;
        private readonly SecretClient _secretClient;
        private readonly AmazonS3Client _s3Client;
        private readonly TransferUtility _transferUtility;
        public S3Helper(IConfiguration config, IKeyVaultRetriever keyVaultRetriever)
        {
            
            //string cloudFlareApiSecret = _secretClient.GetSecret("rainierApps-r2-97b0e4ac-da4f-4598-9080-93424ad673e1").Value.Value;

            //these are api access key and secret key, respectively
            var credentials = new BasicAWSCredentials("0d846fd34d856c0e52ec20248d695ec5",
                "faa908ebae9a0eda32c307f7948a8d9e4fd360f10819ca0e5783142782817ee8");


            //0d846fd34d856c0e52ec20248d695ec5

            //https://test-bucket.e208a3abb92e2b21d902b46df0483dbc.r2.cloudflarestorage.com/seo.png?X-Amz-Expires=2700&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=1d29eb547b315114b8b8b757872ef3e5/20230330/us-east-1/s3/aws4_request&X-Amz-Date=20230330T211700Z&X-Amz-SignedHeaders=host&X-Amz-Signature=18f54fccd61a5f05d7e80efaabb6c16ecd71532cf3df903c95cc9d87f27ed485
            _s3Client = new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = $"https://e208a3abb92e2b21d902b46df0483dbc.r2.cloudflarestorage.com",
               
                

            });

            _transferUtility = new TransferUtility(_s3Client);
            _config = config;
            this.keyVaultRetriever = keyVaultRetriever;
        }

        public async Task ListBuckets(string businessId)
        {
            var response = await _s3Client.ListBucketsAsync();

            foreach (var s3Bucket in response.Buckets)
            {
                Console.WriteLine("{0}", s3Bucket.BucketName);
            }
        }

        public async Task ListObjectsV2(string bucketName, string businessId)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            var response = await _s3Client.ListObjectsV2Async(request);

            foreach (var s3Object in response.S3Objects)
            {
                Console.WriteLine("{0}", s3Object.Key);
            }
        }
        public string GetPreSignedUrl(string bucketName, string keyName, string businessId)
        {

            AWSConfigsS3.UseSignatureVersion4 = true;
            var request = new GetPreSignedUrlRequest
            {
                BucketName = "test-bucket",
                Key = keyName,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(45),
               
            };

            var url = _s3Client.GetPreSignedURL(request);
           // var contents = GetContents(url);
            return url;
        }
      
        // ETag: "186a71ee365d9686c3b98b6976e1f196"
        public async Task GetObject(string bucketName, string key, string businessId)
        {

            GetObjectResponse response = await _s3Client.GetObjectAsync(bucketName, key);

            Console.WriteLine("ETag: {0}", response.ETag);
        }
        // ETag: "186a71ee365d9686c3b98b6976e1f196"
    }

    // dog.png
    // cat.png
}

