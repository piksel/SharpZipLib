using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;

namespace ICSharpCode.SharpZipLib.ReproTester.Repros
{
	[Timeout(minutes: 2)]
	internal class Repro218 : Repro
	{
		string inputFileA, inputFileB;

		static readonly string[] SizeSuffixes =
		  { "byte", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		static string SizeSuffix(Int64 value, int decimalPlaces = 1, bool iec = false)
		{
			var factor = iec ? 1024 : 1000;
			decimal dValue = Math.Abs(value);
			var i = 0;
			for (;dValue >= factor; i++)
				dValue /= factor;

			if (value < 0)
				dValue = -dValue;

			var sb = new StringBuilder().Append(Math.Round(dValue, decimalPlaces));
			if(i>0)
			{
				sb.Append(SizeSuffixes[i][0]);
				if(iec) sb.Append('i');
				sb.Append(SizeSuffixes[i][1]);
			}
			else
			{
				sb.Append(SizeSuffixes[i]);
			}

			return sb.ToString();
		}


		public Repro218()
		{
			inputFileA = GetSampleInput("_GameMeshTri1_color.blob", "blob");
			inputFileB = GetSampleInput("_GameMeshTri1_color.png", "png");
		}

		internal override void Run()
		{
			Console.WriteLine("Good file:");
			CompressFile(inputFileB);

			Console.WriteLine();

			Console.WriteLine("Bad file:");
			CompressFile(inputFileA);
		}

		internal void CompressFile(string file)
		{
			Console.WriteLine($"Input file: {Path.GetFileName(file)}");

			byte[] blob_data;
			using (FileStream fs = new FileStream(file, FileMode.Open))
			{
				blob_data = new byte[fs.Length];
				fs.Read(blob_data, 0, (int)fs.Length);
			}
			Console.WriteLine($"Input size {SizeSuffix(blob_data.Length)}");

			byte[] storage_data;
			using (MemoryStream sourceStream = new MemoryStream(blob_data))
			using (MemoryStream compressStream = new MemoryStream())
			{
				Console.WriteLine("Compressing... ");
				BZip2.BZip2.Compress(sourceStream, compressStream, true, 9);

				Console.WriteLine("Fetching compressed bytes... ");
				storage_data = compressStream.ToArray();
			}
			Console.WriteLine($"Compressed size {SizeSuffix(storage_data.Length)}");
		}
	}
}
