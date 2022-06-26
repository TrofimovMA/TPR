using System;
using System.Collections.Generic;
using System.Globalization;
using Library;

namespace PR7
{
    static internal partial class PR7
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
            Console.ReadLine();
        }
    }
}
