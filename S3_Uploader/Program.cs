using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Transfer;


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
        private static List<string> fileList = new List<string>();
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2;
        private static long transferredBytesPrevious = 0;
        private static DateTime timePrevious;
        private static double percentCompleteCumulative = 0;


        private static IAmazonS3 client;


        static void Main(string[] args)
        {
            var awsCredentialsS3 =
                new Amazon.Runtime.BasicAWSCredentials("ACCESS KEY", "SECRET KEY");

            ReadFile();

            client = new AmazonS3Client(awsCredentialsS3, bucketRegion);
            Console.WriteLine(DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
            WritingAnObjectAsync().Wait();
        }

        private static void ReadFile()
        {
            using (StreamReader sr = new StreamReader("List.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string fileStringTemp = "";
                    fileStringTemp = sr.ReadLine();
                    Console.WriteLine(fileStringTemp);
                    fileList.Add(fileStringTemp);
                }
            }
        }

        static async Task WritingAnObjectAsync()
        {
            Console.WriteLine("Beginning Upload at: " +
                              DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
            int count = 0;
            string fileName;
            while (true)
            {
                fileName = fileList[count];
                try
                {
                    FileInfo file = new FileInfo(fileName);

                    var uploadRequest =
                        new TransferUtilityUploadRequest
                        {
                            BucketName = bucketName, FilePath = fileName, Key = file.Name, ContentType = "text/plain"
                        };
                    DateTime uploadStart = DateTime.Now;
                    uploadRequest.UploadProgressEvent +=
                        new EventHandler<UploadProgressArgs>
                            (uploadRequest_UploadPartProgressEvent);

                    var fileTransferUtility = new TransferUtility(client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                    DateTime uploadFinish = DateTime.Now;
                    TimeSpan timeDifference = uploadFinish - uploadStart;

                    Console.WriteLine("File " + file.Name + " successfully uploaded at: " +
                                      DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    Console.WriteLine("Time Taken: " + timeDifference.Minutes + " minutes, " + timeDifference.Seconds +
                                      "." + timeDifference.Milliseconds + "," + timeDifference.Microseconds + "," +
                                      timeDifference.Nanoseconds + " seconds");
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
                if (count + 1 > fileList.Count)
                {
                    break;
                }
            } //End of while loop

            Console.WriteLine("All files uploaded.");
        }

        static void uploadRequest_UploadPartProgressEvent(object sender, UploadProgressArgs e)
        {
            long transferredBytes = e.TransferredBytes;
            long totalBytes = e.TotalBytes;
            double transferredBytesDouble = (double)transferredBytes / 1000000.0;
            double totalBytesDouble = (double)totalBytes / 1000000.0;
            double percentComplete = (double)transferredBytes / (double)totalBytes;
            percentComplete *= 100; //Brings it from 0.7 to 70%

            //Speed Calculations
            DateTime currentTime = DateTime.Now;
            TimeSpan timeDifference = currentTime - timePrevious;
            //A tick is 100 nanoseconds (10,000 ticks in a millisecond)
            double timeDiffNumber = timeDifference.Ticks / (double)TimeSpan.TicksPerSecond;
            //Speed in MB/second
            double speed = ((double)(transferredBytes - transferredBytesPrevious)) / (timeDiffNumber * 10000000.0);

            //Save new transferred bytes amount to previous one once calculations for speed are complete
            transferredBytesPrevious = transferredBytes;
            timePrevious = currentTime;
            if (percentCompleteCumulative > 0.1)
            {
                Console.WriteLine(transferredBytesDouble.ToString("N1") + " MB / " + totalBytesDouble.ToString("N1") +
                                  " MB. " + percentComplete.ToString("N1") + "% complete. " + speed.ToString("N2") +
                                  "MB/sec");
            }
            else
            {
                percentCompleteCumulative += percentComplete;
            }
        }
    }
}