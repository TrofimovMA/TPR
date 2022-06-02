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
        static List<Dictionary<string, float>> list = new List<Dictionary<string, float>>();

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
            //Console.WriteLine(inputStr);
            // - Обработка Главной Функции
            regex = new Regex(@"^f\(x\)=(?<var>((\+|-)?\d*x\d+))+->max$", RegexOptions.Multiline);
            foreach (Match m in regex.Matches(inputStr))
            {
                List<string> var = new List<string>(m.Groups["var"].Captures.Cast<Capture>().Select(x => x.Value));
                size = var.Count();
                list = new List<Dictionary<string, float>>();
                Dictionary<string, float> dict = new Dictionary<string, float>();
                var.ForEach(x =>
                {
                    string f = x.Substring(x.IndexOf("x"), x.Length - x.IndexOf("x"));
                    float s = -1*float.Parse(x.Substring(0, x.IndexOf("x")));
                    dict.Add(f, s);
                });
                list.Add(dict);
            }
            // - Обработка Ограничений
            regex = new Regex(@"(?<var>((\+|-)?(\d|\.)*x\d+))+<=(?<num>(\d|\.)+)$", RegexOptions.Multiline);
            foreach (Match m in regex.Matches(inputStr))
            {
                List<string> var = new List<string>(m.Groups["var"].Captures.Cast<Capture>().Select(x => x.Value));
                Dictionary<string, float> dict = new Dictionary<string, float>();
                var.ForEach(x =>
                {
                    string nx = x;
                    if (nx[0] == 'x')
                        nx = "1" + nx;
                    string f = nx.Substring(nx.IndexOf("x"), nx.Length - nx.IndexOf("x"));
                    float s = float.Parse(nx.Substring(0, nx.IndexOf("x")));
                    dict.Add(f, s);
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

                        double[,] table = new double[list.Count, size + 1];
                        Dictionary<string, float> t;
                        t = list[0];
                        list.RemoveAt(0);
                        list.Add(t);
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].ContainsKey("N"))
                                table[i, 0] = list[i]["N"];
                            else
                                table[i, 0] = 0;

                            for(int j=1; j<=size; j++)
                            if (list[i].ContainsKey("x"+j))
                                table[i, j] = list[i]["x"+j];
                            else
                                table[i, j] = 0;
                        }

                        double[] result = new double[2];
                        double[,] table_result;
                        Simplex S = new Simplex(table);
                        table_result = S.Calculate(result);

                        Console.WriteLine();
                        Console.WriteLine("Решенная симплекс-таблица:");
                        for (int i = 0; i < table_result.GetLength(0); i++)
                        {
                            for (int j = 0; j < table_result.GetLength(1); j++)
                                Console.Write(Math.Round(table_result[i, j], 5) + " ");
                            Console.WriteLine();
                        }

                        Console.WriteLine();
                        Console.WriteLine("Ответ:");
                        for (int i = 0; i < size; i++)
                            Console.WriteLine("X[{0}] = {1}", i+1, result[i]);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
