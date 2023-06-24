using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RCS.Service
{
	public enum LogLevel : int
	{
		Info = 0,
		Error = 1,
		Warning = 2,
	}

	public static class Log
	{
		public static void WriteError(Exception er)
		{
			Directory.CreateDirectory($"{XmlProvider.PathToSave}\\logs");
			File.WriteAllText($"{XmlProvider.PathToSave}\\logs\\{DateTime.Now.Ticks}.txt", $"{er}\n\n{er.Message}\n\n{er.StackTrace}");
			WriteLine(er, LogLevel.Error);
		}
		static void WriteColor(string message, ConsoleColor color)
		{
			var pieces = Regex.Split(message, @"(\[[^\]]*\])");
			bool use_color = false;
			for (int i = 0; i < pieces.Length; i++)
			{
				string piece = pieces[i];

				if (piece.StartsWith("[") && piece.EndsWith("]"))
				{
					Console.ForegroundColor = color;
					piece = piece.Substring(1, piece.Length - 2);
					use_color = true;
				}
				if (use_color)
					Console.Write($"[{piece}]");
				else
					Console.Write(piece);
				Console.ResetColor();
				use_color = false;
			}

			Console.WriteLine();
		}
		public static void WriteLine(object data, LogLevel log_level)
		{
#if DEBUG
			if (log_level == LogLevel.Info)
				WriteColor($"[{log_level} {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}] {data}", ConsoleColor.White);
			else if (log_level == LogLevel.Warning)
				WriteColor($"[{log_level} {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}] {data}", ConsoleColor.Yellow);
			else if (log_level == LogLevel.Error)
				WriteColor($"[{log_level} {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}] {data}", ConsoleColor.Red);
#endif
		}

		public static void WriteLine(object data)
		{
			Console.WriteLine($"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}] {data}");
		}
	}
}
