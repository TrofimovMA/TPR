using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Library;

using Table = Library.Lib.Table;

namespace PR7
{
    static internal partial class PR7
    {
        static readonly string inputFile = Directory.GetCurrentDirectory() + @"\PR7.txt"; // Входной Файл

        static readonly List<string> Commands = new List<string>(); // Последовательность Команд Программы

        static readonly List<A> As = new List<A>(); // Список Поставщиков
        static readonly List<B> Bs = new List<B>(); // Список Потребителей
        static readonly Dictionary<(A a, B b), float> Cs = new Dictionary<(A a, B b), float>(); // Стоимость Поставки
        static Dictionary<(A a, B b), float> Ts; // Кол-во Груза Поставки

        // Таблица - Информация о Поставщиках, Потребителях и Стоимости
        static void ShowTableABC(List<A> As, List<B> Bs, Dictionary<(A a, B b), float> C)
        {
            // Создание таблицы
            Table t = new Table(As.Count + 2, Bs.Count + 2);

            // Заполнение таблицы
            int row, col;
            string str = string.Empty;
            row = 0; col = 0;
            t.table[row, col++] = "Пункты";
            for (int i = 0; i < Bs.Count; i++)
                t.table[row, col++] = $"B{i+1}";
            t.table[row, col++] = "Запасы";
            for (int i = 0; i < As.Count; i++)
            {
                row++; col = 0;
                t.table[row, col++] = $"A{i+1}";
                for (int j = 0; j < Bs.Count; j++)
                    t.table[row, col++] = $"{C[(As[i], Bs[j])]}";
                t.table[row, col++] = $"{As[i].supply}";
            }
            row++; col = 0;
            t.table[row, col++] = "Потребности";
            for (int j = 0; j < Bs.Count; j++)
                t.table[row, col++] = $"{Bs[j].need}";
            t.table[row, col++] = $"{Bs.Select(x => x.need).Sum()}";

            // Оформление таблицы
            for (int j = 0; j < t.GetY(); j++)
                t.style[0, j] = "1C1";
            for (int i = 1; i < t.GetX(); i++)
            {
                t.style[i, 0] = "1C1";
                for (int j = 1; j < t.GetY()-1; j++)
                    t.style[i, j] = "0R0";
                t.style[i, t.GetY()-1] = "1C1";
            }
            t.lineSeparators = new int[] { 0, t.GetX() - 2, t.GetX() - 1 };

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Вывод таблицы
            t.PrintTable();
        }

        // Таблица - Информация о Поставщиках, Потребителях, Стоимости и Кол-вах Груза
        static void ShowTableABCT(List<A> As, List<B> Bs, Dictionary<(A a, B b), float> C, Dictionary<(A a, B b), float> T)
        {
            // Создание таблицы
            Table t = new Table(As.Count + 2, Bs.Count + 2);

            // Заполнение таблицы
            int row, col;
            string str = string.Empty;
            row = 0; col = 0;
            t.table[row, col++] = "Пункты";
            for (int i = 0; i < Bs.Count; i++)
                t.table[row, col++] = $"B{i + 1}";
            t.table[row, col++] = "Запасы";
            for (int i = 0; i < As.Count; i++)
            {
                row++; col = 0;
                t.table[row, col++] = $"A{i + 1}";
                for (int j = 0; j < Bs.Count; j++)
                    if(T[(As[i], Bs[j])]>-1)
                        t.table[row, col++] = $"{T[(As[i], Bs[j])]} * {C[(As[i], Bs[j])]}";
                    else
                        t.table[row, col++] = $" * {C[(As[i], Bs[j])]}";
                t.table[row, col++] = $"{As[i].supply}";
            }
            row++; col = 0;
            t.table[row, col++] = "Потребности";
            for (int j = 0; j < Bs.Count; j++)
                t.table[row, col++] = $"{Bs[j].need}";
            t.table[row, col++] = $"{Bs.Select(x => x.need).Sum()}";

            // Оформление таблицы
            for (int j = 0; j < t.GetY(); j++)
                t.style[0, j] = "1C1";
            for (int i = 1; i < t.GetX(); i++)
            {
                t.style[i, 0] = "1C1";
                for (int j = 1; j < t.GetY() - 1; j++)
                    t.style[i, j] = "0R0";
                t.style[i, t.GetY() - 1] = "1C1";
            }
            t.lineSeparators = new int[] { 0, t.GetX() - 2, t.GetX() - 1 };

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Вывод таблицы
            t.PrintTable();
        }

