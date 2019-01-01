//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.IO;
using System.ServiceModel.Channels;

namespace Microsoft.Samples.Mtom
{
    //The service contract is defined in generatedClient.cs, generated from the service by the svcutil tool.

    //Client implementation code.
    class Client
    {
        //static void PrepareUpload(string filePath) {
        //    filePath = @"C:\Users\AsifAli\Pictures\file.png";
        //    const int chunkSize = 25;
        //    byte[] bytes = null;
        //    int startIndex, endIndex, length, totalChunks;
        //    //using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        //    //{
        //    //    //// Calculate total chunks to be sent to service
        //    //    totalChunks = (int)Math.Ceiling((double)fs.Length / chunkSize);

        //    //    //// Set up Upload request object
        //    //    UploaderCleint.UploaderClient client = new UploaderCleint.UploaderClient();
        //    //    // uploadRequestInfo.FileByteStream = fs;

        //    //    for (int i = 0; i < totalChunks; i++)
        //    //    {
        //    //        startIndex = i * chunkSize;
        //    //        endIndex = (int)(startIndex + chunkSize > fs.Length ? fs.Length : startIndex + chunkSize);
        //    //        length = endIndex - startIndex;
        //    //        bytes = new byte[length];

        //    //        //// Read bytes from file, and send upload request
        //    //        fs.Read(bytes, 0, bytes.Length);
        //    //        client.Upload(fs,fs.Length);
        //    //    }


        //    //}
        //}
        static void PrepareWithSimpleParams()
        {
            string filePath = @"C:\Users\AsifAli\Pictures\file.pdf";
            const int BUFFERSIZE = 4*1024;

            FileInfo objFIleInfo = new FileInfo(filePath);
            long totalFileSize = objFIleInfo.Length;
            long byteToRead = objFIleInfo.Length;
            int transferredBytes = 0;
            bool fileUploaded = false;
            FileUploaderService2.Service1Client objRemoteFile = new FileUploaderService2.Service1Client();
            using (objRemoteFile as IDisposable)
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    while (byteToRead > 0)
                    {
                       byte[] buffer = new byte[BUFFERSIZE];
                        int n = fs.Read(buffer, 0, BUFFERSIZE);
                        fileUploaded = objRemoteFile.UploadFile(objFIleInfo.Name, "101", objFIleInfo.Length, buffer);
                        transferredBytes += n;
                        byteToRead -= n;
                    }
                }
            }
            if (fileUploaded)
                Console.WriteLine("Completed");
        }
        static void PrepareStreamUpload()
        {
            FileUploaderService3.Service1Client objClient = new FileUploaderService3.Service1Client();

            using (objClient as IDisposable)
            {
                string filePath = @"C:\Users\AsifAli\Pictures\file.pdf";
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    FileUploaderService3.RemoteFileInfo objRemoteFile = new FileUploaderService3.RemoteFileInfo();
                    objRemoteFile.Data = fs;
                    objRemoteFile.FileName = Path.GetFileName(filePath);
                    objClient.Upload(Path.GetFileName(filePath),fs);
                }
            }

        }
        static void PrepareBufferTransfer()
        {
            string filePath = @"C:\Users\AsifAli\Pictures\file.pdf";
            const int BUFFERSIZE = 4096;
                   
            FileInfo objFIleInfo = new FileInfo(filePath);
            long totalFileSize = objFIleInfo.Length;
            long byteToRead = objFIleInfo.Length;
            int transferredBytes = 0;
            FileUploaderService.UploaderClient objClient = new FileUploaderService.UploaderClient();
            using (objClient as IDisposable)
            {


                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    while (byteToRead>0)
                    {
                        FileUploaderService.RemoteFileInfo objRemoteFile = new FileUploaderService.RemoteFileInfo();
                        byte[] buffer = new byte[BUFFERSIZE];
                        int n=fs.Read(buffer, 0, BUFFERSIZE);
                        objClient.Upload(Path.GetFileName(filePath), buffer);
                        transferredBytes += n;
                        byteToRead -= n;
                    }
                }
            }

        }
        static void Main()
        {
            DateTime dt1 = DateTime.Now;
            DateTime dt2 = DateTime.Now;
            //PrepareBufferTransfer();
            dt1 = DateTime.Now;
            //PrepareWithSimpleParams();
            PrepareBufferTransfer();
            //dt2 = DateTime.Now;
            //Console.WriteLine(dt2.Subtract(dt1).ToString());
            //dt1 = DateTime.Now;
            //PrepareWithSimpleParams();
            //dt2 = DateTime.Now;
            //Console.WriteLine(dt2.Subtract(dt1).ToString());
           
            //PrepareStreamUpload();
            dt2 = DateTime.Now;
            Console.WriteLine(dt2.Subtract(dt1).ToString());
            Console.ReadLine();
            return;

            // Upload a stream of 1000 bytes
            int bufferSize = 4000;
            byte[] binaryData = new byte[bufferSize];
            MemoryStream stream = new MemoryStream(binaryData);

            //UploadClient2.UploadClient client = new UploadClient2.UploadClient();
            //Console.WriteLine(client.Upload(stream));
            Console.WriteLine();
            stream.Close();

            // Compare the wire representations of messages with different payloads
            CompareMessageSize(100);
            CompareMessageSize(1000);
            CompareMessageSize(10000);
            CompareMessageSize(100000);
            CompareMessageSize(1000000);

            Console.WriteLine();
            Console.WriteLine("Press <ENTER> to terminate client.");
            Console.ReadLine();
        }

        static void CompareMessageSize(int dataSize)
        {
            // Create and buffer a message with a binary payload
            byte[] binaryData = new byte[dataSize];
            Message message = Message.CreateMessage(MessageVersion.Soap12WSAddressing10, "action", binaryData);
            MessageBuffer buffer = message.CreateBufferedCopy(int.MaxValue);

            // Print the size of a text encoded copy
            int size = SizeOfTextMessage(buffer.CreateMessage());
            Console.WriteLine("Text encoding with a {0} byte payload: {1}", binaryData.Length, size);

            // Print the size of an MTOM encoded copy
            size = SizeOfMtomMessage(buffer.CreateMessage());
            Console.WriteLine("MTOM encoding with a {0} byte payload: {1}", binaryData.Length, size);

            Console.WriteLine();
            message.Close();
        }

        static int SizeOfTextMessage(Message message)
        {
            // Create a text encoder
            MessageEncodingBindingElement element = new TextMessageEncodingBindingElement();
            MessageEncoderFactory factory = element.CreateMessageEncoderFactory();
            MessageEncoder encoder = factory.Encoder;

            // Write the message and return its length
            MemoryStream stream = new MemoryStream();
            encoder.WriteMessage(message, stream);
            int size = (int)stream.Length;

            message.Close();
            stream.Close();
            return size;
        }

        static int SizeOfMtomMessage(Message message)
        {
            // Create an MTOM encoder
            MessageEncodingBindingElement element = new MtomMessageEncodingBindingElement();
            MessageEncoderFactory factory = element.CreateMessageEncoderFactory();
            MessageEncoder encoder = factory.Encoder;

            // Write the message and return its length
            MemoryStream stream = new MemoryStream();
            encoder.WriteMessage(message, stream);
            int size = (int)stream.Length;

            stream.Close();
            message.Close();
            return size;
        }
    }
}
