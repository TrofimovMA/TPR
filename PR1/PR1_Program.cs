using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Library;

using Table = Library.Lib.Table;

namespace PR1
{
    static internal partial class PR1
    {
        static readonly string inputFile = Directory.GetCurrentDirectory() + @"\PR1.txt"; // Входной Файл

        static readonly List<K> Ks = new List<K>(); // Список Критериев
        static readonly List<A> As = new List<A>(); // Список Альтернатив

        // Параметры Способов Сужения Множества Парето-Оптимальных Исходов
        static readonly List<(int k, float bound)> N_Bounds = new List<(int, float)>(); // Указание Границ Критериев
        static readonly List<(char flag, float bound)> N_Suboptimization = new List<(char, float)>(); // Субоптимизация
        static readonly List<int> N_Lexicographical = new List<int>(); // Лексикографическая Оптимизация

        static readonly List<Command> Commands = new List<Command>(); // Последовательность Команд Программы
        static List<A> CurAs = new List<A>(); // Текущий cписок Альтернатив
        static List<A> NewAs = new List<A>(); // Новый cписок Альтернатив

        // Команды Программы, которые могут быть в Инструкции
        enum Command
        {
            CMD_IN,
            CMD_PARETO,
            CMD_NAR_BOUNDS,
            CMD_NAR_SUBOPTIMIZATION,
            CMD_NAR_LEXICOGRAPHICAL,
            CMD_CHANGES,
            CMD_OUT_FULL,
            CMD_OUT
        }

        // Загрузка Входных Данных
        static void LoadInputData()
        {
            // Входные Данные
            string inputStr = File.ReadAllText(inputFile);

            // Обработка Входных Данных
            Regex regex; Match match; string str;
            // - Обработка комментариев
            regex = new Regex(@"^(.*?)(\/\/.*)$", RegexOptions.Multiline);
            inputStr = regex.Replace(inputStr, "$1");
            // - Обработка Альтернатив и Критериев
            regex = new Regex(@"(?<type>K|A)\(((\s*)|(\s*((""(?<name>.*?)"")|(?<name>.*?))\s*(,\s*(?<pars>.*?)\s*)*))\)");
            foreach (Match m in regex.Matches(inputStr))
            {
                string name = m.Groups["name"].Value;
                List<string> pars = new List<string>();
                pars.AddRange(m.Groups["pars"].Captures.Cast<Capture>().Select(x => x.Value));
                switch (m.Groups["type"].Value)
                {
                    case "K":
                        Ks.Add(new K(name, (pars.ElementAtOrDefault(0) ?? "NULL") != "-"));
                        break;
                    case "A":
                        List<float> floatPars = pars.Select(x => x.InterParseFloat()).ToList();
                        As.Add(new A(name, floatPars));

                        break;
                }
            }
            // - Обработка параметров "Указание Границ Критериев"
            str = @"\(\s*(?<k>\d+?)\s*,\s*(?<bound>\S+)\s*\)";
            regex = new Regex($@"N_BOUNDS\(\s*{str}(\s*,\s*{str}\s*)*\s*\)");
            foreach (Match m in regex.Matches(inputStr))
            {
                for (int i = 0; i < m.Groups["k"].Captures.Count; i++)
                {
                    N_Bounds.Add((
                        int.Parse(m.Groups["k"].Captures[i].Value) - 1,
                        m.Groups["bound"].Captures[i].Value.InterParseFloat()));
                }
            }
            // - Обработка параметров "Субоптимизация"
            str = @"(?<k>\S+?)";
            regex = new Regex($@"N_SUBOPTIMIZATION\(\s*{str}\s*(\s*(,\s*{str}\s*))*\)");
            match = regex.Match(inputStr);
            for (int i = 0; i < match.Groups["k"].Captures.Count; i++)
            {
                if (match.Groups["k"].Captures[i].Value.ToUpper() == "X")
                    N_Suboptimization.Add(('X', 0f));
                else if (match.Groups["k"].Captures[i].Value.ToUpper() == "N")
                    N_Suboptimization.Add(('N', 0f));
                else
                    N_Suboptimization.Add(('\0', match.Groups["k"].Captures[i].Value.InterParseFloat()));
            }
            // - Обработка параметров "Лексикографическая Оптимизация"
            str = @"(?<k>\d+?)";
            regex = new Regex($@"N_LEXICOGRAPHICAL\(\s*{str}\s*(\s*(,\s*{str}\s*))*\)");
            match = regex.Match(inputStr);
            for (int i = 0; i < match.Groups["k"].Captures.Count; i++)
                N_Lexicographical.Add(int.Parse(match.Groups["k"].Captures[i].Value));
            // - Обработка Списка Команд
            regex = new Regex(@"S(\s*->\s*(?<cmd>\w+))+");
            match = regex.Match(inputStr);
            if (match.Success)
            {
                foreach (Capture c in match.Groups["cmd"].Captures)
                {
                    switch (c.Value)
                    {
                        case "IN":
                            Commands.Add(Command.CMD_IN);
                            break;
                        case "PARETO":
                            Commands.Add(Command.CMD_PARETO);
                            break;
                        case "N_BOUNDS":
                            Commands.Add(Command.CMD_NAR_BOUNDS);
                            break;
                        case "N_SUBOPTIMIZATION":
                            Commands.Add(Command.CMD_NAR_SUBOPTIMIZATION);
                            break;
                        case "N_LEXICOGRAPHICAL":
                            Commands.Add(Command.CMD_NAR_LEXICOGRAPHICAL);
                            break;
                        case "CHANGES":
                            Commands.Add(Command.CMD_CHANGES);
                            break;
                        case "OUT":
                            Commands.Add(Command.CMD_OUT);
                            break;
                        case "OUT_FULL":
                            Commands.Add(Command.CMD_OUT_FULL);
                            break;
                    }
                }
            }

            // Частичное исправление некорректных Входных Данных
            if (Commands.Count < 1)
                Commands.Add(Command.CMD_IN);
            foreach (A a in As)
            {
                if (a.values.Count() > Ks.Count)
                    a.values = a.values.GetRange(0, Ks.Count);
                if (a.values.Count() < Ks.Count)
                {
                    a.values.AddRange(new float[Ks.Count - a.values.Count]);
                }
            }

            CurAs = new List<A>(As);
            NewAs = new List<A>(As);
        }

