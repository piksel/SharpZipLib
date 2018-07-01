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
		const int BUFFER_SIZE = 32;
		const int BUFFER_MAX = BUFFER_SIZE - 1;

		static Stream stdOut;
		static ConsoleTimeWriter instance;
		static byte[] buffer = new byte[BUFFER_SIZE];

		static bool trailing = false;

		const string PREFIX_STRING = "[yyyy-MM-dd HH:mm:ss.fff] ";
		public const int PREFIX_LENGTH = 26; // prefixString.Length;

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

		public override void WriteLine()
			=> Write("", true);

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
				var timestamp = DateTime.Now.ToString(PREFIX_STRING);
				count = Console.OutputEncoding.GetBytes(timestamp, 0, timestamp.Length, buffer, 0);
				stdOut.Write(buffer, 0, count);
				Console.ForegroundColor = currCol;
			}

			do
			{
				if (index > unread) break;

				else if (index < unread)
				{
					count = (unread - index);
					count = Console.OutputEncoding.GetBytes(value, index, count > BUFFER_SIZE ? BUFFER_SIZE : count, buffer, 0);
				}
				else
				{
					count = 0;
				}
				if (newline)
				{
					if (count < BUFFER_SIZE)
					{
						buffer[count++] = (byte)'\n';
					}
					else if (count == BUFFER_SIZE && (index + count) == unread)
					{
	
					}
				}

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

	public class ConsoleWriter
	{
		private static ConsoleWriter defaultInstance = new ConsoleWriter();

		private ConsoleWriter() { }

		public int LinePrefixLength { get; set; }

		private int getIndentation(int indentation)
			=> indentation > 0 ? indentation : LinePrefixLength + indentation;

		public static ConsoleWriter Write(object o, ConsoleColor color, int indentation, bool indentFirst = false)
		{
			var cc = Console.ForegroundColor;
			Console.ForegroundColor = color;
			var cw = Write(o, indentation, indentFirst);
			Console.ForegroundColor = cc;
			return cw;
		}

		public static ConsoleWriter Write(object o, ConsoleColor color)
		{
			var cc = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Write(o);
			Console.ForegroundColor = cc;
			return defaultInstance;
		}

		internal static PrefixedConsoleWriter Prefixed(string prefix, ConsoleColor color)
		{
			return new PrefixedConsoleWriter(prefix, color);
		}

		public static ConsoleWriter Write(object o)
		{
			Console.Write(o);
			return defaultInstance;
		}

		public static ConsoleWriter Write(object o, int indentation, bool indentFirst = false)
		{
			var cw = new ConsoleWriter();
			cw.LinePrefixLength = indentation;
			var indentString = getIndentString(indentation);
			var indentedSep = "\n" + indentString;

			if (indentFirst)
				Console.Write(indentString);

			Console.Write(o.ToString().Replace("\n", indentedSep));

			return cw;
		}

		private static string getIndentString(int indentation)
			=> new string(' ', indentation);

		public ConsoleWriter _(object o, int indentation)
			=> Write(o, getIndentation(indentation));

		public ConsoleWriter _(object o)
			=> Write(o);

		public ConsoleWriter _(object o, ConsoleColor color, int indentation)
			=> Write(o, color, getIndentation(indentation));

		public ConsoleWriter _(object o, ConsoleColor color)
			=> Write(o, color);

		public void End(ConsoleColor? color = null)
		{
			if (color.HasValue)
				Console.ForegroundColor = color.Value;

			Console.WriteLine();
		}

	}

	internal class PrefixedConsoleWriter
	{
		private readonly string prefix;
		private readonly ConsoleColor prefixColor;
		private readonly int prefixLength;

		public PrefixedConsoleWriter(string prefix, ConsoleColor prefixColor)
		{
			this.prefix = prefix;
			this.prefixColor = prefixColor;
			this.prefixLength = prefix.Length;
		}

        //public int LinePrefixLength { get; set; }

        private int getIndentation(int indentation)
			=> indentation + prefixLength;

		public ConsoleWriter Write(object o, ConsoleColor color)
			=> ConsoleWriter.Write(prefix, prefixColor)._(o, color, 0);

		public ConsoleWriter Write(object o, ConsoleColor color, int indentation, bool indentFirst = false)
			=> ConsoleWriter.Write(prefix, prefixColor, getIndentation(indentation), indentFirst)._(o, color, 0);

		public ConsoleWriter Write(object o)
			=> ConsoleWriter.Write(prefix, prefixColor)._(o);

		public ConsoleWriter Write(object o, int indentation, bool indentFirst = false)
			=> ConsoleWriter.Write(prefix, prefixColor, getIndentation(indentation), indentFirst)._(o, 0);
	}
}
