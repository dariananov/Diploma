using System;
using System.Collections.Generic;

namespace StableDiff
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            int MAXVAL = 2;
            var possibleStates = new List<List<int>>();

            var state = new List<int>(new int[5]);
            possibleStates.Add(new List<int>(state));
            for (int i = 0; i < (int)Math.Pow(MAXVAL + 1, 5); i++)
            {
                int j = 5 - 1;
                state[5 - 1] += 1;
                while (state[j] > MAXVAL && j > 0)
                {
                    state[j] = 0;
                    state[j - 1] += 1;
                    j -= 1;
                }
                possibleStates.Add(new List<int>(state));
            }
            for (int i = 0; i < possibleStates.Count; i++)
            {
                for (int j = 0; j < 5; j++)
                    Console.Write(possibleStates[i][j]);
                Console.WriteLine();
            }
        }
    }
}
