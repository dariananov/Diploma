using System;
using System.Collections.Generic;

namespace AsyncModels
{
    
    public class Vertex
    {
        public Vertex(int initVal, List<int> inputs=null, List<int> f=null)
        {
            val = initVal;
            InputNumbers = inputs;
            TargetFunction = (f != null) ? f : new List<int>();
        }

        public static int MAXVAL = 2;
        public static int MINVAL = 0;
        int val;
        public List<int> InputNumbers { get; }
        public List<int> TargetFunction { get; }

        public int Val
        {
            get { return val; }
            set {
                if (value > MAXVAL) val = MAXVAL;
                else if (value < MINVAL) val = MINVAL;
                else val = value;
                }
        }
    }
}
