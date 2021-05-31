using System.Collections.Generic;

namespace com.clusterrr.Famicom.CoolGirl
{
    public class ClassicSorter : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int p = 0;
            while (true)
            {
                if ((p >= x.Length) && (p >= y.Length))
                    return 0;
                else if (p >= x.Length)
                    return -1;
                else if (p >= y.Length)
                    return 1;
                else if (x[p] < y[p])
                    return -1;
                else if (x[p] > y[p])
                    return 1;
                p++;
            }
        }
    }
}
