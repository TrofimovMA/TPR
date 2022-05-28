/*
    ТРОФИМОВ М. А. ИКБО-15-20
    Теория Принятия Решений
    Практическая Работа #2. Метод ЭЛЕКТРА 2
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Library;

namespace PR2
{
    static internal partial class PR2
    {
        // Критерий
        class K
        {
            public string name = "K"; // Имя Критерия
            public bool positive = true; // Стремление Критерия
            public int weight = 1; // Вес Критерия
            public ScaleMark[] scale =
                new ScaleMark[3] { ("A", 1), ("B", 2), ("C", 3) }; // Шкала Критерия

            public K(string name, ScaleMark[] scale, int weight, bool positive)
            {
                this.name = name;
                this.scale = scale;
                this.weight = weight;
                this.positive = positive;
            }

            public override string ToString()
            {
                return string.Format("K(\"{0}\", {1}, {2}, {3})",
                    name,
                    "[" + String.Join("| ", scale.Select(x => x.ToString())) + "]",
                    weight,
                    positive ? "+" : "-");
            }
        }

        // Альтернатива
        class A
        {
            public string name = "A"; // Имя Альтернативы
            public List<int> values = new List<int>(); // Оценки Альтернативы

            public A(string name, List<int> pars)
            {
                this.name = name;
                this.values = new List<int>(pars);
            }

            public override string ToString()
            {
                return string.Format("A(\"{0}\", {1})", name, string.Join(", ", values));
            }
        }

        // Метод ЭЛЕКТРА
        // - Главная Функция
        // ( Составление матрицы предпочтений проектов )
        static bool Elektra(List<A> As, List<K> Ks, out string[,] infoPreferences, out string graphVizStr, float threshold = 1f)
        {
            bool isSolutionReady = true;

            infoPreferences = new string[As.Count, As.Count];
            for (int i = 0; i < As.Count; i++)
                for (int j = 0; j < As.Count; j++)
                {
                    int P = 0, N = 0; float D; string r;
                    for (int k = 0; k < Ks.Count; k++)
                        if ((As[i].values[k] > As[j].values[k] && Ks[k].positive) || (As[i].values[k] < As[j].values[k] && !Ks[k].positive))
                        { P += Ks[k].weight; N += 0; }
                        else if ((As[i].values[k] < As[j].values[k] && Ks[k].positive) || (As[i].values[k] > As[j].values[k] && !Ks[k].positive))
                        { P += 0; N += Ks[k].weight; }
                        else { P += 0; N += 0; };
                    if (i == j)
                        r = "x";
                    else if (N == 0)
                        r = (P == 0) ? "-" : "inf";
                    else
                    {
                        D = P * 1f / N;
                        r = (D <= threshold) ? "-" : Math.Round(D, 1).ToString();
                    }
                    infoPreferences[i, j] = r;
                }

            // Получение графа
            List<(int, int)> graph = new List<(int, int)>();
            for (int i = 0; i < infoPreferences.GetLength(0); i++)
                for (int j = 0; j < infoPreferences.GetLength(1); j++)
                    if (infoPreferences[i, j] != "-" && infoPreferences[i, j] != "x") graph.Add((i + 1, j + 1));

            // Формирование кода GraphViz
            graphVizStr = "";
            graphVizStr += "digraph D {\r\n";
            foreach ((int, int) ii in graph)
                graphVizStr += "  \"" + ii.Item1 + "\" -> \"" + ii.Item2 + "\";\r\n";
            graphVizStr += "}\r\n";
            Console.Write($"GraphViz Сode:\r\n\r\n{graphVizStr}");

            // Проверка на целостность графа
            List<(int, int)> graphExt = new List<(int, int)> (graph);
            for (int i = 0; i < graph.Count(); i++)
                graphExt.Add((graph[i].Item2, graph[i].Item1));
            bool canAchieveAll = true;
            for (int i = 1; i <= As.Count; i++)
                for (int j = 1; j <= As.Count; j++)
                {
                    if (i == j)
                        continue;

                    Console.WriteLine(i + " / " + j + " / " + nodeSearch(graphExt, new List<int>() { i }, j));
                    canAchieveAll &= nodeSearch(graphExt, new List<int>() { i }, j);
                }
            Console.WriteLine(canAchieveAll);

            // Проверка на петли
            bool loopDetected = false;
            for (int i = 1; i <= As.Count; i++)
                loopDetected |= loopSearch(graph, new List<int>() { i });
            Console.WriteLine(loopDetected);

            // Уровни графа
            int[] maxWay = new int[As.Count];
            for (int i = 1; i <= As.Count; i++)
            {
                maxWay[i - 1] = getMaxWay(graph, new List<int>() { i });
                Console.WriteLine(maxWay[i - 1]);
            }


            // Получено ли решение?
            return isSolutionReady;
        }

        static int getMaxWay(List<(int, int)> graph, List<int> way)
        {
            int cur = way.Last();
            int length = way.Count() - 1;
            List<int> nexts = graph.Where(x => x.Item1 == cur).Select(x => x.Item2).ToList();
            foreach (var x in nexts)
            {
                if (way.Contains(x))
                    return length;

                List<int> newWay = new List<int>(way);
                newWay.Add(x);
                length = Math.Max(length, getMaxWay(graph, newWay));
            }
            return length;
        }

        static bool loopSearch(List<(int, int)> graph, List<int> way)
        {
            int cur = way.Last();
            bool answer = false;
            List<int> nexts = graph.Where(x => x.Item1 == cur).Select(x => x.Item2).ToList();
            foreach (var x in nexts)
            {
                if (way.Contains(x))
                    return true;

                List<int> newWay = new List<int>(way);
                newWay.Add(x);
                answer |= loopSearch(graph, newWay);
            }
            return answer;
        }

        static bool nodeSearch(List<(int, int)> graph, List<int> way, int dest)
        {
            int cur = way.Last();
            bool answer = false;
            List<int> nexts = graph.Where(x => x.Item1 == cur).Select(x => x.Item2).ToList();
            foreach (var x in nexts)
            {
                if (way.Contains(x))
                    continue;

                if (x == dest)
                    return true;

                List<int> newWay = new List<int>(way);
                newWay.Add(x);
                answer |= nodeSearch(graph, newWay, dest);
            }
            return answer;
        }

        // Метод Парето
        // - Отношение Парето-Доминирования
        // ( Определение отношения Парето-Доминирования между 2-мя объектами )
        static int Pareto_Compare(A a, A b, List<K> Ks)
        {
            // Объект A (1-ый объект) и Объект B (2-ой объект)

            // Условие 1: объект A по всем критериям не хуже объекта B
            bool flag1 = true;
            for (int i = 0; i < Ks.Count(); i++)
                if (Ks[i].positive && a.values[i] < b.values[i] || !Ks[i].positive && a.values[i] > b.values[i])
                    flag1 = false;

            // Условие 2: объект A хотя бы по одному критерию лучше объекта B
            bool flag2 = false;
            for (int i = 0; i < Ks.Count(); i++)
                if (Ks[i].positive && a.values[i] > b.values[i] || !Ks[i].positive && a.values[i] < b.values[i])
                {
                    flag2 = true;
                    break;
                }

            // Полученное отношение Парето-Доминирования
            // Случай 1: Объект A - Доминирующий, объект B - Доминируемый
            if (flag1 && flag2)
                return 1;
            // Случай 2: Объект A - Доминируемый, объект B - Доминирующий
            if (!flag1 && !flag2)
                return -1;
            // Случай 3: Объекты A и B - Несравнимые
            return 0;
        }

        // Сужение Множества Парето-Оптимальных Решений
        // - Указание Границ Критериев
        // ( Сужение Множества Парето-Оптимальных Решений при помощи метода Указания Границ Критериев )
        static List<A> Narrowing_Bounds(List<A> As, List<K> Ks, List<(int, float)> Bounds)
        {
            List<A> NewAs = new List<A>(As);

            // Проверка соответствия Границам
            foreach (A a in As)
            {
                foreach ((int k, float bound) in Bounds)
                {
                    if (Ks[k].positive && a.values[k] < bound || !Ks[k].positive && a.values[k] > bound)
                    {
                        NewAs.Remove(a);
                        break;
                    }
                }
            }

            // Полученное Суженное Множество Парето-Оптимальных Решений
            return NewAs;
        }

        // Сужение Множества Парето-Оптимальных Решений
        // - Субоптимизация
        // ( Сужение Множества Парето-Оптимальных Решений при помощи метода Субоптимизации )
        static List<A> Narrowing_Suboptimization(List<A> As, List<K> Ks, List<(char flag, float bound)> Suboptimization)
        {
            // Выделенный Критерий
            int mainK = Suboptimization.FindIndex(x => x.flag == 'X');

            // Формирования множества исходов, оценки которых по Остальным Критериям не ниже назначенных
            List<A> OldAs = new List<A>(As);
            List<A> NewAs = new List<A>(OldAs);
            foreach (A a in OldAs)
            {
                for (int k = 0; k < Suboptimization.Count; k++)
                {
                    float flag = Suboptimization[k].flag;
                    float bound = Suboptimization[k].bound;
                    if (Suboptimization[k].flag == 'X' || Suboptimization[k].flag == 'N')
                        continue;
                    if (Ks[k].positive && a.values[k] < bound || !Ks[k].positive && a.values[k] > bound)
                    {
                        NewAs.Remove(a);
                        break;
                    }
                }
            }
            // Определение исходов, максимизирующих Выделенный Критерий
            if (mainK == -1 || !NewAs.Any())
                return NewAs;
            OldAs = NewAs;
            NewAs = new List<A>(OldAs);
            float max = Ks[mainK].positive ?
                NewAs.Select(x => x.values[mainK]).Max() :
                NewAs.Select(x => x.values[mainK]).Min();
            foreach (A a in OldAs)
            {
                if (Ks[mainK].positive && a.values[mainK] >= max || !Ks[mainK].positive && a.values[mainK] <= max)
                    continue;
                NewAs.Remove(a);
            }

            // Полученное Суженное Множество Парето-Оптимальных Решений
            return NewAs;
        }

        // Сужение Множества Парето-Оптимальных Решений
        // - Лексикографическая Оптимизация
        // ( Сужение Множества Парето-Оптимальных Решений при помощи метода Лексикографической Оптимизации )
        static List<A> Narrowing_Lexicographical(List<A> As, List<K> Ks, List<int> Lexicographical)
        {
            if (!As.Any())
                return As;

            // Последовательное прохождение по Критериям, Упорядоченным по их Относительной Важности
            List<A> NewAs = new List<A>(As);
            foreach (int i in Lexicographical)
            {
                int k = i - 1;
                // Отбор исходов, которые имеют максимальную оценку по Текущему Критерию
                float max = Ks[k].positive ?
                    NewAs.Select(x => x.values[k]).Max() :
                    NewAs.Select(x => x.values[k]).Min();

                NewAs.RemoveAll(x => (Ks[k].positive && x.values[k] < max || !Ks[k].positive && x.values[k] > max));

                if (NewAs.Count < 2)
                    break;
            }

            // Полученное Суженное Множество Парето-Оптимальных Решений
            return NewAs;
        }
    }
}
