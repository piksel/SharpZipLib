using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ICSharpCode.SharpZipLib.ReproTester.Repros
{
	internal class Repro121 : Repro
	{
		readonly string[] FileFormats = new [] { "gnu", "oldgnu", "v7", "ustar", "posix" };

		private readonly (string Format, string FileUtf8, string FileLongName)[] testFiles;

		const string LongFileNameGood = "lftest-0000000000111111111122222222223333333333444444444455555555556666666666777777777788888888889999999999";
		const string Utf8FileNameGood = "åäö";
		const string Utf8FileNameBad = "Ã¥Ã¤Ã¶";


		public Repro121()
		{
			testFiles = FileFormats.Select(f => (
				Format: f,
				FileUtf8: GetSampleInput($"utf8test-{f}.tar", "tar"),
				FileLongName: GetSampleInput($"lftest-{f}.tar", "tar")
			)).ToArray();
		}

		internal override void Run()
		{
			void LogTestResult(string name, string error)
			{
				var cw = ConsoleWriter.Write($"  {name}: ");
				if (error != null)
					cw._($"Failed! Error: {error}", ConsoleColor.Red).End();
				else
					cw._($"OK!", ConsoleColor.Green).End();
			}

			Console.WriteLine();

			foreach (var (Format, FileUtf8, FileLongName) in testFiles)
			{
				ConsoleWriter.Write($"Testing ")._(Format, ConsoleColor.Cyan)._(" format: ").End();
				var utf8error = TestUtf8(Format, FileUtf8);
				var lferror = TestLongName(Format, FileLongName);

				ConsoleWriter.Write($"Results for ")._(Format, ConsoleColor.Cyan)._(":").End();
				LogTestResult("UTF8", utf8error);
				LogTestResult("Long file names", lferror);
				Console.WriteLine();
			}
		}

		private string TestUtf8(string format, string file)
		{
			string error = null;
			Console.WriteLine($"## Testing {format} UTF8");

			using (var fs = File.OpenRead(file))
			using (var tis = new TarInputStream(fs))
			{

				var entry = tis.GetNextEntry();
				if (entry == null)
					error = "Entry is null";
				else if (entry.Name == null)
					error = "Entry name is null";
				else if (entry.Name == Utf8FileNameBad)
					error = $"Entry name matches known bad value";
				else if (entry.Name != Utf8FileNameGood)
					error = "Entry name does not match expected good value";

				ConsoleWriter.Write($"Name: ")._(entry?.Name ?? "(null)", ConsoleColor.Magenta).End();

			}

			return error;
		}

		private string TestLongName(string format, string file)
		{
			string error = null;
			Console.WriteLine($"## Testing {format} long filename");

			using (var fs = File.OpenRead(file))
			using (var tis = new TarInputStream(fs))
			{
				var entry = tis.GetNextEntry();

				if (entry == null)
					error = "Entry is null";
				else if (entry.Name == null)
					error = "Entry name is null";
				else if (entry.Name.Length < 107)
					error = $"Entry name truncated to {entry.Name.Length} bytes";
				else if (entry.Name != LongFileNameGood)
					error = "Entry name does not match expected good value";

				ConsoleWriter.Write($"Name: ")._(entry?.Name ?? "(null)", ConsoleColor.Magenta).End();

				return error;

			}
		}
	}
}
