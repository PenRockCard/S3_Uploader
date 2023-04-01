using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


/*
 * Uploads files to AWS S3 buckets.
 * Might have a bunch of security flaws, etc, but it works as a simple uploader.
 * A file list (full paths) must be saved as List.txt within the folder containing the .exe file
 */

namespace ConsoleApp1
{
    class Program
    {

        private static string bucketName = "photos-glacier-deep";
        private static List<string> fileList =new List<string>();
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2;
        
        
        private static IAmazonS3 client;
        
        static void Main(string[] args)
        {
            var awsCredentialsS3 = 
                new Amazon.Runtime.BasicAWSCredentials("ACCESS KEY", "SECRET KEY");

            ReadFile();
            
            client = new AmazonS3Client(awsCredentialsS3,bucketRegion);
            Console.WriteLine(DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
            //Console.WriteLine("Hello World!");
           WritingAnObjectAsync().Wait();
        }

        private static void ReadFile()
        {
            string fileStringTemp;
            
            using (StreamReader sr = new StreamReader("List.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    fileStringTemp = sr.ReadLine();
                    Console.WriteLine(fileStringTemp);
                    fileList.Add(fileStringTemp);
                }
            }
        }
        
        static async Task WritingAnObjectAsync()
        {
            Console.WriteLine("Beginning Upload at: "+DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
            int count = 0;
            string fileName;
            while (true)
            {
                fileName = fileList[count];
                try
                {
                    FileInfo file = new FileInfo(fileName);

                    var putRequest2 = new PutObjectRequest
                    {
                        BucketName = bucketName
                        ,Key = file.Name
                        ,FilePath = fileName
                        ,ContentType = "text/plain"
                        //,StorageClass = S3StorageClass.DeepArchive //Set up to use the DeepArchive storage class, change as needed
                    };

                    putRequest2.Metadata.Add("x-amz-meta-title", "someTitle");
                    PutObjectResponse response2 = await client.PutObjectAsync(putRequest2);
                    Console.WriteLine("File " + file.Name + " successfully uploaded at: "+DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                catch (AmazonS3Exception e)
                {
                    Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
                    Console.WriteLine(DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        "Unknown encountered on server. Message:'{0}' when writing an object"
                        , e.Message);
                    Console.WriteLine(DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    continue;
                }

                count++;
                if (count+1 > fileList.Count)
                {
                    break;
                }
            } //End of while loop
            Console.WriteLine("All files uploaded.");
        }
        
    }
}