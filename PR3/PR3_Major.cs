/*
    ТРОФИМОВ М. А. ИКБО-15-20
    Теория Принятия Решений
    Практическая Работа #3. Метод Анализа Иерархий
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Library;

namespace PR3
{
    static internal partial class PR3
    {
        // Метод МАИ
        // - Иерархия принципа декомпозиции
        static string Target = String.Empty; // Цель - 1-ый уровень
        static readonly List<K> Ks = new List<K>(); // Критерии - 2-ой уровень
        static readonly List<A> As = new List<A>(); // Альтернативы - 3-ий уровень
        // - Шкала Отн. Важности
        static readonly Dictionary<string, string> Scale = new Dictionary<string, string>();
        // - Индекс Случайной Согласованности
        static readonly Dictionary<int, float> SI = new Dictionary<int, float>()
            { { 1, 0f }, { 2, 0f }, { 3, 0.58f }, {4, 0.9f }, {5, 1.12f },
            {6, 1.24f }, {7, 1.32f }, {8, 1.41f }, {9, 1.45f }, {10, 1.49f },
            {11, 1.51f }, {12, 1.48f }, {13, 1.56f }, {14, 1.57f }, {15, 1.59f } };
        // - Матрицы парных сравнений
        static readonly Dictionary<string, Fraction[,]> Ms = new Dictionary<string, Fraction[,]>();

        // Критерий
        class K
        {
            public string name = "K"; // Имя Критерия
            public bool positive = true; // Стремление Критерия
            public int weight = 1; // Вес Критерия
            public ScaleMark[] scale =
                new ScaleMark[3] { ("A", 1), ("B", 2), ("C", 3) }; // Шкала Критерия

            public K(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return string.Format("K(\"{0}\")", name);
            }
        }

        // Альтернатива
        class A
        {
            public string name = "A"; // Имя Альтернативы

            public A(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return string.Format("A(\"{0}\")", name);
            }
        }

        // Метод МАИ
        public static void MAI()
        {
            Console.WriteLine("МЕТОД МАИ");
            Console.WriteLine();
            // Элементы, для которых строятся матрицы парных сравнений
            List<string> objs = new List<string>();
            objs.Add("T");
            Enumerable.Range(1, Ks.Count()).ToList().ForEach(x => objs.Add("K" + x.ToString()));
            //Шаг 1. Расчет Локальных Векторов Приоритетов
            Console.WriteLine($"{cmdCount}.1. ЛОКАЛЬНЫЕ ВЕКТОРЫ ПРИОРИТЕТОВ");
            Dictionary<(string, int), float> V = new Dictionary<(string, int), float>();
            Dictionary<string, float> SV = new Dictionary<string, float>();
            Dictionary<(string, int), float> W = new Dictionary<(string, int), float>();
            foreach (string obj in objs)
            {
                int n = Ms[obj].GetLength(0);

                SV[obj] = 0f;
                for (int i = 0; i < n; i++)
                {
                    V[(obj, i)] = 1f;
                    for (int j = 0; j < n; j++)
                        V[(obj, i)] *= Ms[obj][i, j];
                    V[(obj, i)] = (float)Math.Pow(V[(obj, i)], 1f / n);
                    SV[obj] += V[(obj, i)];
                }
                for (int i = 0; i < n; i++)
                    W[(obj, i)] = V[(obj, i)] / SV[obj];

                Console.WriteLine();
                ShowTable(obj, Ms[obj], V, SV, W);
                Console.WriteLine();
            }
            //Шаг 2. Проверка Согласованности Оценки Эксперта
            Console.WriteLine($"{cmdCount}.2. СОГЛАСОВАННОСТЬ ОЦЕНКИ ЭКСПЕРТА");
            List<string> notCoherenced = new List<string>();
            Dictionary<(string, int), float> S = new Dictionary<(string, int), float>();
            Dictionary<(string, int), float> P = new Dictionary<(string, int), float>();
            Dictionary<string, float> lambdaMax = new Dictionary<string, float>();
            Dictionary<string, float> IS = new Dictionary<string, float>();
            Dictionary<string, float> OS = new Dictionary<string, float>();
            foreach (string obj in objs)
            {
                int n = Ms[obj].GetLength(0);

                for (int j = 0; j < n; j++)
                {
                    S[(obj, j)] = 0f;
                    Fraction s = new Fraction { N = 0, D = 1 };
                    for (int i = 0; i < n; i++)
                    {
                        s += Ms[obj][i, j];
                    }
                    S[(obj, j)] = s;
                    P[(obj, j)] = S[(obj, j)] * W[(obj, j)];
                }
                //Console.WriteLine("{0} SSS : [ {1} ]", obj, string.Join(", ", S.Where(x => x.Key.Item1 == obj).Select(x => x.Value.ToString(3))));
                //Console.WriteLine("{0} PPP : [ {1} ]", obj, string.Join(", ", P.Where(x => x.Key.Item1 == obj).Select(x => x.Value.ToString(3))));
                lambdaMax[obj] = 0f;
                for (int j = 0; j < n; j++)
                {
                    lambdaMax[obj] += P[(obj, j)];
                }
                //Console.WriteLine("{0} LLL : {1}", obj, lambdaMax[obj].ToString(3)); //!
                IS[obj] = (lambdaMax[obj] - n) / (n - 1);
                //Console.WriteLine("{0} ISISIS : {1}", obj, IS[obj].ToString(3)); //!
                OS[obj] = IS[obj] / SI[n];
                if (OS[obj] <= 0.10f)
                    Console.WriteLine("Матрица {0}: ОС = {1} <= 0.10 => Оценки эксперта согласованы.", obj, Math.Round(OS[obj], 3));
                else
                {
                    notCoherenced.Add(obj);
                    Console.WriteLine("Матрица {0}: ОС = {1}  > 0.10 => Оценки эксперта НЕ согласованы.", obj, Math.Round(OS[obj], 3));
                }
            }
            Console.WriteLine("ИТОГОВЫЙ РЕЗУЛЬТАТ ПРОВЕРКИ: " + 
                (notCoherenced.Count() == 0 ?
                "Все матрицы парных сравнений согласованы." :
                $"Обнаружено {notCoherenced.Count()} несогласованных матриц парных сравнений.")
                );
            Console.WriteLine();
            if (notCoherenced.Count() > 0) // Оценки не согласованы
            {
                Console.WriteLine($"{cmdCount}.3. ОТВЕТ");
                string error = String.Format("Оценки эксперта НЕ согласованы в следующих матрицах:\n{{ {0} }}", String.Join(", ", notCoherenced));
                Console.WriteLine("Решение НЕ получено:\n{0}", error);
            }
            else // Оценки согласованы
            {
                Console.WriteLine($"{cmdCount}.3. ГЛОБАЛЬНЫЕ ПРИОРИТЕТЫ АЛЬТЕРНАТИВ");
                Dictionary<int, float> Wa = new Dictionary<int, float>();
                for (int i = 0; i < As.Count; i++)
                {
                    Wa[i] = 0f;
                    for (int j = 0; j < Ks.Count; j++)
                    {
                        Wa[i] += W[("K" + (j + 1), i)] * W[("T", j)];
                    }
                }
                ShowTable(As, Ks, W, Wa);
                Console.WriteLine();
                Console.WriteLine($"{cmdCount}.4. ОТВЕТ");
                string answer = String.Join(", ", Wa.Where(x => x.Value == Wa.Select(y => y.Value).Max()).Select(z => ("A" + (z.Key + 1))));
                Console.WriteLine("Согласно МАИ предпочтение следует отдать: {{ {0} }}", answer);
            }
        }
    }
}
