using ICSharpCode.SharpZipLib.GZip;
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
	internal class Repro204 : Repro
	{
		internal override void Run()
		{
			var gzipFileName = GetSampleInput("test.tar.gz");
			var targetDir = GetOutputDir();

			Stream inStream = File.OpenRead(gzipFileName);
			Stream gzipStream = new GZipInputStream(inStream);
			TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
			tarArchive.ExtractContents(targetDir);
			tarArchive.Close();
			gzipStream.Close();


		}
	}
}
