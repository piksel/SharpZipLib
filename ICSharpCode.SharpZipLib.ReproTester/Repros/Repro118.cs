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
	internal class Repro118 : Repro
	{
		internal override void Run()
		{
			//var fileName = DownloadInput("https://github.com/icsharpcode/SharpZipLib/files/1265994/aaa.zip");
			var fileName = GetSampleInput("aaa16.zip");

			using (FileStream fs = File.OpenRead(fileName))
			{
				var zf = new ZipFile(fs);
				zf.Password = "0123456789abcdef";
				foreach (ZipEntry zipEntry in zf)
				{
					if (!zipEntry.IsFile)
					{
						continue; // Ignore directories
					}

					byte[] buffer = new byte[zipEntry.Size];
					//try
					//{
					using (Stream zipStream = zf.GetInputStream(zipEntry))
					{
						zipStream.Read(buffer, 0, buffer.Length);
					}
					//}
					//catch (Exception ex)
					//{
									
					//}
				}
			}
		}
	}
}
