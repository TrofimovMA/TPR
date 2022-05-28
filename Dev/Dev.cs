using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library;

namespace Dev
{
    internal static class Dev
    {
        static void Main(string[] args)
        {
            K a = new K();
            Console.WriteLine(a);
            Console.WriteLine(a.scale);
            Console.WriteLine(String.Join(", ", a.scale.Select(x => x.ToString())));

            Console.ReadKey();
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
