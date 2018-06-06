using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ICSharpCode.SharpZipLib.ReproTester
{
	class ConsoleTimeWriter : TextWriter
	{
		const int BUFFER_SIZE = 26;

		static Stream stdOut;
		static ConsoleTimeWriter instance;
		static byte[] buffer = new byte[BUFFER_SIZE];

		static bool trailing = false;

		public static ConsoleTimeWriter Instance
			=> instance ?? (instance = new ConsoleTimeWriter());

		public override Encoding Encoding => Console.OutputEncoding;

		public static void HookConsole()
		{
			stdOut = Console.OpenStandardOutput();
			Console.SetOut(Instance);
		}

		internal static void HookTrace()
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Instance));
		}

		private ConsoleTimeWriter()
		{
		}

		public override void WriteLine(string value)
			=> Write(value, true);

		public override void Write(string value)
			=> Write(value, false);

		public void Write(string value, bool newline = false)
		{
			int count = 0;
			int index = 0;
			int unread = value.Length;

			if(!trailing)
			{
				var currCol = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Gray;
				var timestamp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff] ");
				count = Console.OutputEncoding.GetBytes(timestamp, 0, timestamp.Length, buffer, 0);
				stdOut.Write(buffer, 0, count);
				Console.ForegroundColor = currCol;
			}

			do
			{
				count = (unread - index);

				count = Console.OutputEncoding.GetBytes(value, index, count > BUFFER_SIZE ? BUFFER_SIZE : count, buffer, 0);
				if (newline && count < (BUFFER_SIZE-1))
					buffer[count++] = (byte)'\n';
				stdOut.Write(buffer, 0, count);
				index += count;

			} while (count >= BUFFER_SIZE);

			trailing = !newline;
		}

#if UNSAFE
		public unsafe override void Write(char value)
		{
			byte[] bytes = new byte[] { 0, 0 };
			int count = 0;
			fixed (byte* b = bytes)
			{
				count = Console.OutputEncoding.GetBytes(&value, 1, b, 2);
			}

			stdOut.Write(bytes, 0, count);
		}
#else
		public override void Write(char value)
		{
			byte[] cbuf = new byte[2] ;
			stdOut.Write(cbuf, 0, Console.OutputEncoding.GetBytes(new[] { value }, 0, 1, cbuf, 0));
		}
#endif
	}
}
