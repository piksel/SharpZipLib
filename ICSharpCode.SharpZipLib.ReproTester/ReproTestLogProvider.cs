using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Logging;
using ICSharpCode.SharpZipLib.Logging.LogProviders;

namespace ICSharpCode.SharpZipLib.ReproTester
{
	internal class ReproTestLogProvider : ILogProvider
	{
		public Logger GetLogger(string name)
		{
			return new ReproTestLogger(name).Log;
		}

		public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
			=> new DisposableFacade();

		public IDisposable OpenNestedContext(string message)
			=> new DisposableFacade();

		internal class ReproTestLogger
		{
			string name;

			public ReproTestLogger(string name)
			{
				this.name = name;
			}

			readonly ConsoleColor[] LevelColors = new []
			{
				ConsoleColor.Cyan,   // Trace,
				ConsoleColor.Gray,   // Debug,
				ConsoleColor.White,  // Info,
				ConsoleColor.Yellow, // Warn,
				ConsoleColor.Red,    // Error,
				ConsoleColor.Magenta // Fatal
			};

			internal bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, object[] formatParameters)
			{
				if (messageFunc == null) return true;

				ConsoleWriter.Write($"[{name}] [{logLevel}]: ")._(messageFunc?.Invoke(), LevelColors[(int)logLevel]).End();
				if (exception != null)
				{
					//Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Threw {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}");
				}
				return true;
			}
		}

		internal class DisposableFacade : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}

}
