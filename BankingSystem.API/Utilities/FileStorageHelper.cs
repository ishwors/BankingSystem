using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BankingSystem.API.Utilities
{
    public class FileStorageHelper
    {
        private readonly IConfiguration _configuration;

        public FileStorageHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
        {
            try
            {
                string bucketName = _configuration["AWS:BucketName"];
                string fileKey = "uploads/" + fileName;

                var awsCredentials = new BasicAWSCredentials(_configuration["AWS:IAMAccessKey"], _configuration["AWS:IAMSecretKey"]);
                var s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.USEast1);

                var bucketRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileKey,
                    InputStream = fileStream
                };

                PutObjectResponse response = await s3Client.PutObjectAsync(bucketRequest);

                return fileName;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
                return null;
            }
        }

        public async Task<string> GeneratePresignedUrlAsync(string fileName)
        {
            try
            {
                string bucketName = _configuration["AWS:BucketName"];
                string fileKey = "uploads/" + fileName;

                var awsCredentials = new BasicAWSCredentials(_configuration["AWS:IAMAccessKey"], _configuration["AWS:IAMSecretKey"]);
                var s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.USEast1);

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = fileKey,
                    Expires = DateTime.Now.AddMinutes(5) // Adjust expiration time as needed
                };

                string url = s3Client.GetPreSignedURL(request);
                return url;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when generating presigned URL", e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when generating presigned URL", e.Message);
                return null;
            }
        }
    }
}
