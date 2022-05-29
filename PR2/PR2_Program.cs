using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Library;

using Table = Library.Lib.Table;

namespace PR2
{
    static internal partial class PR2
    {
        static readonly string inputFile = Directory.GetCurrentDirectory() + @"\PR2.txt"; // Входной Файл

        static readonly List<K> Ks = new List<K>(); // Список Критериев
        static readonly List<A> As = new List<A>(); // Список Альтернатив

        // Параметры Способов Сужения Множества Парето-Оптимальных Исходов
        static readonly List<(int k, float bound)> N_Bounds = new List<(int, float)>(); // Указание Границ Критериев
        static readonly List<(char flag, float bound)> N_Suboptimization = new List<(char, float)>(); // Субоптимизация
        static readonly List<int> N_Lexicographical = new List<int>(); // Лексикографическая Оптимизация

        static readonly List<string> Commands = new List<string>(); // Последовательность Команд Программы
        static List<A> CurAs = new List<A>(); // Текущий cписок Альтернатив

        static string graphViz = String.Empty; // Код GraphViz для графа

        // Загрузка Входных Данных
        static void LoadInputData()
        {
            // Входные Данные
            string inputStr = File.ReadAllText(inputFile);

            // Обработка Входных Данных
            Regex regex; Match match;
            Dictionary<string, ScaleMark[]> scales = new Dictionary<string, ScaleMark[]>();
            List<(string name, string scale, string weight, string positive)> KsTemp = new List<(string, string, string, string)>();
            // - Обработка комментариев
            regex = new Regex(@"^\s*(.*?)\s*(\/\/.*)?$", RegexOptions.Multiline);
            inputStr = regex.Replace(inputStr, "$1");
            // - Обработка Альтернатив, Критериев и Шкал Критериев
            regex = new Regex(@"^(?<type>K|A|S)\(((\s*)|(\s*((""(?<name>.*?)"")|(?<name>.*?))\s*(,\s*(?<pars>.*?)\s*)*))\)$", RegexOptions.Multiline);
            foreach (Match m in regex.Matches(inputStr))
            {
                string name = m.Groups["name"].Value;
                List<string> pars = new List<string>();
                pars.AddRange(m.Groups["pars"].Captures.Cast<Capture>().Select(x => x.Value));
                switch (m.Groups["type"].Value)
                {
                    case "K":
                        KsTemp.Add((name, pars[0].DelQuotes(), pars[1], pars[2]));
                        break;
                    case "A":
                        List<int> intPars = pars.Select(x => int.Parse(x)).ToList();
                        As.Add(new A(name, intPars));
                        break;
                    case "S":
                        ScaleMark[] sm = new ScaleMark[pars.Count() / 2];
                        for (int i = 0; i < sm.Length; i++)
                        {
                            sm[i].Name = pars[i * 2].DelQuotes();
                            sm[i].Code = int.Parse(pars[1 + i * 2]);
                        }
                        scales.Add(name, sm);
                        break;
                }
            }
            foreach ((string name, string scale, string weight, string positive) in KsTemp)
            {
                Ks.Add(new K(name, scales[scale], int.Parse(weight), positive != "-"));
            }
            // - Обработка Списка Команд
            regex = new Regex(@"S(\s*->\s*(?<cmd>\w+(?<pars>\(\S+\))?))+");
            match = regex.Match(inputStr);
            if (match.Success)
            {
                foreach (Capture c in match.Groups["cmd"].Captures)
                    Commands.Add(c.Value);
            }

            // Частичное исправление некорректных Входных Данных
            if (Commands.Count < 1)
            {
                Commands.Add("IN_K");
                Commands.Add("IN_AK");
            }
            foreach (A a in As)
            {
                if (a.values.Count() > Ks.Count)
                    a.values = a.values.GetRange(0, Ks.Count);
                if (a.values.Count() < Ks.Count)
                {
                    a.values.AddRange(new int[Ks.Count - a.values.Count]);
                }
            }

            CurAs = new List<A>(As);
        }

