using System;
using System.Collections.Generic;
using System.Globalization;
using Library;

namespace PR2
{
    static internal partial class PR2
    {
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

        // Точка входа в программу
        static void Main()
        {
            // Поддержка разных Региональных Настроек
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentCulture = culture;

            // Главная Программа
            MainProgram();

            // Не закрывать окно консоли автоматически по завершении Программы
            Console.ReadKey();
        }

        // Определение изменений во Множестве Решений
        static List<(A a, char c)> GetChanges(List<A> oldAs, List<A> newAs)
        {
            List<(A a, char c)> changes = new List<(A a, char c)>();

            // Составление списка изменений во Множестве Решений
            foreach (A a in oldAs)
                if (!newAs.Contains(a))
                    changes.Add((a, '-'));
            foreach (A a in newAs)
                if (!oldAs.Contains(a))
                    changes.Add((a, '+'));

            return changes;
        }
    }
}
