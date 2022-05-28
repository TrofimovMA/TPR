using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Library
{
    public static class Lib
    {
        // Проверка
        public static void Test()
        {
            Console.WriteLine("Test");
        }

        // Таблица
        public class Table
        {
            public string[,] table; // Содержание таблицы
            public StyleString[,] style; // Стиль ячеек
            public int[] maxRowHeight; // Наибольшая высота строки
            public int[] maxColWidth; // Наибольшая ширина столбца
            public int[] lineSeparators; // Дополнительные линии-разделители

            // Конструктор
            public Table(int x, int y)
            {
                table = new string[x, y];
                style = new StyleString[x, y];
                maxRowHeight = new int[x];
                maxColWidth = new int[y];
                lineSeparators = new int[2] { 0, x - 1 };

                for (int i = 0; i < x; i++)
                    for (int j = 0; j < y; j++)
                    {
                        table[i, j] = String.Empty;
                        style[i, j] = "0L0";
                    }
            }

            // Получение кол-ва строк
            public int GetX()
            {
                return (table == null) ? 0 : table.GetLength(0);
            }

            // Получение кол-ва столбцов
            public int GetY()
            {
                return (table == null) ? 0 : table.GetLength(1);
            }

            // Обновление информации о таблице
            public void UpdateInfo()
            {
                for (int i = 0; i < GetX(); i++)
                {
                    int max = 0;
                    for (int j = 0; j < GetY(); j++)
                    {
                        if (table[i, j].ToLines().Length > max)
                            max = table[i, j].ToLines().Length;
                    }
                    maxRowHeight[i] = max;
                }
                for (int j = 0; j < GetY(); j++)
                {
                    int max = 0;
                    for (int i = 0; i < GetX(); i++)
                    {
                        int addEmptySpace = (style[i, j].GetParam(0) + style[i, j].GetParam(2));
                        if (table[i, j].ToLines().GetMaxLength() + addEmptySpace > max)
                            max = table[i, j].ToLines().GetMaxLength() + addEmptySpace;
                    }
                    maxColWidth[j] = max;
                }
            }

            // Вывод таблицы
            public void PrintTable()
            {
                Console.WriteLine(GetTableLineSeparator());
                for (int i = 0; i < GetX(); i++)
                {
                    string[] cols = table.GetRow(i).ToArray();
                    for (int k = 0; k < maxRowHeight[i]; k++)
                    {
                        Console.Write("|");
                        for (int j = 0; j < cols.Length; j++)
                        {
                            string s = cols[j].ToLines(maxRowHeight[i])[k];
                            s = " ".Repeat(style[i, j].GetParam(0)) + s + " ".Repeat(style[i, j].GetParam(2));
                            s = s.ApplyAlign(style[i, j].GetParam(1), maxColWidth[j]);
                            Console.Write(s);
                            Console.Write("|");
                        }
                        Console.WriteLine();
                    }

                    if (lineSeparators.Contains(i))
                        Console.WriteLine(GetTableLineSeparator());
                }
            }

            // Линия - разделитель строк таблицы
            public string GetTableLineSeparator(int[] ignore = null)
            {
                string str = "+";
                for (int j = 0; j < maxColWidth.Length; j++)
                    if (ignore == null || !ignore.Contains(j))
                        str += "-".Repeat(maxColWidth[j]) + "+";
                    else
                        str += "-".Repeat(maxColWidth[j]) + "-";
                return str;
            }

            // Задание стиля ячейки с помощью строки
            // Формат: "1C1" = пробелы до / выравнивание (L-лево/R-право/C-центр) / пробелы после
            public class StyleString
            {
                readonly string style;

                public StyleString(string str)
                {
                    this.style = str;
                }

                public static implicit operator StyleString(string d)
                {
                    return new StyleString(d);
                }

                public static implicit operator string(StyleString d)
                {
                    return d.style;
                }

                public int GetParam(int num)
                {
                    Regex regex = new Regex(@"(\d+)([LRC])(\d+)");
                    Match match = regex.Match(style);
                    switch (num)
                    {
                        case 0:
                        case 2:
                            return int.Parse(match.Groups[num + 1].Value);
                        default:
                            switch (match.Groups[2].Value)
                            {
                                case "L":
                                    return 0;
                                case "R":
                                    return 1;
                                default:
                                    return 2;
                            }
                    }
                }
            }
        }

        // Методы Расширения
        public static string ApplyAlign(this string s, int align, int width)
        {
            switch (align)
            {
                case 0:
                    return String.Format($"{{0, {-width}}}", s);
                case 1:
                    return String.Format($"{{0, {width}}}", s);
                default:
                    if (s.Length >= width)
                        return s;
                    int leftPadding = (width - s.Length) / 2;
                    int rightPadding = width - s.Length - leftPadding;
                    return new string(' ', leftPadding) + s + new string(' ', rightPadding);
            }
        }
        public static string DelQuotes(this string str)
        {
            string s; int f = 0; int l = str.Length - 1;
            s = str.Substring((str[f] == '"' && str[l] == '"') ? 1 : 0, str.Length - ((str[f] == '"' && str[l] == '"') ? 2 : 0));
            return s;
        }
        public static string Repeat(this string str, int num)
        {
            string s = string.Empty;
            for (int i = 1; i <= num; i++)
                s += str;
            return s;
        }
        public static int GetSum(this int[] array, int[] consider)
        {
            int sum = 0;
            for (int i = 0; i < array.Length; i++)
                if (consider.Contains(i))
                    sum += array[i];
            return sum;
        }
        public static string[] ToLines(this string str, int minNum = -1, string separator = "\n")
        {
            string[] a = str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (minNum != -1 && a.Length < minNum)
            {
                int c = minNum - a.Length;
                for (int i = 0; i < c; i++)
                    a = a.Append("").ToArray();
            }
            return a;
        }
        public static IEnumerable<T> GetRow<T>(this T[,] array, int rowIndex)
        {
            int columnsCount = array.GetLength(1);
            for (int colIndex = 0; colIndex < columnsCount; colIndex++)
                yield return array[rowIndex, colIndex];
        }
        public static string ToMultiline(this string str, string separator = "|")
        {
            return str.Replace(separator, "\n");
        }
        public static int GetMaxLength(this string[] array)
        {
            return array.Select(x => x.Length).Max();
        }
        public static float InterParseFloat(this string a)
        {
            return float.Parse(a, NumberStyles.Any, CultureInfo.InvariantCulture);
        }
    }
}