        // Таблица - информация об Альтернативах и Критериях
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
        }

        // Таблица - информация об Отношениях Парето-Доминирования
        static void ShowTableR(int[,] infoRelations)
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
                    if (i <= j)
                    {
                        t.table[i, j] = "-";
                        continue;
                    }

                    // Информация об отношениях Парето-Доминирования
                    // между объектами A (с индекстом i-1) и B (с индексом j-1).
                    switch (infoRelations[i - 1, j - 1])
                    {
                        case 1: // Объект A доминирующий
                            t.table[i, j] = i.ToString();
                            break;
                        case -1: // Объект A доминируемый
                            t.table[i, j] = j.ToString();
                            break;
                        default: // Объекты A и B несравнимые
                            t.table[i, j] = "н";
                            break;
                    }
                }

            // Оформление таблицы остается по умолчанию

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
            int cmdCount = 0;
            foreach (Command c in Commands)
            {
                Console.Write($"{++cmdCount}. ");
                switch (c)
                {
                    // Входные Данные
                    case Command.CMD_IN:
                        Console.WriteLine("ВХОДНЫЕ ДАННЫЕ");
                        ShowTableAK(As, Ks);
                        Console.WriteLine("Исходное множество решений: {{ {0} }}", 
                            String.Join(", ", As.Select(x => "A" + (As.IndexOf(x) + 1))));
                        CurAs = new List<A>(As);
                        NewAs = new List<A>(As);
                        Console.WriteLine();
                        break;
                    // Метод Парето
                    case Command.CMD_PARETO:
                        Console.WriteLine("МЕТОД ПАРЕТО");
                        Console.WriteLine("Отношения Парето-доминирования:");
                        int[,] infoRelations; // Информация об отношениях
                        NewAs = Pareto(CurAs, Ks, out infoRelations); // Новый список альтернатив
                        ShowTableR(infoRelations);
                        Console.WriteLine("Исключенные доминируемые альтернативы: {0}", String.Join("; ", GetChanges(CurAs, NewAs)
                            .Where(x => x.c == '-').Select(x => x.a).Select(x => "-A" + (As.IndexOf(x) + 1))
                            ));
                        CurAs = new List<A>(NewAs);
                        Console.WriteLine("Полученное множество решений: {{ {0} }}",
                            String.Join(", ", CurAs.Select(x => "A" + (As.IndexOf(x) + 1))));
                        Console.WriteLine();
                        break;
                    // Указание Границ Критериев
                    case Command.CMD_NAR_BOUNDS:
                        Console.WriteLine("УКАЗАНИЕ ГРАНИЦ КРИТЕРИЕВ");
                        Console.WriteLine("Границы критериев: {0}",
                            String.Join(" ; ", N_Bounds
                            .Select(x => "K" + (x.k + 1) + (Ks[x.k].positive ? " >= " : " <= ") + x.bound)));
                        NewAs = Narrowing_Bounds(CurAs, Ks, N_Bounds);
                        Console.WriteLine("Сужение множества решений: {0}", String.Join("; ", GetChanges(CurAs, NewAs)
                            .Where(x => x.c == '-').Select(x => x.a).Select(x => "-A" + (As.IndexOf(x) + 1))
                            ));
                        CurAs = new List<A>(NewAs);
                        Console.WriteLine("Полученное множество решений: {{ {0} }}",
                            String.Join(", ", CurAs.Select(x => "A" + (As.IndexOf(x) + 1))));
                        Console.WriteLine();
                        break;
                    // Субоптимизация
                    case Command.CMD_NAR_SUBOPTIMIZATION:
                        Console.WriteLine("СУБОПТИМИЗАЦИЯ");
                        Console.WriteLine("Выделенный критерий: {0}",
                            ((Func<string>)(() =>
                            {
                                int t = N_Suboptimization.FindIndex(x => x.flag == 'X') + 1;
                                return t == 0 ? "" : "K" + t;
                            }
                            ))());
                        Console.WriteLine("Границы остальных критериев: {0}",
                            String.Join(" ; ", N_Suboptimization
                                .Select((x, i) => (knum: i + 1, kinfo: x)).Where(x => x.kinfo.flag == '\0')
                                .Select(x => "K" + x.knum + (Ks[x.knum - 1].positive ? " >= " : " <= ") + x.kinfo.bound)
                            )
                        );
                        NewAs = Narrowing_Suboptimization(CurAs, Ks, N_Suboptimization);
                        Console.WriteLine("Сужение множества решений: {0}", String.Join("; ", GetChanges(CurAs, NewAs)
                            .Where(x => x.c == '-').Select(x => x.a).Select(x => "-A" + (As.IndexOf(x) + 1))
                            ));
                        CurAs = new List<A>(NewAs);
                        Console.WriteLine("Полученное множество решений: {{ {0} }}",
                            String.Join(", ", CurAs.Select(x => "A" + (As.IndexOf(x) + 1))));
                        Console.WriteLine();
                        break;
                    // Лексикографическая Оптимизация
                    case Command.CMD_NAR_LEXICOGRAPHICAL:
                        Console.WriteLine($"ЛЕКСИКОГРАФИЧЕСКАЯ ОПТИМИЗАЦИЯ");
                        Console.WriteLine("Порядок критериев по их относительной важности: {0}",
                            String.Join(" > ", N_Lexicographical.Select(x => "K" + x)));
                        NewAs = Narrowing_Lexicographical(NewAs, Ks, N_Lexicographical);
                        Console.WriteLine("Сужение множества решений: {0}", String.Join("; ", GetChanges(CurAs, NewAs)
                            .Where(x => x.c == '-').Select(x => x.a).Select(x => "-A" + (As.IndexOf(x) + 1))
                            ));
                        CurAs = new List<A>(NewAs);
                        Console.WriteLine("Полученное множество решений: {{ {0} }}",
                            String.Join(", ", CurAs.Select(x => "A" + (As.IndexOf(x) + 1))));
                        Console.WriteLine();
                        break;
                    // Полученное Множество Решений
                    case Command.CMD_CHANGES:
                        Console.WriteLine($"ПОЛУЧЕННОЕ МНОЖЕСТВО РЕШЕНИЙ");
                        Console.WriteLine("{{ {0} }}", String.Join(", ", CurAs.Select(x => "A" + (As.IndexOf(x) + 1))));
                        Console.WriteLine();
                        break;
                    case Command.CMD_OUT_FULL:
                        // Итоговое Множество Решений (подробно)
                        Console.WriteLine($"ИТОГОВОЕ МНОЖЕСТВО РЕШЕНИЙ (ПОДРОБНО)");
                        ShowTableAK(NewAs, Ks);
                        Console.WriteLine();
                        break;
                    case Command.CMD_OUT:
                        // Итоговое Множество Решений
                        Console.WriteLine($"ИТОГОВОЕ МНОЖЕСТВО РЕШЕНИЙ");
                        Console.WriteLine("{{ {0} }}", String.Join(", ", CurAs.Select(x => "A" + (As.IndexOf(x) + 1))));
                        Console.WriteLine();
                        break;
                }
            }
        }
    }
}
