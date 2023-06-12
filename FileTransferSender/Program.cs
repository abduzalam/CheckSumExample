using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace FileTransferSender
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"c:\\temp\\example.txt"; // Replace with your file path
             string ipAddress = "127.0.0.1";

            int port = 5000;

            try
            {
                using var client = new TcpClient(ipAddress, port);
                using var networkStream = client.GetStream();
                using var writer = new StreamWriter(networkStream, Encoding.UTF8);

                // Send file name
                writer.WriteLine(Path.GetFileName(filePath));
                writer.Flush();

                // Calculate and send MD5 hash
                using var md5 = MD5.Create();
                using var fileStream = File.OpenRead(filePath);
                byte[] hashBytes = md5.ComputeHash(fileStream);
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                writer.WriteLine(hashString);
                writer.Flush();

                // Send file content
                fileStream.Position = 0;
                fileStream.CopyTo(networkStream);
                networkStream.Flush();
                writer.Close(); // Close the StreamWriter
                networkStream.Close(); // Close the network stream

                Console.WriteLine("File transfer completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}