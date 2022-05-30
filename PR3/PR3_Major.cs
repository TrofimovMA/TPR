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
            public List<int> values = new List<int>(); // Оценки Альтернативы

            public A(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return string.Format("A(\"{0}\")", name);
            }
        }
    }
}
