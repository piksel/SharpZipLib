using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace ICSharpCode.SharpZipLib.ReproTester
{
	internal abstract class Repro
	{
		private string workPath;
		private uint inputCount = 0;
		private Random random = new Random();

		internal abstract void Run();

		protected Repro()
		{
			workPath = Path.Combine(Path.GetTempPath(),
				$"SharpZipRepro_{GetType().Name}_{DateTime.UtcNow.Ticks:x16}");
			if (Directory.Exists(workPath))
				Directory.Delete(workPath, true);

			Directory.CreateDirectory(workPath);
		}

		public void Cleanup()
		{
			if (Directory.Exists(workPath))
				Directory.Delete(workPath, true);
		}

		internal string DownloadInput(string url, string fileExt = null)
		{
			if (string.IsNullOrEmpty(fileExt))
			{
				fileExt = "zip";
				try
				{
					Path.GetExtension(url);
				}
				catch (Exception x)
				{
					Console.WriteLine($"Error trying to get extension: {x.Message}, using default .zip");
				}
			}

			var fileName = Path.Combine(workPath, $"input{inputCount++:x2}.{fileExt}");

			Console.Write($"Downloading \"{url}\" to \"{fileName}\"... ");

			var hc = new HttpClient();
			using (var fs = File.OpenWrite(fileName))
			{
				var ass = hc.GetStreamAsync(url);
				if (ass.Wait(TimeSpan.FromSeconds(30)))
				{
					using (var hs = ass.Result)
					{
						hs.CopyTo(fs);
						Console.WriteLine("OK");
					}
				}
				else
				{
					Console.WriteLine("Timed out!");
					throw new TimeoutException($"Timed out waiting for the input file \"url\" to download");
				}
			}
			return fileName;
		}

		internal string GetSampleInput(string sampleName, string fileExt = null)
		{
			if (string.IsNullOrEmpty(fileExt))
			{
				fileExt = "zip";
				try
				{
					Path.GetExtension(sampleName);
				}
				catch (Exception x)
				{
					Console.WriteLine($"Error trying to get extension: {x.Message}, using default .zip");
				}
			}

			var fileName = Path.Combine(workPath, $"input{inputCount++:x2}.{fileExt}");
			var sampleFile = Path.GetFullPath(Path.Combine("Repros", "Samples", GetType().Name, sampleName));

			Console.Write($"Copying \"{sampleFile}\" to \"{fileName}\"... ");

			try
			{
				File.Copy(sampleFile, fileName);
				Console.WriteLine("OK");
			}
			catch (Exception x)
			{
				Console.WriteLine("Failed!");
				throw new Exception($"Failed to get input from file {sampleName}", x);
			}

			return fileName;
		}

		internal string[] GetDummyFiles(int count = 5)
		{
			var files = Array.CreateInstance(typeof(string), count) as string[];

			for(int i=0; i < count; i++)
			{
				files[i] = GetDummyFile();
			}

			return files;
		}

		internal string GetDummyFile(int size = -1)
		{
			var filename = Path.Combine(workPath, Path.GetRandomFileName());
			if (size < 0)
			{
				File.WriteAllText(filename, DateTime.UtcNow.Ticks.ToString("x16"));
			}
			else if(size > 0)
			{
				var bytes = Array.CreateInstance(typeof(byte), size) as byte[];
				random.NextBytes(bytes);
				File.WriteAllBytes(filename, bytes);
			}
			return filename;
		}

		internal string GetOutput(string extension = null)
		{
			var output = Path.GetRandomFileName();
			if (extension != null)
				Path.ChangeExtension(output, extension);
			return Path.Combine(workPath, output);
		}

		internal string GetOutputDir()
		{
			var dir = Path.Combine(workPath, Path.GetRandomFileName());
			Directory.CreateDirectory(dir);
			return dir;
		}
	}
}