        // Таблица - информация о Критериях
        static void ShowTableK(List<K> Ks)
        {
            // Создание таблицы
            Table t = new Table(Ks.Count + 1, 5);

            // Заполнение таблицы
            int row, col;
            string str = string.Empty;
            row = 0; col = 0;
            t.table[row, col++] = "Имя";
            t.table[row, col++] = "Шкала";
            t.table[row, col++] = "Код";
            t.table[row, col++] = "Вес";
            t.table[row, col++] = "Стремление";
            for (int i = 0; i < Ks.Count; i++)
            {
                row++; col = 0;
                t.table[row, col++] = Ks[i].name.ToMultiline();
                t.table[row, col++] = String.Join("\n", Ks[i].scale.Select(x => x.Name));
                t.table[row, col++] = String.Join("\n", Ks[i].scale.Select(x => x.Code));
                t.table[row, col++] = Ks[i].weight.ToString();
                t.table[row, col++] = Ks[i].positive ? "Max" : "Min";
            }

            // Оформление таблицы
            for (int j = 0; j < t.GetY(); j++)
                t.style[0, j] = "1C1";
            for (int i = 1; i < t.GetX(); i++)
            {
                t.style[i, 0] = "1L1";
                t.style[i, 1] = "0L0";
                for (int j = 2; j < t.GetY(); j++)
                    t.style[i, j] = "0R0";
            }
            t.lineSeparators = Enumerable.Range(0, t.GetX()).ToArray();

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Вывод таблицы
            Console.WriteLine(t.GetTableLineSeparator(Enumerable.Range(0, t.GetY()).ToList().Where(x => !((x == t.GetY() - 1))).ToArray()));
            Console.Write("|");

            Console.Write("Kритерии".ApplyAlign(2, t.maxColWidth.GetSum(Enumerable.Range(0, t.GetY()).ToArray()) + t.GetY() - 1));
            Console.Write("|");
            Console.WriteLine();
            t.PrintTable();
        }

        // Таблица - информация об оценках Альтернатив
        static void ShowTableAK(List<A> As, List<K> Ks)
        {
            // Создание таблицы
            Table t = new Table(As.Count + 1, Ks.Count + 2);

            // Заполнение таблицы
            int row, col;
            string str = string.Empty;
            row = 0; col = 0;
            t.table[row, col++] = "№";
            t.table[row, col++] = "Имя";
            foreach (K k in Ks)
                t.table[row, col++] = k.name.ToMultiline();
            for (int i = 0; i < As.Count; i++)
            {
                row++; col = 0;
                t.table[row, col++] = (i + 1).ToString();
                t.table[row, col++] = As[i].name.ToMultiline();
                foreach (float f in As[i].values)
                    t.table[row, col++] = f.ToString();
            }

            // Оформление таблицы
            for (int j = 0; j < t.GetY(); j++)
                t.style[0, j] = "1C1";
            for (int i = 1; i < t.GetX(); i++)
            {
                t.style[i, 0] = "1C1";
                t.style[i, 1] = "1L1";
                for (int j = 2; j < t.GetY(); j++)
                    t.style[i, j] = "0R0";
            }

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Вывод таблицы
            Console.WriteLine(t.GetTableLineSeparator(Enumerable.Range(0, t.GetY()).ToList().Where(x => !((x == 1) || (x == t.GetY() - 1))).ToArray()));
            Console.Write("|");
            Console.Write("A".ApplyAlign(2, t.maxColWidth.GetSum(new int[] { 0, 1 }) + 1) + "|");
            Console.Write("K".ApplyAlign(2, t.maxColWidth.GetSum(Enumerable.Range(2, t.GetY() - 2).ToArray()) + t.GetY() - 2 - 1));
            Console.Write("|");
            Console.WriteLine();
            t.PrintTable();
            Console.Write("|");
            Console.Write(" Вес К".ApplyAlign(0, t.maxColWidth.GetSum(new int[] { 0, 1 }) + 1) + "|");
            for (int i = 0; i < Ks.Count; i++)
                Console.Write(Ks[i].weight.ToString().ApplyAlign(2, t.maxColWidth[i + 2]) + "|");
            Console.WriteLine();
            Console.WriteLine(t.GetTableLineSeparator(new int[] { 0 }));
            Console.Write("|");
            Console.Write(" Стр К".ApplyAlign(0, t.maxColWidth.GetSum(new int[] { 0, 1 }) + 1) + "|");
            for (int i = 0; i < Ks.Count; i++)
                Console.Write((Ks[i].positive ? "Max" : "Min").ApplyAlign(2, t.maxColWidth[i + 2]) + "|");
            Console.WriteLine();
            Console.WriteLine(t.GetTableLineSeparator(new int[] { 0 }));
        }

