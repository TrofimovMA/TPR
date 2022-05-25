using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUtils;

namespace Dev
{
    internal static class Dev
    {
        static void Main(string[] args)
        {
            int[] a = { 1, 2, 3, 4, 5, 6 };
            string[] b = { "aaaaaaaaaaaaaaaaaaa", "yz", "zy" };
            
            //Console.WriteLine(b.GetMaxLength1());
            //Console.WriteLine(b.GetMaxLength2());
            Console.WriteLine(b.Max1());
            Console.WriteLine((int)b.Max());
            Console.ReadKey();
        }

        public static string[] GetMultiLine(this string str, int minNum = -1, string separator = "\n")
        {
            string[] a = str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (minNum != -1 && a.Length < minNum)
            {
                int c = minNum - a.Length;
                for (int i = 0; i < c; i++)
                    a = a.Append("").ToArray();
            }
            return a;
        }
        public static string GetMultiLine(this string str, string separator = "|")
        {
            return str.Replace(separator, "\n");
        }

        public static int GetMaxLength1(this string[] array)
        {
            int max = 0;
            foreach (string s in array)
                if (s.Length > max) max = s.Length;
            return max;
        }

        public static int GetMaxLength2(this string[] array)
        {
            return array.Select(x => x.Length).Max();
        }

        public static int Max1(this string[] array)
        {
            return array.Select(x => x.Length).Max();
        }

        public static int Max(this IEnumerable<string> array)
        {
            int max = 0;
            foreach (string s in array)
                if (s.Length > max) max = s.Length;
            return max;
        }
    }
}
