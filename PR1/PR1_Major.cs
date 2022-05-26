/*
    ТРОФИМОВ М. А. ИКБО-15-20
    Теория Принятия Решений
    Практическая Работа #1. Метод Парето
 */

using System.Collections.Generic;
using System.Linq;
using Library;

namespace PR1
{
    static internal partial class PR1
    {
        // Критерий
        class K
        {
            public string name = "K"; // Имя Критерия
            public bool positive = true; // Стремление Критерия

            public K(string name, bool positive)
            {
                this.name = name;
                this.positive = positive;
            }

            public override string ToString()
            {
                return string.Format("K(\"{0}\", {1})", name, positive ? "+" : "-");
            }
        }

        // Альтернатива
        class A
        {
            public string name = "A"; // Имя Альтернативы
            public List<float> values = new List<float>(); // Значения Альтернативы

            public A(string name, List<float> pars)
            {
                this.name = name;
                this.values = new List<float>(pars);
            }

            public override string ToString()
            {
                return string.Format("A(\"{0}\", {1})", name, string.Join(", ", values));
            }
        }


        // Метод Парето
        // - Главная Функция
        // ( Нахождение Множества Парето-Оптимальных Решений )
        static List<A> Pareto(List<A> As, List<K> Ks, out int[,] infoRelations)
        {
            // Требуемое Множество Парето-Оптимальных Решений
            List<A> NewAs = new List<A>(As);

            // Нахождение Множества Парето-Оптимальных Решений
            infoRelations = new int[As.Count, As.Count];
            for (int i = 0; i < As.Count; i++)
                for (int j = 0; j < As.Count; j++)
                {
                    // Отношение Парето-Доминирования
                    A a = As[i]; // Объект A (с индексом i)
                    A b = As[j]; // Объект B (с индексом j)

                    // Заполнение таблицы Отношений Парето-Доминирования
                    infoRelations[i, j] = Pareto_Compare(a, b, Ks);
                    // Результат =  1: Объект A - Доминирующий, объект B - Доминируемый
                    // Результат = -1: Объект A - Доминируемый, объект B - Доминирующий
                    // Результат =  0: Объекты A и B несравнимые

                    // Если Объект A является Доминируемым, то исключаем его из выбора.
                    if (infoRelations[i, j] == -1)
                        NewAs.Remove(a);
                }

            // Полученное Множество Парето-Оптимальных Решений
            return NewAs;
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
