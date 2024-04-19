using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechPort.Helpers
{
    public enum InformationType
    {
        Information = ConsoleColor.Cyan,
        AdditionalInformation = ConsoleColor.Gray,
        Warning = ConsoleColor.Yellow,
        Error = ConsoleColor.Red,
        Success = ConsoleColor.Green,
        DefaultColor = ConsoleColor.White
    }
    public static class Console
    {
        public static void Write(object value, InformationType color = InformationType.DefaultColor)
        {
            System.Console.ForegroundColor = (ConsoleColor)color;
            System.Console.Write(value.ToString());
            System.Console.ResetColor();
        }
        public static void WriteLine(object value, InformationType color = InformationType.DefaultColor)
        {
            System.Console.ForegroundColor = (ConsoleColor)color;
            System.Console.WriteLine(value.ToString());
            System.Console.ResetColor();
        }
        public static void NewLine(int i = 1)
        {
            for (; i > 0; i--)
                System.Console.WriteLine();
        }
    }
}
