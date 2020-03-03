using System;
using System.Collections.Generic;
using InstabilityFinder;

class MainClass
{
    public static void Main(string[] args)
    {
        Region region = new Region();
        region.lowPoint = new int[]{ 0, 0};
        region.highPoint = new int[]{ 1, 1};

        Mode mode = Mode.ASYNC;

        var tfc = new List<Func<List<int>, int>>();
        tfc.Add((List<int> inputs) => { return ((inputs[0] == 0) ? 1 : 0) + inputs[1]; });
        tfc.Add((List<int> inputs) => { return (inputs[1] == 0) ? 1 : 0; });

        Finder finder = new Finder(2, 1, mode, region, tfc);


        var cycle = finder.Begin();
        if (cycle.Count == 0)
            if (mode == Mode.ASYNC)
                Console.WriteLine("Asynchronous cycle was not found.");
            else
                Console.WriteLine("Synchronous cycle was not found.");
        else {
            if (mode == Mode.ASYNC)
                Console.WriteLine("Asynchronous \nsoft cycle found:");
            else
                Console.WriteLine("Synchronous \ncycle found:");
            for (int i = 0; i< cycle.Count; i++)
            {
                Console.WriteLine("\nState " + i + ":");
                for (int j = 0; j < cycle[i].Length; j++)
                    Console.Write(cycle[i][j] + " ");
                Console.WriteLine();
            }
        }
    }
}