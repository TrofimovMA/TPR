using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library;
using System.IO;
using System.Text.RegularExpressions;

namespace Dev
{
    internal static class Dev
    {
        static readonly string inputFile = @"G:\VSProjects\TPR\PR2\PR2.txt"; // Входной Файл

        public class GraphIsLoopedException : Exception
        {
            public GraphIsLoopedException(string message) : base(message) { }
        }

        public class GraphIsNotConnectedException: Exception
        {
            public GraphIsNotConnectedException(string message) : base(message) { }
        }



        static void Main(string[] args)
        {
            return;
            List<(int, int)> graph = new List<(int, int)>();
            graph.Add((1, 6));
            graph.Add((3, 3));
            graph.Add((4, 5));
            graph.Add((4, 6));
            graph.Add((4, 5));
            graph.Add((5, 1));
            graph.Add((5, 2));
            graph.Add((5, 6));
            graph.Add((7, 6));

            // Уровни графа
            int[] maxWay = new int[7];
            for (int i = 1; i <= 7; i++)
                maxWay[i - 1] = getMaxWay(graph, new List<int>() { i });
            Dictionary<int, int> map = new Dictionary<int, int>();
            for (int i = 0; i < 7; i++)
                map[i] = maxWay[i];
            map.ToList().ForEach(x => Console.WriteLine((x.Key+1) + "->" + String.Join(", ", x.Value))); //DEBUG
            maxWay = maxWay.ToList().Distinct().OrderByDescending(x => x).ToArray();
            Dictionary<int, int[]> levels = new Dictionary<int, int[]>();

            Console.ReadKey();
        }

        static int getMaxWay(List<(int, int)> graph, List<int> way)
        {
            int cur = way.Last();
            int length = way.Count() - 1;
            // В обратную сторону
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

        // Критерий
        class K
        {
            public string name = "K"; // Имя Критерия
            public bool positive = true; // Стремление Критерия
            public int weigtht = 1; // Вес Критерия
            public ScaleMark[] scale =
                new ScaleMark[3] { ("A", 1), ("B", 2), ("C", 3) }; // Шкала Критерия

            public K() { }

            public K(string name, bool positive, ScaleMark[] scale)
            {
                this.name = name;
                this.positive = positive;
                this.scale = scale;
            }

            public override string ToString()
            {
                return string.Format("K(\"{0}\", {1}, {2})", name, positive ? "+" : "-", scale);
            }
        }

        // Деление шкалы
        struct ScaleMark
        {
            public string Name;
            public int Code;

            // ScaleMark a = ((string)name, (int)code)
            public static implicit operator ScaleMark((string name, int code) info)
            {
                return new ScaleMark { Name = info.name, Code = info.code };
            }

            // (string name, int code) a = (ScaleMark)sm
            public static implicit operator (string name, int code)(ScaleMark sm)
            {
                return (sm.Name, sm.Code);
            }

            public override string ToString()
            {
                return string.Format("SM(\"{0}\", {1})", Name, Code);
            }
        }
    }
}
