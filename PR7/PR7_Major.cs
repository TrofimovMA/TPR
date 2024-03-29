﻿/*
    ТРОФИМОВ М. А. ИКБО-15-20
    Теория Принятия Решений
    Практическая Работа #7. Транспортная Задача
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace PR7
{
    static internal partial class PR7
    {
        // Поставщик
        class A
        {
            public float supply; // Запас

            public A(float supply)
            {
                this.supply = supply;
            }

            public A(List<float> pars) : this(pars[0]) { }

            public override string ToString()
            {
                return string.Format("A({0})", supply);
            }
        }

        // Потребитель
        class B
        {
            public float need; // Потребность

            public B(float need)
            {
                this.need = need;
            }

            public B(List<float> pars) : this(pars[0]) { }

            public override string ToString()
            {
                return string.Format("B({0})", need);
            }
        }

        // Метод Северо-Западного Угла
        static Dictionary<(A a, B b), float> NW_Corner(List<A> As, List<B> Bs)
        {
            Dictionary<(A a, B b), float> Ts = new Dictionary<(A a, B b), float>(); // План Поставок

            List<float> curSupply = new List<float>(As.Select(a => a.supply)); // Текущий Запас Поставщика
            List<float> curNeed = new List<float>(Bs.Select(b => b.need)); // Текущая Потребность Потребителя

            // Таблица Поставок
            float[,] t = new float[As.Count, Bs.Count];
            for (int i = 0; i < As.Count; i++)
                for (int j = 0; j < Bs.Count; j++)
                    t[i, j] = -1;

            // Северо-Западный Угол
            int x = 0, y = 0;
            // Заполнение ячеек Таблицы Поставок
            // Условие выхода из цикла: пока Запасы полностью не распределены (и, соответственно, Потребности не удовлетворены)
            while (curSupply.Sum() > 0)
            {
                // Определение Кол-ва Груза новой поставки
                float transport = Math.Min(curSupply[x], curNeed[y]);
                t[x, y] = transport;

                // Учет новой поставки
                curSupply[x] -= transport;
                curNeed[y] -= transport;

                // Если Запасы Поставщика полностью израсходованы, то
                // осуществляется соответствующий сдвиг по таблице
                if (curSupply[x] <= 0)
                    x++;

                // Если Потребности Потребителя полностью удовлетворены, то
                // осуществляется соответствующий сдвиг по таблице
                if (curNeed[y] <= 0)
                    y++;

            }

            // Представление Результата в типе данных «Словарь»
            for (int i = 0; i < As.Count; i++)
                for (int j = 0; j < Bs.Count; j++)
                    Ts.Add((As[i], Bs[j]), t[i, j]);

            return Ts;
        }

        // Метод Минимальной Стоимости
        static Dictionary<(A a, B b), float> MinCost(List<A> As, List<B> Bs, Dictionary<(A a, B b), float> Cs)
        {
            Dictionary<(A a, B b), float> Ts = new Dictionary<(A a, B b), float>(); // План Поставок

            List<float> curSupply = new List<float>(As.Select(a => a.supply)); // Текущий Запас Поставщика
            List<float> curNeed = new List<float>(Bs.Select(b => b.need)); // Текущая Потребность Потребителя

            Dictionary<(A a, B b), float> curCs = new Dictionary<(A a, B b), float>(Cs); // Текущая Таблица Стоимости

            // Таблица Поставок
            float[,] t = new float[As.Count, Bs.Count];
            for (int i = 0; i < As.Count; i++)
                for (int j = 0; j < Bs.Count; j++)
                    t[i, j] = -1;

            // Заполнение ячеек Таблицы Поставок
            // Условие выхода из цикла: пока Запасы полностью не распределены (и, соответственно, Потребности не удовлетворены)
            while (curSupply.Sum() > 0)
            {
                // Нахождение Минимальной Стоимости
                float minCost = curCs.ToList().Select(x => x.Value).Min();

                // Поиск Соответствующих Ячеек
                List<(int x, int y)> minCells = new List<(int, int)>();
                curCs.ToList().ForEach(x =>
                {
                    if (x.Value == minCost)
                        minCells.Add((As.FindIndex(a => a == x.Key.a), Bs.FindIndex(b => b == x.Key.b)));
                });

                minCells.ForEach(c =>
                {
                    int x = c.x, y = c.y;

                    // Определение Кол-ва Груза новой поставки
                    float transport = Math.Min(curSupply[x], curNeed[y]);
                    t[x, y] = transport;

                    // Учет новой поставки
                    curSupply[x] -= transport;
                    curNeed[y] -= transport;

                    // Если Запасы Поставщика полностью израсходованы, то
                    // соответствующий Поставщик (строка) исключается из рассмотрения
                    if (curSupply[x] <= 0)
                        curCs = curCs.Where(kv => kv.Key.a != As[x]).ToDictionary(kv => kv.Key, kv => kv.Value);

                    // Если Потребности Потребителя полностью удовлетворены, то
                    // соответствующий Потребитель (столбец) исключается из рассмотрения
                    if (curNeed[y] <= 0)
                        curCs = curCs.Where(kv => kv.Key.b != Bs[y]).ToDictionary(kv => kv.Key, kv => kv.Value);
                });
            }

            // Представление Результата в типе данных «Словарь»
            for (int i = 0; i < As.Count; i++)
                for (int j = 0; j < Bs.Count; j++)
                    Ts.Add((As[i], Bs[j]), t[i, j]);

            return Ts;
        }

        // Метод Потенциалов
        static Dictionary<(A a, B b), float> Potential(List<A> As, List<B> Bs, Dictionary<(A a, B b), float> Cs, Dictionary<(A a, B b), float> startTs)
        {
            Dictionary<(A a, B b), float> Ts = new Dictionary<(A a, B b), float>(); // План Поставок

            // Таблица Поставок
            float[,] t = new float[As.Count, Bs.Count];
            for (int i = 0; i < As.Count; i++)
                for (int j = 0; j < Bs.Count; j++)
                    t[i, j] = -1;
            startTs.ToList().ForEach(x =>
            {
                t[As.FindIndex(a => a == x.Key.a), Bs.FindIndex(b => b == x.Key.b)] = x.Value;
            });

            int ci = 0; // Счетчик Итераций
            float cost = GetTotalCost(startTs, Cs); // Цена Текущего Плана

            Console.WriteLine();
            Console.WriteLine("Начальный план: ");

            // Итерации Метода Потенциалов
            while (1 > 0)
            {
                Console.WriteLine($"f{ci++} = {cost} (ден. ед.)");

                // I. Исследование Базисного Решения на Оптимальность

                // I.1.
                // Вычислим потенциалы Ui и Vj исходя из базисных переменных.
                // Для их нахождения используем условие Ui + Vj = Cij.

                // I.1.1. Составление уравнений
                List<string> equations = new List<string>();
                List<string> vars = new List<string>();
                for (int x = 0; x < As.Count; x++)
                    for (int y = 0; y < Bs.Count; y++)
                        if (t[x, y] > -1) // Заполненная клетка
                        {
                            equations.Add($"u{x + 1}+v{y + 1}={Cs[(As[x], Bs[y])]}");
                            if (!vars.Contains($"u{x + 1}"))
                                vars.Add($"u{x + 1}");
                            if (!vars.Contains($"v{y + 1}"))
                                vars.Add($"v{y + 1}");
                        }
                //Console.WriteLine("{0}", string.Join(", ", equations));

                // I.1.2. Решение системы уравнений
                Dictionary<string, float> answers = new Dictionary<string, float>();
                answers.Add("u1", 0);
                while (answers.Count < vars.Count)
                {
                    for(int j = 0; j < answers.Count; j++)
                    {
                        string lastAnswer = answers.ToList()[j].Key;
                        float lastValue = answers.ToList()[j].Value;

                        for (int i = 0; i < equations.Count; i++)
                        {
                            if (equations[i].Contains(lastAnswer))
                            {
                                string s = equations[i].Replace(lastAnswer, lastValue.ToString());

                                string x1 = s.Substring(0, s.Length - (s.Length - s.IndexOf("+")));
                                string x2 = s.Substring(s.IndexOf("+") + 1, s.IndexOf("=") - s.IndexOf("+") - 1);
                                string x3 = s.Substring(s.IndexOf("=") + 1);

                                float y;
                                if (!float.TryParse(x1, out y))
                                    float.TryParse(x2, out y);
                                float z = float.Parse(x3);
                                float a = z - y;

                                float ttt;
                                if (float.TryParse(x1, out ttt))
                                    answers.Add(x2, a);
                                else
                                    answers.Add(x1, a);

                                equations.RemoveAt(i--);
                            }

                            //answers.ToList().ForEach(y => Console.Write("{0} -> {1} |", y.Key, y.Value));
                            //Console.WriteLine();
                        }
                    }
                }

                // I.1.3. Запись ответов
                float[] u = new float[As.Count], v = new float[Bs.Count];
                for (int i = 0; i < u.Length; i++)
                    u[i] = answers[$"u{i + 1}"];
                for (int j = 0; j < v.Length; j++)
                    v[j] = answers[$"v{j + 1}"];

                // I.2. Для каждой свободной клетки вычислим относительные оценки.
                float[,] delta = new float[As.Count, Bs.Count];
                for (int x = 0; x < As.Count; x++)
                    for (int y = 0; y < Bs.Count; y++)
                        if (t[x, y] <= -1) // Свободная клетка
                        {
                            delta[x, y] = Cs[(As[x], Bs[y])] - (u[x] + v[y]);
                            //Console.WriteLine($"D({x + 1},{y + 1}) = {Cs[(As[x], Bs[y])]} - ({u[x]} + {v[y]}) = {delta[x, y]}");
                        }

                // I.3. Проверка выполнения условия оптимальности плана Dij >= 0
                bool isOptimal = true;
                for (int x = 0; x < As.Count; x++)
                    for (int y = 0; y < Bs.Count; y++)
                        if (t[x, y] <= -1) // Свободная клетка
                            if (delta[x, y] < 0.0)
                            {
                                isOptimal = false;
                                break;
                            }
                Console.WriteLine("Проверка оптимальности: {0}", isOptimal ? "План оптимален" : "План НЕ оптимален");
                Console.WriteLine();
                if (isOptimal)
                    break;

                // II. Определение нового базисного решения

                // II.1. Определение минимальной размерности
                float minDelta = 0; (int x, int y) minDeltaCell = (0, 0);
                for (int x = 0; x < As.Count; x++)
                    for (int y = 0; y < Bs.Count; y++)
                        if (t[x, y] <= -1) // Свободная клетка
                            if (delta[x, y] < minDelta)
                            {
                                minDelta = delta[x, y];
                                minDeltaCell.x = x; minDeltaCell.y = y;
                            }
                //Console.WriteLine("MinDelta = {0} ({1})", minDelta, minDeltaCell);

                // II.2. Построение замкнутого цикла пересчета
                List<List<(int x, int y)>> loops = GetLoops(t, minDeltaCell);
                List<(int x, int y)> path = loops[0];

                // II.3. Определение Lambda
                float lambda = path.Where((x, i) => i % 2 != 0).Select(x => t[x.x, x.y]).Min();
                //Console.WriteLine("Lambda = {0}", lambda);
                t[minDeltaCell.x, minDeltaCell.y] = lambda;

                Console.WriteLine($"Итерация {ci}: ");
                Console.WriteLine("До: ");
                ShowTablePotentialPreIteration(As, Bs, Cs, t, u, v, path);

                // II.4. Перенос Lambda единиц груза по означенному Циклу
                path.Where((x, i) => i % 2 != 0).ToList().ForEach(x => t[x.x, x.y] -= lambda);
                path.Where((x, i) => i % 2 == 0 && i != 0 && i != path.Count - 1).ToList().ForEach(x => t[x.x, x.y] += lambda);

                // II.5. Объявление свободной переменной (только одна из равных нулю)
                foreach(var x in path)
                {
                    if (t[x.x, x.y] > 0)
                        continue;

                    t[x.x, x.y] = -1;
                    break;
                }

                Console.WriteLine("После: ");
                ShowTablePotentialPostIteration(As, Bs, Cs, t, u, v);

                // II.6. Определение стоимости перевозок по новому плану
                cost = cost + minDelta * lambda;
            }

            // Представление Результата в типе данных «Словарь»
            for (int i = 0; i < As.Count; i++)
                for (int j = 0; j < Bs.Count; j++)
                    Ts.Add((As[i], Bs[j]), t[i, j]);

            return Ts;
        }

        // Поиск Циклов в Таблице
        // - Главная функция
        static List<List<(int x, int y)>> GetLoops(float[,] t, (int x, int y) start)
        {
            List<List<(int x, int y)>> loops = new List<List<(int x, int y)>>();

            List<(int x, int y)> path = new List<(int x, int y)>();

            GetLoops_Sub(t, start, path, start, ref loops);

            loops.ForEach(x =>
            {
                //Console.WriteLine("ANSWER: {0}", String.Join(", ", x.Select(y => (y.x + 1, y.y + 1))));
            });

            return loops;
        }
        // - Вспомогательная функция
        static void GetLoops_Sub(float[,] t, (int x, int y) nPoint, List<(int x, int y)> oPath, (int x, int y) endPoint, ref List<List<(int x, int y)>> loops)
        {
            int m = t.GetLength(0);
            int n = t.GetLength(1);

            // Следующий шаг в Пути
            List<(int x, int y)> path = new List<(int x, int y)>(oPath) { nPoint };

            //Console.WriteLine("Debug: {0}", String.Join(", ", path.Select(x => (x.x + 1, x.y + 1))));

            // Путь замкнулся => Цикл образован и данная ветка перебора закончена
            if(path.Count > 1 && nPoint == endPoint)
            {
                loops.Add(path);
                return;
            }

            // Проверка чередования столбец-строка
            bool rowCheck = true;
            if (path.Count >= 2 && path[path.Count - 2].x == nPoint.x)
                rowCheck = false;
            if (rowCheck)
            {
                // Проход по строке
                for (int j = 0; j < n; j++)
                {
                    if ((nPoint.x, j) == nPoint)
                        continue;

                    int c = 0;
                    c = path.Where(x => x.x == nPoint.x).Count();

                    if (t[nPoint.x, j] > 0) // Заполненная клетка
                    {
                        // Пропускать уже пройденные вершины
                        if (path.Contains((nPoint.x, j)))
                            continue;

                        // В строке могут быть только две отмеченные вершины
                        if (c != 1)
                            continue;

                        GetLoops_Sub(t, (nPoint.x, j), path, endPoint, ref loops);
                    }
                    else if ((nPoint.x, j) == endPoint) // Выбранная клетка
                    {
                        // В строке могут быть только две отмеченные вершины
                        if (c != 2)
                            continue;

                        GetLoops_Sub(t, (nPoint.x, j), path, endPoint, ref loops);
                    }
                }
            }

            // Проверка чередования столбец-строка
            bool columnCheck = true;
            if (path.Count >= 2 && path[path.Count - 2].y == nPoint.y)
                columnCheck = false;
            if (columnCheck)
            {
                // Проход по столбцу
                for (int i = 0; i < m; i++)
                {
                    // Пропускать уже пройденные вершины
                    if ((i, nPoint.y) == nPoint)
                        continue;

                    int c = 0;
                    c = path.Where(x => x.y == nPoint.y).Count();

                    if (t[i, nPoint.y] > 0) // Заполненная клетка
                    {
                        // Пропускать уже пройденные вершины
                        if (path.Contains((i, nPoint.y)))
                            continue;

                        // В столбце могут быть только две отмеченные вершины
                        if (c != 1)
                            continue;

                        GetLoops_Sub(t, (i, nPoint.y), path, endPoint, ref loops);
                    }
                    else if ((i, nPoint.y) == endPoint)  // Выбранная клетка
                    {
                        // В столбце могут быть только две отмеченные вершины
                        if (c != 2)
                            continue;

                        GetLoops_Sub(t, (i, nPoint.y), path, endPoint, ref loops);
                    }
                }
            }
        }
    }
}