        // Таблица - Итерация Метода Потенциалов
        static void ShowTablePotentialIteration(List<A> As, List<B> Bs, Dictionary<(A a, B b), float> C, float[,] T, float[] u, float[] v, List<(int x, int y)> path)
        {
            // Создание таблицы
            Table t = new Table(As.Count + 2, Bs.Count + 2);

            // Заполнение таблицы
            int row, col;
            string str = string.Empty;
            row = 0; col = 0;
            t.table[row, col++] = "Пункты";
            for (int i = 0; i < Bs.Count; i++)
                t.table[row, col++] = $"B{i + 1}\n(v{i + 1}={v[i]})";
            t.table[row, col++] = "Запасы";
            for (int i = 0; i < As.Count; i++)
            {
                row++; col = 0;
                t.table[row, col++] = $"A{i + 1}\n(u{i + 1}={u[i]})";
                for (int j = 0; j < Bs.Count; j++)
                    t.table[row, col++] = $"{T[i, j]} * {C[(As[i], Bs[j])]}";
             
                t.table[row, col++] = $"{As[i].supply}";
            }
            row++; col = 0;
            t.table[row, col++] = "Потребности";
            for (int j = 0; j < Bs.Count; j++)
                t.table[row, col++] = $"{Bs[j].need}";
            t.table[row, col++] = $"{Bs.Select(x => x.need).Sum()}";
            // - Заполнение Пути
            List<(int x, int y)> fullPath = new List<(int x, int y)>(path);
            for (int k = 0; k < fullPath.Count - 1; k++)
            {
                (int x, int y) nextPoint = (fullPath[k].x, fullPath[k].y);
                do
                {
                    nextPoint.x += (fullPath[k + 1].x > nextPoint.x) ? 1 : (fullPath[k + 1].x < nextPoint.x) ? -1 : 0;
                    nextPoint.y += (fullPath[k + 1].y > nextPoint.y) ? 1 : (fullPath[k + 1].y < nextPoint.y) ? -1 : 0;
                    if (nextPoint == fullPath[k + 1])
                        break;
                    fullPath.Insert(k++ + 1, nextPoint);
                } while (1 > 0);
            }
            for (int k = 0; k < fullPath.Count - 1; k++)
            {
                int dx = fullPath[k + 1].y - fullPath[k].y;
                int dy = fullPath[k + 1].x - fullPath[k].x;

                string movement;
                if (dx < 0)
                    movement = $"{(char)17}";
                else if (dx > 0)
                    movement = $"{(char)16}";
                else if(dy > 0)
                    movement = $"{(char)31}";
                else
                    movement = $"{(char)30}";
                t.table[fullPath[k].x+1, fullPath[k].y+1] += $"\n{movement}";
            }
            // - Расставление знаков +/-
            for(int k= 0; k < path.Count - 1; k++)
            {
                string sign;
                sign = ((k + 1) % 2 == 1) ? "+" : "-";
                t.table[path[k].x + 1, path[k].y + 1] += $" ({sign})\n";
            }

            // Оформление таблицы
            for (int j = 0; j < t.GetY(); j++)
                t.style[0, j] = "1C1";
            for (int i = 1; i < t.GetX(); i++)
            {
                t.style[i, 0] = "1C1";
                for (int j = 1; j < t.GetY() - 1; j++)
                    t.style[i, j] = "1C1";
                t.style[i, t.GetY() - 1] = "1C1";
            }
            t.lineSeparators = Enumerable.Range(0, t.GetX()).ToArray();

            // Обновление характеристик таблицы, зависящих от ее содержимого, перед ее выводом
            t.UpdateInfo();

            // Вывод таблицы
            t.PrintTable();
        }

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
            // - Обработка Поставщиков (A) и Потребителей (B)
            regex = new Regex(@"(?<type>A|B)\((\s*(?<pars>.*?)\s*(,\s*(?<pars>.*?)\s*)*)\)");
            foreach (Match m in regex.Matches(inputStr))
            {
                string name = m.Groups["name"].Value;
                List<string> pars = new List<string>(m.Groups["pars"].Captures.Cast<Capture>().Select(x => x.Value));
                List<float> floatPars = pars.Select(x => x.InterParseFloat()).ToList();
                switch (m.Groups["type"].Value)
                {
                    case "A": // Поставщик
                        As.Add(new A(floatPars));
                        break;
                    case "B": // Потребитель
                        Bs.Add(new B(floatPars));
                        break;
                }
            }
            // - Обработка Стоимости Поставок
            regex = new Regex(@"(?<id>C)\s*>\s*M\((?<pars>\s*(?<par>.*?)\s*(,\s*(?<par>.*?)\s*)*)\)");
            foreach (Match m in regex.Matches(inputStr))
            {
                string pars = m.Groups["pars"].Value;
                List<string> par = new List<string>(m.Groups["par"].Captures.Cast<Capture>().Select(x => x.Value));
                List<float> floatPar = par.Select(x => x.InterParseFloat()).ToList();
                int par_id = 0;
                foreach(A a in As)
                    foreach(B b in Bs)
                        Cs.Add((a, b), floatPar[par_id++]);
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

        // Получение Итоговой Стоимости
        static float GetTotalCost(Dictionary<(A a, B b), float> Ts, Dictionary<(A a, B b), float> Cs)
        {
            float sum = 0;

            // Проход по всем Поставкам
            Ts.ToList().ForEach(x =>
            {
                // Прибавление Стоимости Поставки = Кол-во Груза * Стоимость Доставки 1-ой Единицы Груза
                sum += (x.Value>-1? x.Value : 0) * Cs.ToList().Find(y => x.Key == y.Key).Value;
            });

            return sum;
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
                    case "IN":
                        Console.WriteLine("ВХОДНЫЕ ДАННЫЕ");
                        ShowTableABC(As, Bs, Cs);
                        Console.WriteLine();
                        break;
                    // Метод Северо-Западного Угла
                    case "NW_CORNER":
                        Console.WriteLine("МЕТОД СЕВЕРО-ЗАПАДНОГО УГЛА");
                        Ts = NW_Corner(As, Bs);
                        ShowTableABCT(As, Bs, Cs, Ts);
                        Console.WriteLine("Итоговая стоимость перевозок = {0} (ед.)", GetTotalCost(Ts, Cs));
                        Console.WriteLine();
                        break;
                    // Метод Минимальной Стоимости
                    case "MIN_COST":
                        Console.WriteLine("МЕТОД МИНИМАЛЬНОЙ СТОИМОСТИ");
                        Ts = MinCost(As, Bs, Cs);
                        ShowTableABCT(As, Bs, Cs, Ts);
                        Console.WriteLine("Итоговая стоимость перевозок = {0} (ед.)", GetTotalCost(Ts, Cs));
                        Console.WriteLine();
                        break;
                    // Метод Потенциалов
                    case "POTENTIAL":
                        Console.WriteLine("МЕТОД ПОТЕНЦИАЛОВ");
                        Ts = Potential(As, Bs, Cs, Ts);
                        ShowTableABCT(As, Bs, Cs, Ts);
                        Console.WriteLine("Итоговая стоимость перевозок = {0} (ед.)", GetTotalCost(Ts, Cs));
                        Console.WriteLine();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
