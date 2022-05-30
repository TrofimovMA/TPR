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
        static Dictionary<int, int[]> Elektra(List<A> As, List<K> Ks, out string[,] infoPreferences, out string graphVizStr, float threshold = 1f)
        {
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
            graphVizStr += "}";

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

                    canAchieveAll &= nodeSearch(graphExt, new List<int>() { i }, j);
                }
            if (!canAchieveAll)
                throw new GraphIsNotConnectedException("Граф не является целостным");

            // Проверка на петли
            bool loopDetected = false;
            for (int i = 1; i <= As.Count; i++)
                loopDetected |= loopSearch(graph, new List<int>() { i });
            if (loopDetected)
                throw new GraphIsLoopedException("Граф содержит петли");

            // Уровни графа
            int[] maxWay = new int[As.Count];
            for (int i = 1; i <= As.Count; i++)
                maxWay[i - 1] = getMaxWay(graph, new List<int>() { i });
            Dictionary<int, int> map = new Dictionary<int, int>();
            for (int i = 0; i < As.Count; i++)
                map[i] = maxWay[i];
            maxWay = maxWay.ToList().Distinct().OrderBy(x => x).ToArray();
            Dictionary<int, int[]> levels = new Dictionary<int, int[]>();
            for (int i = 0; i < maxWay.Length; i++)
            {
                levels[i + 1] = map.Where(x => x.Value == maxWay[i]).Select(x => x.Key + 1).ToArray();
            }

            // Альтернативы, расположенные по уровням
            return levels;
        }

        // Метод ЭЛЕКТРА
        // - Возможные Исключения
        // -- Полученный граф содержит петли
        public class GraphIsLoopedException : Exception
        {
            public GraphIsLoopedException(string message) : base(message) { }
        }
        // -- Полученный граф не является целостным
        public class GraphIsNotConnectedException : Exception
        {
            public GraphIsNotConnectedException(string message) : base(message) { }
        }

        // Работа с Графом
        // - Нахождение самого длинного пути из вершины в графе в обратную сторону
        static int getMaxWay(List<(int, int)> graph, List<int> way)
        {
            int cur = way.Last();
            int length = way.Count() - 1;
            List<int> nexts = graph.Where(x => x.Item2 == cur).Select(x => x.Item1).ToList();
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
        // - Поиск петель в графе
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
        // - Возможно ли попасть из одной вершины в другую
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
    }
}
