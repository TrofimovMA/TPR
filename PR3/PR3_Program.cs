using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Library;

using Table = Library.Lib.Table;

namespace PR3
{
    static internal partial class PR3
    {
        static readonly string inputFile = Directory.GetCurrentDirectory() + @"\PR3.txt"; // Входной Файл

        static string Target = String.Empty; // Цель
        static readonly Dictionary<string, string> Scale = new Dictionary<string, string>(); // Шкала отн. важности
        static readonly List<K> Ks = new List<K>(); // Список Критериев
        static readonly List<A> As = new List<A>(); // Список Альтернатив
        static readonly Dictionary<string, Fraction[,]> Ms = new Dictionary<string, Fraction[,]>();

        static readonly List<string> Commands = new List<string>(); // Последовательность Команд Программы
        static List<A> CurAs = new List<A>(); // Список рассматриваемых Альтернатив

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
            // - Обработка Цели, Критериев и Альтернатив
            regex = new Regex(@"(?<type>T|K|A|S)\(((\s*(""(?<name>.*?)"")|(?<pars>.*?))\s*(,\s*(?<pars>.*?)\s*)*)\)");
            foreach (Match m in regex.Matches(inputStr))
            {
                string name = m.Groups["name"].Value;
                List<string> pars = new List<string>(m.Groups["pars"].Captures.Cast<Capture>().Select(x => x.Value));
                switch (m.Groups["type"].Value)
                {
                    case "T":
                        Target = name;
                        break;
                    case "K":
                        Ks.Add(new K(name));
                        break;
                    case "A":
                        As.Add(new A(name));
                        break;
                    case "S":
                        for (int i = 0; i < pars.Count / 2; i++)
                        {
                            Scale.Add(pars[i*2], pars[1+i*2]);
                        }
                        break;
                }
            }
            //Scale.ToList().ForEach(x => Console.WriteLine(x.Key + "->" + String.Join(", ", x.Value)));
            //As.ToList().ForEach(x => Console.WriteLine(x));
            //Ks.ToList().ForEach(x => Console.WriteLine(x));
            // - Обработка заданных Матриц
            regex = new Regex(@"(?<id>\w+)\s*>\s*M\((?<pars>\s*(?<par>.*?)\s*(,\s*(?<par>.*?)\s*)*)\)");
            foreach (Match m in regex.Matches(inputStr))
            {
                string id = m.Groups["id"].Value;
                string pars = m.Groups["pars"].Value;
                List<string> par = new List<string>(m.Groups["par"].Captures.Cast<Capture>().Select(x => x.Value));
                //Console.WriteLine(String.Join(", ", par));
                //Console.WriteLine(String.Join(", ", par.Select(x => (Fraction)x)));
                //Console.WriteLine(String.Join(", ", par.Select(x => ((Fraction)x).ToString(false))));
                int size = (int)Math.Sqrt(par.Count);
                Fraction[,] M = new Fraction[size, size];
                for (int i = 0; i < par.Count; i++)
                    M[i / size, i % size] = (Fraction)par[i];
                Ms.Add(id, M);
                //Console.WriteLine(id);
                //Console.WriteLine(pars);
                //Console.WriteLine(String.Join(", ", par.ToList()));
            }
            // - Обработка Списка Команд
            regex = new Regex(@"S(\s*->\s*(?<cmd>\w+(?<pars>\(\S+\))?))+");
            match = regex.Match(inputStr);
            if (match.Success)
            {
                foreach (Capture c in match.Groups["cmd"].Captures)
                    Commands.Add(c.Value);
            }

            CurAs = new List<A>(As);
        }

        // Таблица 1
        static void ShowInputTable(string obj, Fraction[,] M)
        {
            int X = M.GetLength(0);
            // Создание таблицы
            Table t = new Table(X+1, X+1);

            // Заполнение таблицы
            int row, col;
            string str = string.Empty;
            row = 0; col = 0;
            t.table[row, col] = obj;
            for (int i = 1; i < X + 1; i++)
                t.table[row, i] = t.table[i, col] = (obj[0] == 'T'?"K":"A") + i.ToString();
            for (int i = 1; i < X + 1; i++)
                for (int j = 1; j < X + 1; j++)
                    t.table[i, j] = M[i-1, j-1];

            // Оформление таблицы
            for (int i = 0; i < t.GetX(); i++)
                for (int j = 0; j < t.GetY(); j++)
                    t.style[i, j] = "1C1";
            //t.lineSeparators = Enumerable.Range(0, t.GetX()).ToArray();

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Вывод таблицы
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
                    t.table[i, j] = infoPreferences[i - 1, j - 1];
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
                    p = c.Substring(c.IndexOf("(") + 1, c.Length - c.IndexOf("(") - 2).Trim();
                Console.Write($"{++cmdCount}. ");
                switch (c)
                {
                    // Метод МАИ
                    case "MAI":
                        Console.WriteLine("МЕТОД МАИ");
                        int X = Ms["T"].GetLength(0);
                        Dictionary<(string, int), float> V = new Dictionary<(string, int), float>();
                        Dictionary<(string, int), float> W = new Dictionary<(string, int), float>();
                        Dictionary<string, float> SV = new Dictionary<string, float>();
                        List<string> objs = new List<string>();
                        objs.Add("T");
                        Enumerable.Range(1, 5).ToList().ForEach(x => objs.Add("K" + x.ToString()));
                        Console.WriteLine("1. СИНТЕЗ ПРИОРИТЕТОВ");
                        foreach (string obj in objs)
                        {
                            SV[obj] = 0f;
                            for (int i = 0; i < X; i++)
                            {
                                V[(obj, i)] = 1f;
                                for (int j = 0; j < X; j++)
                                {
                                    V[(obj, i)] *= Ms[obj][i, j];
                                }
                                V[(obj, i)] = (float)Math.Pow(V[(obj, i)], 1f / X);
                                SV[obj] += V[(obj, i)];
                            }
                            Console.WriteLine(String.Join(", ", V.Where(x => x.Key.Item1 == obj).Select(x => x.Value)));
                            Console.WriteLine(SV[obj]);
                            for (int i = 0; i < X; i++)
                            {
                                W[(obj, i)] = V[(obj, i)] / SV[obj];
                            }
                            Console.WriteLine(String.Join(", ", W.Where(x => x.Key.Item1 == obj).Select(x => x.Value)));
                            Console.WriteLine();
                        }
                        Console.WriteLine();
                        break;

                    default:
                        // Входные Матрицы
                        if (c.StartsWith("IN"))
                        {
                            Console.WriteLine("ТАБЛИЦА");
                            ShowInputTable(p, Ms[p]);
                            Console.WriteLine();
                        }
                        break;
                }
            }
        }
    }
}