        // Таблица - матрица предпочтений проектов, составленная методом ЭЛЕКТРА
        static void ShowTableP(string[,] infoPreferences)
        {
            // Создание таблицы
            Table t = new Table(As.Count + 1, As.Count + 1);

            // Заполнение таблицы
            t.table[0, 0] = "№/№";
            for (int i = 1; i <= As.Count; i++)
                t.table[0, i] = i.ToString();
            for (int j = 1; j <= As.Count; j++)
                t.table[j, 0] = j.ToString();
            for (int i = 1; i <= As.Count; i++)
                for (int j = 1; j <= As.Count; j++)
                {
                    t.table[i, j] = infoPreferences[i-1, j-1];
                }

            // Оформление таблицы
            for (int i = 0; i < t.GetX(); i++)
                for (int j = 0; j < t.GetY(); j++)
                    t.style[i, j] = "0C0";

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Дополнительное оформление таблицы
            for (int j = 1; j <= As.Count; j++)
                t.maxColWidth[j] = 5;

            // Вывод таблицы
            t.PrintTable();
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
                string p = String.Empty;
                if (c.IndexOf("(") != -1)
                    p = c.Substring(c.IndexOf("(") + 1, c.Length - c.IndexOf("(") - 2);
                Console.Write($"{++cmdCount}. ");
                switch (c)
                {
                    // Входные Данные
                    case "IN_AK":
                        Console.WriteLine("ТАБЛИЦА ОЦЕНОК ПРОЕКТОВ ПО КРИТЕРИЯМ");
                        ShowTableAK(As, Ks);
                        CurAs = new List<A>(As);
                        Console.WriteLine();
                        break;
                    // Таблица Критериев
                    case "IN_K":
                        Console.WriteLine("ТАБЛИЦА КРИТЕРИЕВ ДЛЯ ОЦЕНОК ПРОЕКТОВ");
                        ShowTableK(Ks);
                        Console.WriteLine();
                        break;
                    // Код GraphViz для графа
                    case "GVCODE":
                        Console.WriteLine("КОД GRAPHVIZ ДЛЯ ГРАФА");
                        Console.WriteLine($"{graphViz}");
                        Console.WriteLine();
                        break;
                    default:
                        // Метод ЭЛЕКТРА 2
                        if (c.StartsWith("ELEKTRA"))
                        {
                            float T;
                            if (String.IsNullOrEmpty(p))
                                T = 1f;
                            else
                                T = p.InterParseFloat();
                            Console.WriteLine("МЕТОД ЭЛЕКТРА (порог отбора предпочтений: T={0})", T);
                            string[,] infoPreferences; // Матрица предпочтений проектов
                            try
                            {
                                Dictionary<int, int[]> levels = Elektra(CurAs, Ks, out infoPreferences, out graphViz, T);
                                Console.WriteLine("Полная матрица предпочтений проектов:");
                                ShowTableP(infoPreferences);
                                Console.WriteLine("Решение:");
                                for (int i = 1; i <= levels.Count; i++)
                                {
                                    string levelName = String.Format("{0}-е место", i);
                                    string additional;
                                    additional = (i == 1) ? "(лучшее решение)" : (i == levels.Count) ? "(худшее решение)" : "";
                                    Console.WriteLine("{0}: {1} {2}", levelName, String.Join(", ", levels[i].Select(x => "A"+x)), additional);
                                }
                            }
                            catch (Exception e) when (e is GraphIsLoopedException || e is GraphIsNotConnectedException)
                            {
                                Console.WriteLine("Решение не получено: {0}", e.Message);
                            }
                            Console.WriteLine();
                        }
                        break;
                }
            }
        }
    }
}
