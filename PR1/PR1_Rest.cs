using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using LibUtils;

namespace PR1
{
    static internal partial class PR1
    {
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
