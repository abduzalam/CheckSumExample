using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace FileTransferReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 5000;
            TcpListener listener = null;

            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine($"Listening on port {port}...");

                using var client = listener.AcceptTcpClient();
                using var networkStream = client.GetStream();
                using var reader = new StreamReader(networkStream, Encoding.UTF8);

                // Receive file name
                string fileName = reader.ReadLine();

                // Receive MD5 hash
                string receivedHash = reader.ReadLine();

                // Receive and save file content
                string receivedFilePath = Path.Combine(Path.GetTempPath(), fileName);
              //  string receivedFilePath = Path.Combine(@"c:\temp", fileName);
                using var fileStream = File.Create(receivedFilePath);


                // Intentionally modify the received file to cause a checksum mismatch
                fileStream.Seek(0, SeekOrigin.Begin);
                int firstByte = fileStream.ReadByte();
                int modifiedByte = firstByte ^ 0xFF;
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.WriteByte((byte)modifiedByte);
                fileStream.Flush();
                // modification ends here


                networkStream.CopyTo(fileStream);
                fileStream.Close(); // Close the file stream
                // Calculate MD5 hash of the received file
                using var md5 = MD5.Create();
                using var receivedFileStream = File.OpenRead(receivedFilePath);
                byte[] hashBytes = md5.ComputeHash(receivedFileStream);
                string calculatedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                // Compare hashes
                if (receivedHash == calculatedHash)
                {
                    Console.WriteLine($"File transfer successful. File saved at: {receivedFilePath}");
                }
                else
                {
                    Console.WriteLine("File transfer failed. Checksum mismatch.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
               
            }
            finally
            {
                // Close the listener
                listener?.Stop();
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        public static void WriteToLog(string logMessage)
        {
            string path = @"c:\temp\log.txt";
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(logMessage);
            }
        }
    }
}