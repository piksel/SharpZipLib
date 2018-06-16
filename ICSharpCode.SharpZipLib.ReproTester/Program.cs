using ICSharpCode.SharpZipLib.ReproTester.Repros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ICSharpCode.SharpZipLib.ReproTester
{
	static class Program
	{
		private static PrefixedConsoleWriter mainLogger;
		private static string workRoot;

		public static string WorkRoot => workRoot;

		static void Main()
		{
			Console.Title = "SharpZipLib Repro Tester";
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(Console.Title);
			Console.WriteLine();

			// Hook the console so that timestamps and color can be added
			ConsoleTimeWriter.HookConsole();

			// Hook the Debug/Trace outputs (does not work on NETCORE)
			ConsoleTimeWriter.HookTrace();

			mainLogger = ConsoleWriter.Prefixed("[Main] ", ConsoleColor.Yellow);

			workRoot = Path.Combine(Path.GetTempPath(), "SharpZipRepro");
			if(Directory.Exists(workRoot))
			{
				mainLogger.Write("Cleaning work directory from previous tests... ");
				Directory.Delete(workRoot, true);
				mainLogger.Write("OK").End();
			}

			Directory.CreateDirectory(workRoot);

			Run<Repro229>();
			//Run<Repro218>();
			//Run<Repro213>();
			//Run<Repro204>();
			//Run<Repro118>();
			//Run<Repro208>();

			//RunAll();

			Console.Write("Press any key to exit... ");
			Console.ReadKey();
		}

		private static void RunAll()
		{
			mainLogger.Write($"Warning: Running all repros in current assembly.", ConsoleColor.Yellow).End();

			int succeeded = 0;
			int failed = 0;
			foreach (var et in Assembly.GetCallingAssembly().DefinedTypes)
			{
				if(et != typeof(ReproTEMPLATE) && et != typeof(Repro) && typeof(Repro).IsAssignableFrom(et))
				{
					mainLogger.Write("Start of ")._(et.Name, ConsoleColor.Cyan).End();

					var timeOut = et.GetCustomAttribute<TimeoutAttribute>()?.TimeOut 
						?? TimeSpan.FromMinutes(3);

					timeOut = TimeSpan.FromSeconds(10);

					var result = Run(et.Name, () => et.DeclaredConstructors.First().Invoke(new object[0]) as Repro, timeOut);
					if (result)
						succeeded++;
					else
						failed++;

					mainLogger.Write("End of ")._(et.Name, ConsoleColor.Cyan)._("\n").End();
				}
			}

			ConsoleWriter.Write($"Out of ")
				._(failed + succeeded, ConsoleColor.Cyan)
				._(" repro(s), ")
				._(failed, ConsoleColor.Red)
				._(" failed and ")
				._(succeeded, ConsoleColor.Green)
				._(" ran without exceptions.").End();
		}

		private static bool Run<TRepro>() where TRepro : Repro, new()
			=> Run(typeof(TRepro).Name, () => new TRepro());

		private static bool Run(string name, Func<Repro> constructor, TimeSpan? timeOut = null)
		{
			if (!timeOut.HasValue) timeOut = TimeSpan.FromMinutes(5);

			Repro repro = null;
			Exception threadException = null;

			var reproLog = ConsoleWriter.Prefixed($"[{name}] ", ConsoleColor.White);
			//reproLog.LinePrefixLength = ConsoleTimeWriter.PREFIX_LENGTH;

			try
			{
				reproLog.Write("Preparing...", ConsoleColor.Cyan).End(ConsoleColor.Gray);

				repro = constructor();

				var t = new Thread((ro) =>
				{
					try
					{
						if (ro is Repro r)
						{
							r.Run();
						}
						else
						{
							threadException = new InvalidOperationException("Invalid parameter passed to repro runner thread");
						}
					}
					catch (Exception x)
					{
						threadException = x;
					}
				});

				reproLog.Write("Running...", ConsoleColor.Cyan).End(ConsoleColor.White);

				t.Start(repro);
				if (!t.Join(timeOut.Value))
				{
					t.Abort();
					if (!t.Join(TimeSpan.FromSeconds(20)))
						reproLog.Write("Warning: The repro thread did not abort correctly, cleanup may fail!", ConsoleColor.Yellow).End();

					throw new TimeoutException($@"Timed out waiting for repro to finish ({timeOut:mm\m\ ss\s})");
				}

				if (threadException != null)
					throw threadException;

				reproLog.Write("No unhandled exceptions were caught.", ConsoleColor.Green).End();
				return true;
			}
			catch (Exception x)
			{
				var xw = reproLog.Write($"{x.GetType().Name} thrown.\nMessage: {x.Message}", ConsoleColor.Red, ConsoleTimeWriter.PREFIX_LENGTH);
				xw._($"\n{x.StackTrace}", ConsoleColor.Red, 0);

				var ix = x.InnerException;
				int level = 0;
				while (ix != null)
				{
					level += 2;
					xw._($"\n{ix.GetType().FullName}: {ix.Message}", ConsoleColor.Red, level);
					ix = ix.InnerException;
				}
				xw.End();

				return false;
			}
			finally
			{
				reproLog.Write("Cleaning up...", ConsoleColor.Cyan).End();
				repro?.Cleanup();
				reproLog.Write("Done!", ConsoleColor.Cyan).End();
			}
		}



	}
}
