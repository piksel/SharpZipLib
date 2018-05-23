using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ICSharpCode.SharpZipLib.ReproTester.Repros
{
    class Repro208: Repro
    {
		public static byte[] MakeTarGzipArchive(Dictionary<string, byte[]> files)
		{
			using (var output = new MemoryStream())
			{
				var gz = new GZipOutputStream(output);
				var tar = new TarOutputStream(gz);

				var tarArchive = TarArchive.CreateOutputTarArchive(tar);

				foreach (var file in files)
				{
					var tarEntry = TarEntry.CreateTarEntry(file.Key);
					var size = file.Value.Length;
					tarEntry.Size = size;
					tar.PutNextEntry(tarEntry);
					tar.Write(file.Value, 0, size);
					tar.CloseEntry();
					//tarArchive.RootPath = "";
					//tarArchive.WriteEntry(tarEntry, false);
				}

				tar.IsStreamOwner = false;
				tar.Close();

				gz.IsStreamOwner = false;
				gz.Close();

				output.Position = 0;

				return output.ToArray();
			}
		}

		internal override void Run()
		{
			var inputFiles = new Dictionary<string, byte[]>();

			foreach(var f in GetDummyFiles(5))
			{
				inputFiles.Add(Path.GetFileName(f), File.ReadAllBytes(f));
			}

			var bytes = MakeTarGzipArchive(inputFiles);
			Console.WriteLine("Result bytes: ");
			foreach (var b in bytes)
				Console.Write(b.ToString("x2") + " ");

			var outputFile = GetOutput("tar");
			Console.WriteLine($"Writing output to \"{outputFile}\"...");

			File.WriteAllBytes(outputFile, bytes);

			Console.WriteLine("Working directory will be cleaned, verify output contents and press any key to continue...");
			Console.ReadKey();

			Console.WriteLine();
		}
	}
}
