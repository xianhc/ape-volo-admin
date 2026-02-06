using System;
using System.Collections.Generic;
using System.Linq;

namespace Ape.Volo.Common.Helper;

public static class ConsoleHelper
{
    public static void WriteLine() => Console.Out.WriteLine();

    /// <summary>
    /// 打印控制台信息
    /// </summary>
    /// <param name="str">待打印的字符串</param>
    /// <param name="color">想要打印的颜色</param>
    public static void WriteLine(string str, ConsoleColor color = ConsoleColor.White)
    {
        ConsoleColor currentForeColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(str);
        Console.ForegroundColor = currentForeColor;
    }


    public static void PrintConfigTable(
        Dictionary<string, string> configs,
        string title = "配置集",
        ConsoleColor borderColor = ConsoleColor.Gray,
        ConsoleColor headerBgColor = ConsoleColor.Cyan, // 青底
        ConsoleColor headerFgColor = ConsoleColor.Black) // 黑字
    {
        var headers = new[] { "配置名称", "配置信息" };
        var rows = configs.Select(kv => new[] { kv.Key, kv.Value }).ToList();
        rows.Insert(0, headers);

        int col1Width = rows.Max(r => GetStringRealLength(r[0]));
        int col2Width = rows.Max(r => GetStringRealLength(r[1]));

        void PrintBorder(string left, string mid, string right, string lineChar)
        {
            Console.ForegroundColor = borderColor;
            Console.Write(left);
            Console.Write(new string(lineChar[0], col1Width + 2));
            Console.Write(mid);
            Console.Write(new string(lineChar[0], col2Width + 2));
            Console.WriteLine(right);
            Console.ResetColor();
        }

        string Pad(string s, int width, bool center)
        {
            int len = GetStringRealLength(s ?? string.Empty);
            int diff = width - len;
            if (center)
            {
                int padLeft = diff / 2, padRight = diff - padLeft;
                return new string(' ', padLeft) + s + new string(' ', padRight);
            }

            return s + new string(' ', diff);
        }

        PrintBorder("╔", "╦", "╗", "═");

        // 标题
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("║");
        int totalWidth = col1Width + col2Width + 5;
        int leftPad = (totalWidth - GetStringRealLength(title)) / 2;
        int rightPad = totalWidth - GetStringRealLength(title) - leftPad;
        Console.Write(new string(' ', leftPad));
        Console.Write(title);
        Console.WriteLine(new string(' ', rightPad) + "║");
        Console.ResetColor();

        PrintBorder("╠", "╬", "╣", "═");

        for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
        {
            var row = rows[rowIdx];
            Console.Write("║ ");
            // -------- 表头变青底黑字 --------
            if (rowIdx == 0)
            {
                Console.BackgroundColor = headerBgColor;
                Console.ForegroundColor = headerFgColor;
            }

            Console.Write(Pad(row[0], col1Width, true));
            Console.ResetColor();

            Console.Write(" ║ ");

            if (rowIdx == 0)
            {
                Console.BackgroundColor = headerBgColor;
                Console.ForegroundColor = headerFgColor;
                Console.Write(Pad(row[1], col2Width, true));
                Console.ResetColor();
            }
            else
            {
                if (row[1].Equals("True", StringComparison.OrdinalIgnoreCase))
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (row[1].Equals("False", StringComparison.OrdinalIgnoreCase))
                    Console.ForegroundColor = ConsoleColor.Red;
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Write(Pad(row[1], col2Width, true));
                Console.ResetColor();
            }

            Console.WriteLine(" ║");

            if (rowIdx == 0) PrintBorder("╠", "╬", "╣", "═");
        }

        PrintBorder("╚", "╩", "╝", "═");
    }

    static int GetStringRealLength(string str)
    {
        if (string.IsNullOrEmpty(str)) return 0;
        int len = 0;
        foreach (char c in str)
            len += c > 127 ? 2 : 1;
        return len;
    }
}