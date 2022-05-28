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

        static void Main(string[] args)
        {
            List<(int, int)> graph = new List<(int, int)>();
            graph.Add((1, 2));
            graph.Add((2, 3));
            graph.Add((3, 4));
            graph.Add((4, 5));

            int[] maxWay = new int[5];
            for (int i = 1; i <= 5; i++)
            {
                maxWay[i-1] = getMaxWay(graph, new List<int>() { i });
                Console.WriteLine(maxWay[i - 1]);
            }

            Console.ReadKey();
        }

        static int getMaxWay(List<(int, int)> graph, List<int> way)
        {
            int cur = way.Last();
            int length = way.Count()-1;
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
