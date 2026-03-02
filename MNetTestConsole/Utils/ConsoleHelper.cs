using System;
using System.Collections.Generic;
using System.Text;

namespace MNetTestConsole.Utils
{
    internal class ConsoleHelper
    {
        public static void WriteWithColor()
        {

        }
        public static void WriteLineWithColor(string line, ConsoleColor color)
        {
            ConsoleColor _old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(line);
            Console.ForegroundColor = _old;
        }
        public static void WriteLineWithYellow(object line)
        {
            WriteLineWithColor(line?.ToString(), ConsoleColor.Yellow);
        }
        public static void WriteLineWithRed(object line)
        {
            WriteLineWithColor(line?.ToString(), ConsoleColor.Red);
        }
    }
}
