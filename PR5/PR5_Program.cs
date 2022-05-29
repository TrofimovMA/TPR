using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Library;

using Table = Library.Lib.Table;

namespace PR5
{
    static internal partial class PR5
    {
        static readonly string inputFile = Directory.GetCurrentDirectory() + @"\PR5.txt"; // Входной Файл

        static readonly List<string> Commands = new List<string>(); // Последовательность Команд Программы

        static int size;
        static List<Dictionary<string, int>> list = new List<Dictionary<string, int>>();

        // Загрузка Входных Данных
        static void LoadInputData()
        {
            // Входные Данные
            string inputStr = File.ReadAllText(inputFile);

            // Обработка Входных Данных
            Regex regex; Match match;
            // - Обработка комментариев
            regex = new Regex(@"^\s*(.*?)\s*(\/\/.*)?$", RegexOptions.Multiline);
            inputStr = regex.Replace(inputStr, "$1");
            // - Удаление пробелов
            inputStr = Regex.Replace(inputStr, @"(\t| )", "", RegexOptions.Multiline);
            Console.WriteLine(inputStr);
            // - Обработка Главной Функции
            regex = new Regex(@"^f\(x\)=(?<var>((\+|-)?\d*x\d+))+->max$", RegexOptions.Multiline);
            foreach (Match m in regex.Matches(inputStr))
            {
                Console.WriteLine("DEBUG: " + m.Value);
                List<string> var = new List<string>(m.Groups["var"].Captures.Cast<Capture>().Select(x => x.Value));
                size = var.Count();
                list = new List<Dictionary<string, int>>();
                Dictionary<string, int> dict = new Dictionary<string, int>();
                var.ForEach(x =>
                {
                    string f = x.Substring(x.IndexOf("x"), x.Length - x.IndexOf("x"));
                    int s = -1*int.Parse(x.Substring(0, x.IndexOf("x")));
                    dict.Add(f, s);
                    Console.WriteLine("{0} vs {1}", f, s);
                    Console.WriteLine(x);
                });
                list.Add(dict);
            }
            // - Обработка Ограничений
            regex = new Regex(@"(?<var>((\+|-)?\d*x\d+))+<=(?<num>\d+)$", RegexOptions.Multiline);
            foreach (Match m in regex.Matches(inputStr))
            {
                Console.WriteLine("DEBUG 222: " + m.Value);
                List<string> var = new List<string>(m.Groups["var"].Captures.Cast<Capture>().Select(x => x.Value));
                Dictionary<string, int> dict = new Dictionary<string, int>();
                var.ForEach(x =>
                {
                    string nx = x;
                    if (nx[0] == 'x')
                        nx = "1" + nx;
                    string f = nx.Substring(nx.IndexOf("x"), nx.Length - nx.IndexOf("x"));
                    int s = int.Parse(nx.Substring(0, nx.IndexOf("x")));
                    dict.Add(f, s);
                    Console.WriteLine("{0} wo {1}", f, s);
                    Console.WriteLine(nx);
                });
                dict.Add("N", int.Parse(m.Groups["num"].Value));
                list.Add(dict);
            }
            // - Обработка Списка Команд
            regex = new Regex(@"S(\s*->\s*(?<cmd>\w+(?<pars>\(\S+\))?))+");
            match = regex.Match(inputStr);
            if (match.Success)
            {
                foreach (Capture c in match.Groups["cmd"].Captures)
                    Commands.Add(c.Value);
            }
        }

        // Главная Программа
        // ( Чтение входных данных и последовательное выполнение заданных в Инструкции команд )
        static void MainProgram()
        {
            // Загрузка Входных Данных
            LoadInputData();

            // Выполнение Инструкции = Результат Программы = Выходные Данные
            int cmdCount = 0;
            foreach (string c in Commands)
            {
                Console.Write($"{++cmdCount}. ");
                switch (c)
                {
                    // Симплексный Метод
                    case "SIMPLEX":
                        Console.WriteLine("СИМПЛЕКСНЫЙ МЕТОД");
                        //double[,] table = { {25, -3,  5},
                        //        {30, -2,  5},
                        //        {10,  1,  0},
                        //        { 6,  3, -8},
                        //        { 0, -6, -5} };

                        //double[,] table = { {50, 6,  5},
                        //        {128, 2,  4},
                        //        {150,  4,  16},
                        //        { 0, -10, -30} };

                        double[,] table = new double[list.Count, 3];
                        Dictionary<string, int> t = list[0];
                        list[0] = list[list.Count - 1];
                        list[list.Count - 1] = t;
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].ContainsKey("N"))
                                table[i, 0] = list[i]["N"];
                            else
                                table[i, 0] = 0;
                            if (list[i].ContainsKey("x1"))
                                table[i, 1] = list[i]["x1"];
                            else
                                table[i, 1] = 0;
                            if (list[i].ContainsKey("x2"))
                                table[i, 2] = list[i]["x2"];
                            else
                                table[i, 2] = 0;
                        }

                        //list.ForEach(x =>
                        //{
                        //    Console.WriteLine("---");
                        //    x.ToList().ForEach(y => Console.Write("{0} -> {1}", y.Key, y.Value));
                        //    Console.WriteLine();
                        //});

                        double[] result = new double[2];
                        double[,] table_result;
                        Simplex S = new Simplex(table);
                        table_result = S.Calculate(result);

                        Console.WriteLine("Решенная симплекс-таблица:");
                        for (int i = 0; i < table_result.GetLength(0); i++)
                        {
                            for (int j = 0; j < table_result.GetLength(1); j++)
                                Console.Write(table_result[i, j] + " ");
                            Console.WriteLine();
                        }

                        Console.WriteLine();
                        Console.WriteLine("Решение:");
                        Console.WriteLine("X[1] = " + result[0]);
                        Console.WriteLine("X[2] = " + result[1]);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
