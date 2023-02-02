using System;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;

namespace CompressionFactory
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: compressionfactory.exe [-c | --compress] [-e | --encode] [-d | --decode] [-x | --extract] [inputFile] [outputFile]");
                Console.WriteLine("-c, --compress    Compresses inputFile data");
                Console.WriteLine("-e, --encode      Encodes inputFile data using Base64 encoding");
                Console.WriteLine("-d, --decode      Decodes inputFile data using Base64 decoding");
                Console.WriteLine("-x, --extract     Decompresses inputFile data");
                Console.WriteLine("-ce, --compress-encode    Compresses and encodes inputFile data");
                Console.WriteLine("-dx, --decode-extract     Decodes and decompresses inputFile data");
                Console.WriteLine("[inputFile]           The inputFile data, either a URL, file, or raw data");
                Console.WriteLine("[outputFile]          The outputFile file, if not specified outputFile will be sent to console");
                return;
            }

            string action = args[0];
            string inputFile = args[1];
            string outputFile = args.Length >= 3 ? args[2] : null;

            byte[] inputData;
            if (Uri.TryCreate(inputFile, UriKind.Absolute, out Uri uri))
            {
                // Input is a URL
                using (WebClient client = new WebClient())
                {
                    client.Proxy = WebRequest.GetSystemWebProxy();
                    inputData = client.DownloadData(uri);
                }
            }
            else if (File.Exists(inputFile))
            {
                // Input is a file
                inputData = File.ReadAllBytes(inputFile);
            }
            else
            {
                // Input is data
                inputData = Encoding.UTF8.GetBytes(inputFile);
            }

            byte[] outputData;
            switch (action)
            {
                case "-c":
                case "--compress":
                    outputData = Compress(inputData);
                    break;
                case "-e":
                case "--encode":
                    outputData = Encode(inputData);
                    break;
                case "-d":
                case "--decode":
                    outputData = Decode(inputData);
                    break;
                case "-x":
                case "--extract":
                    outputData = Decompress(inputData);
                    break;
                case "-ce":
                case "--compress-encode":
                    outputData = Encode(Compress(inputData));
                    break;
                case "-dx":
                case "--decode-extract":
                    outputData = Decompress(Decode(inputData));
                    break;
                default:
                    Console.WriteLine("Error: Invalid action");
                    return;
            }

            if (outputFile != null)
            {
                File.WriteAllBytes(outputFile, outputData);
            }
            else
            {
                Console.WriteLine(Encoding.UTF8.GetString(outputData));
            }

            static byte[] Encode(byte[] inputData)
            {
                return Encoding.UTF8.GetBytes(Convert.ToBase64String(inputData));
            }

            static byte[] Compress(byte[] inputData)
            {
                using (MemoryStream compressed = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(compressed, CompressionLevel.Optimal))
                    {
                        gzip.Write(inputData, 0, inputData.Length);
                    }

                    return compressed.ToArray();
                }
            }

            static byte[] Decode(byte[] inputData)
            {
                return Convert.FromBase64String(Encoding.UTF8.GetString(inputData));
            }

            static byte[] Decompress(byte[] inputData)
            {
                using (MemoryStream compressed = new MemoryStream(inputData))
                {
                    using (GZipStream gzip = new GZipStream(compressed, CompressionMode.Decompress))
                    {
                        using (MemoryStream decompressed = new MemoryStream())
                        {
                            gzip.CopyTo(decompressed);
                            return decompressed.ToArray();
                        }
                    }
                }
            }
        }
    }
}