using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;

namespace ICSharpCode.SharpZipLib.ReproTester.Repros
{
	[Timeout(minutes: 1)]
	internal class Repro229 : Repro
	{
		string inputGood;
		string inputBad;

		public Repro229()
		{
			inputGood = GetSampleInput("afmlsxmio-good.zip", "zip");
			inputBad = GetSampleInput("afmlsxmio-bad.zip", "zip");
		}

		internal override void Run()
		{
			Console.WriteLine("GOOD:");
			TestFile(inputGood);

			Console.WriteLine("BAD:");
			TestFile(inputBad);
		}

		internal void TestFile(string inputFile)
		{
			var entryPrinted = false;
			var _c = Stopwatch.StartNew();

			Console.Write("Opening file... ");
			using (var fs = File.Open(inputFile, FileMode.Open, FileAccess.Read))
			using(var zf = new ZipFile(fs))
			{
				var maxBytes = 1100000;
				Console.WriteLine($" {_c.ElapsedMilliseconds:f0}ms");

				void testCallback(TestStatus s, string m)
				{
					if (s.BytesTested > maxBytes)
						throw new Exception($"Tested bytes exceeds maximum ({maxBytes} bytes)");

					if(s.Operation == TestOperation.Complete)
					{
						ConsoleWriter.Write("Test OK", ConsoleColor.Green).End();
					}
					if(!string.IsNullOrEmpty(m))
					{
						if(s.ErrorCount > 0)
						{
							throw new Exception(m);
						}
						else
						{
							Console.WriteLine(m);
						}
					}
					if (!entryPrinted && s.Entry is ZipEntry e)
					{
						Console.WriteLine($"Entry #{e.ZipFileIndex} {e.Name}");
						Console.WriteLine($"  Size: {e.Size} byte(s) uncompressed, {e.CompressedSize} byte(s) compressed.");
						Console.WriteLine($"  CRC: {(e.HasCrc ? e.Crc : 0):x8} ");
						Console.WriteLine($"  Version: {e.Version}, {(e.VersionMadeBy / 2.0):f1} ");
						entryPrinted = true;
					}
				}
				Console.WriteLine("Testing archive WITH low level CRC check...");
				_c.Restart();
				var status = zf.TestArchive(true, TestStrategy.FindFirstError, testCallback);
				Console.WriteLine($"Testing done, status: {(status ? "OK" : "Failed")} ({_c.ElapsedMilliseconds:f0}ms)");
			}
		}
	}
}
