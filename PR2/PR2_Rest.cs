using System;
using System.Collections.Generic;
using System.Globalization;
using Library;

namespace PR2
{
    static internal partial class PR2
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

        // Деление шкалы
        public struct ScaleMark
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

            public string Print()
            {
                return string.Format("SM(\"{0}\", {1})", Name, Code);
            }
        }
    }
}
