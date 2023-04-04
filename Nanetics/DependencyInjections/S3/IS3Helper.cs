using System;
using Amazon.S3;
using Amazon.S3.Model;
using Contracts.Media;

namespace Api.DependencyInjections.S3
{
	public interface IS3Helper
	{

        public Task ListBuckets(string userId);

        public Task ListObjectsV2(string bucketName, string businessId);
        public string GetPreSignedUrl(string bucketName, string keyName, string businessId);

        public Task GetObject(string bucketName, string key, string businessId);

    }
}

