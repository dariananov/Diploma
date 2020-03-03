using System;
using System.Collections.Generic;

namespace AsyncModels
{
    public class Comparer : IEqualityComparer<List<int>>
    {
        public Comparer()
        {
        }

        public bool Equals(List<int> x, List<int> y)
        {

            if (x.Count != y.Count) return false; 
            int len = x.Count;
            for (int i = 0; i < len; i++)
            {
                if (x[i] != y[i]) return false;
            }
            return true;
        }

        public int GetHashCode(List<int> obj)
        {
            return 0;
        }
    }
}
