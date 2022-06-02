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

        static readonly List<string> Commands = new List<string>(); // Последовательность Команд Программы
        static int cmdCount; // Номер текущей команды

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
            // - Обработка заданных Матриц
            regex = new Regex(@"(?<id>\w+)\s*>\s*M\((?<pars>\s*(?<par>.*?)\s*(,\s*(?<par>.*?)\s*)*)\)");
            foreach (Match m in regex.Matches(inputStr))
            {
                string id = m.Groups["id"].Value;
                string pars = m.Groups["pars"].Value;
                List<string> par = new List<string>(m.Groups["par"].Captures.Cast<Capture>().Select(x => x.Value));
                int size = (int)Math.Sqrt(par.Count);
                Fraction[,] M = new Fraction[size, size];
                for (int i = 0; i < par.Count; i++)
                    M[i / size, i % size] = (Fraction)par[i];
                Ms.Add(id, M);
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

        // Таблица - Матрица Парных Сравнений
        static void ShowInputTable(string obj, Fraction[,] M)
        {
            int X = M.GetLength(0);
            // Создание таблицы
            Table t = new Table(X+1, X+1);

            // Заполнение таблицы
            int row, col;
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

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Вывод таблицы
            t.PrintTable();
        }

        // Таблица - Вычислением Вектора Локальных Приоритетов
        static void ShowTable(string obj, Fraction[,] M, Dictionary<(string, int), float> V, Dictionary<string, float> SV, Dictionary<(string, int), float> W)
        {
            int X = M.GetLength(0);
            // Создание таблицы
            Table t = new Table(X + 1, X + 3);

            // Заполнение таблицы
            int row, col;
            row = 0; col = 0;
            t.table[row, col] = obj;
            for (int i = 1; i < X + 1; i++)
                t.table[row, i] = t.table[i, col] = (obj[0] == 'T' ? "K" : "A") + i.ToString();
            t.table[row, X + 1] = String.Format("V[{0} i]", obj);
            t.table[row, X + 2] = String.Format("W[{1} {0} i]", obj, (obj == "T") ? "2" : "3");
            for (int i = 1; i < X + 1; i++)
            {
                for (int j = 1; j < X + 1; j++)
                    t.table[i, j] = M[i - 1, j - 1];
            }
            for(int i = 0; i < X; i++)
            {
                t.table[i+1, X+1] = Math.Round(V[(obj, i)], 3).ToString();
                t.table[i+1, X+2] = Math.Round(W[(obj, i)], 3).ToString();
            }

            // Оформление таблицы
            for (int i = 0; i < t.GetX(); i++)
                for (int j = 0; j < t.GetY(); j++)
                    t.style[i, j] = "1C1";

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Вывод таблицы
            t.PrintTable();

            // Нижняя строка с суммой
            Console.Write("|");
            Console.Write("S".ApplyAlign(2, t.maxColWidth.GetSum(Enumerable.Range(0, X+1).ToArray())+X+1-1) + "|");
            Console.Write(Math.Round(SV[obj], 3).ToString().ApplyAlign(2, t.maxColWidth[X+1]) + "|");
            Console.Write(1.ToString().ApplyAlign(2, t.maxColWidth[X + 2]) + "|");
            Console.WriteLine();
            Console.WriteLine(t.GetTableLineSeparator(new int[] { 0 }));
        }

        // Таблица - Вычисление Вектора Глобальных Приоритетов
        static void ShowTable(List<A> As, List<K> Ks, Dictionary<(string, int), float> W, Dictionary<int, float> Wa)
        {
            int X = As.Count;
            int Y = Ks.Count;
            // Создание таблицы
            Table t = new Table(X+2, Y+2);

            // Заполнение таблицы
            int row, col;
            row = 0; col = 0;
            t.table[row, col] = "";
            for (int i = 0; i < X; i++)
                t.table[i+1, col] = As[i].name;
            for (int j = 0; j < Y; j++)
                t.table[row, j+1] = Ks[j].name.ToMultiline();
            t.table[row, Y + 1] = String.Format("Глобальный\nПриоритет\nW[i]");
            for (int j = 0; j < Y; j++)
                t.table[X + 1, j + 1] = W[("T", j)].ToString(3);
            for (int i = 1; i < X + 1; i++)
            {
                for (int j = 1; j < Y + 1; j++)
                {
                    t.table[i, j] = W[(("K" + (j)).ToString(), i-1)].ToString(3);
                }
            }
            for (int i = 0; i < X; i++)
            {
                t.table[i + 1, Y + 1] = Wa[i].ToString(3);
            }

            // Оформление таблицы
            for (int i = 0; i < t.GetX(); i++)
                for (int j = 0; j < t.GetY(); j++)
                    t.style[i, j] = "1C1";
            t.lineSeparators = new int[] { 0, X, X+1 };

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

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
            cmdCount = 0;
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
                        MAI();
                        Console.WriteLine();

                        break;
                    default:
                        // Входные Матрицы (Матрицы Парных Сравнений)
                        if (c.StartsWith("IN"))
                        {
                            Console.WriteLine("МАТРИЦА ПАРНЫХ СРАВНЕНИЙ ДЛЯ ЭЛЕМЕНТА «{0}»", p);
                            ShowInputTable(p, Ms[p]);
                            Console.WriteLine();
                        }
                        break;
                }
            }
        }
    }
}
