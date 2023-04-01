# AWS_S3_Uploader
Uploads file to an AWS S3 storage bucket.

You need to have an IAM user with a full S3 access (AmazonS3FullAccess) policy. There might be a more restricted one that's better suited to this, but this policy works fine.
Put the access key and secret key in the correct location.

To do this, go to [account name] (in the upper right corner) -> "Security Credentials". 
Then click "Create Access Key", and ignore the warning pop-up about how doing this doesn't follow best practices.
From here, simply copy the access and secret key into the program

Add the files (with their full paths) to a "List.txt" file in the same folder as the .exe file. Each file should be seperated with a new line. You could also set a full path in the code to the text file. 
From there it reads the text file, saves all the files to a list, and enters the part where they are uploaded.
Here it names the object being uploaded as the filename (logically), and uploads it. I set mine up to upload the the glacier deep archive type, but have commented out that line.

It outputs the current progress and speed of the file being upload in this format: `2.0 MB / 8.4 MB. 24.0% complete. 8.87MB/sec`

If it fails to upload for some reason, it'll try to upload the same file repeatably until it succeeds or is manually stopped.
If it succeeds it'll continue onto the next file.
There are outputs showing if the upload was successful or not, with timestamps.

I have no idea how secure this approach to uploading files is, but it works for me. If this program causes issues for you, you have only yourself to blame for following some random script you found on Github.
