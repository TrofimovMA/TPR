using System;
using System.Collections.Generic;
using System.Globalization;
using Library;

namespace PR3
{
    static internal partial class PR3
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

        // Дробь
        public struct Fraction
        {
            public int N;
            public int D ;

            public Fraction(int n, int d)
            {
                N = n;
                D = d;
            }

            // (Fraction) a = ((int) n, (int) d)
            public static implicit operator Fraction((int n, int d) pars)
            {
                return new Fraction { N = pars.n, D = pars.d };
            }

            // (Fraction) a = (string s)
            public static implicit operator Fraction(string s)
            {
                s = s.Trim();
                if (s.IndexOf("/") == -1)
                    return new Fraction { N = int.Parse(s), D = 1 }; ;
                string ns = s.Substring(0, s.IndexOf("/"));
                string ds = s.Substring(s.IndexOf("/")+1, s.Length - s.IndexOf("/")-1);
                return new Fraction { N = int.Parse(ns), D = int.Parse(ds) };
            }

            // (int n, int d) a = (Fraction) f
            public static implicit operator (int n, int d)(Fraction f)
            {
                return (f.N, f.D);
            }

            // (float) a = (Fraction) f
            public static implicit operator float(Fraction f)
            {
                return f.N * 1f / f.D;
            }

            // (string) a = (Fraction) f
            public static implicit operator string(Fraction f)
            {
                return f.ToString();
            }
            public Fraction Normalization()//Нормализация дроби
            {
                return new Fraction(N / Lib.GetCommonDivisor(N, D), D / Lib.GetCommonDivisor(N, D));
            }
            public static Fraction operator +(Fraction a, Fraction b)
            {
                return new Fraction(a.N * b.D + b.N * a.D, a.D * b.D).Normalization();
            }
            public static Fraction operator -(Fraction a, Fraction b)
            {
                return new Fraction(a.N * b.D - b.N * a.D, a.D * b.D).Normalization();
            }
            public static Fraction operator *(Fraction a, Fraction b)
            {
                return new Fraction(a.N * b.N, a.D * b.D).Normalization();
            }
            public static Fraction operator /(Fraction a, Fraction b)
            {
                return new Fraction(a.N * b.D, b.N * a.D).Normalization();
            }

            public override string ToString()
            {
                if(D == 1)
                    return string.Format("{0}", N);

                return string.Format("{0}/{1}", N, D);
            }

            public string ToString(bool full)
            {
                if (full)
                    return string.Format("{0}/{1}", N, D);

                return string.Format("{0}", (float)this);
            }
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
