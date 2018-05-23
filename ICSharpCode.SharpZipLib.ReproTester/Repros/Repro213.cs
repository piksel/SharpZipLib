using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace ICSharpCode.SharpZipLib.ReproTester.Repros
{
	internal class Repro213 : Repro
	{
		internal override void Run()
		{
			//var fileName = GetSampleInput("aaa16.zip");
			//var files = GetDummyFiles(5);
			//var output = GetOutput("zip");
			var targetPath = GetDummyFile(70145);

			var memstream = new MemoryStream();

			var writePos = WriteFileToTarStream(memstream, targetPath);
			Console.WriteLine($"Wrote {writePos} bytes to stream.");

			using (var fs = new FileStream("test_archive.tar", FileMode.Create))
			{
				memstream.WriteTo(fs);
			}

			memstream.Seek(0, SeekOrigin.Begin);

			var readPos = ReadFileFromTarStream(memstream);
			Console.WriteLine($"Read {readPos} bytes from stream.");

			Console.WriteLine($"Difference of {writePos - readPos} bytes.");

		}

		static long WriteFileToTarStream(Stream stream, string target)
		{
			TarArchive tar = TarArchive.CreateOutputTarArchive(stream);
			tar.IsStreamOwner = false;

			tar.RootPath = Path.GetDirectoryName(target);

			// see SharpZipLib GitHub Wiki for details. Short version is the root
			// path must use forward slashes, and not end with a slash.
			tar.RootPath = tar.RootPath.Replace('\\', '/');
			if (tar.RootPath.EndsWith("/"))
			{
				// remove the trailing slash
				tar.RootPath = tar.RootPath.Remove(tar.RootPath.Length - 1);
			}

			var entry = TarEntry.CreateEntryFromFile(target);
			tar.WriteEntry(entry, false);

			tar.Close();

			return stream.Position;

		}

		static long ReadFileFromTarStream(Stream stream)
		{
			var tar = TarArchive.CreateInputTarArchive(stream);
			tar.IsStreamOwner = false;

			var tempPath = System.IO.Path.GetTempPath();
			var tempDir = Path.Combine(tempPath, "tarTest");

			if (Directory.Exists(tempDir))
			{
				Directory.Delete(tempDir, true);
			}

			tar.ExtractContents(tempDir);
			tar.Close();

			return stream.Position;
		}
	}
}
