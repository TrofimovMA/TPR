using System;
using System.Collections.Generic;
using System.Globalization;
using Library;

namespace PR1
{
    static internal partial class PR1
    {
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
