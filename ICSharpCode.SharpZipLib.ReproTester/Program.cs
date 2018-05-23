using ICSharpCode.SharpZipLib.ReproTester.Repros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ICSharpCode.SharpZipLib.ReproTester
{
	static class Program
	{
		static void Main()
		{
			
			Console.Title = "SharpZipLib Repro Tester";
			Console.WriteLine(Console.Title);
			Console.WriteLine();

			Run<Repro213>();
			Run<Repro204>();
			//Run<Repro118>();
			//Run<Repro208>();

			Console.Write("Press any key to exit... ");
			Console.ReadKey();
		}

		private static void Run<TRepro>() where TRepro: Repro, new()
		{
			var name = typeof(TRepro).Name;
			TRepro repro = null;
			try
			{
				Console.WriteLine($"[{name}] Preparing...");
				Console.ForegroundColor = ConsoleColor.White;
				repro = new TRepro();

				Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine($"[{name}] Running...");
				Console.ForegroundColor = ConsoleColor.White;
				repro.Run();

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"[{name}] No unhandled exceptions were caught.");
			}
			catch (Exception x)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"[{name}] {x.GetType().Name} thrown: {x.Message}");
				var ix = x.InnerException;
				while (ix != null) {
					Console.WriteLine($"  {ix.GetType().Name}: {ix.Message}");
					ix = ix.InnerException;
				}
				Console.WriteLine("Stacktrace:");
				Console.WriteLine(x.StackTrace);
				Console.WriteLine();
			}
			finally
			{
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine($"[{name}] Cleaning up...");
				repro?.Cleanup();
				Console.WriteLine($"[{name}] Done!\n");
			}
		}
	}
}
