using System;
using System.Collections.Generic;

namespace AsyncModels
{
    public class Program
    {
        static void Main(string[] args)
        {
            Network network = new Network(4);
            Vertex v1 = new Vertex(1);
            Vertex v2 = new Vertex(1, new List<int>() { 0, 1, 2 }, x => x[1] + x[0] - x[2]);
            Vertex v3 = new Vertex(0, new List<int>() { 0, 2, 3 }, x => x[1] + x[0] - x[2]);
            Vertex v4 = new Vertex(1, new List<int>() { 1, 3 }, x => x[0] + x[1]);

            network.AddVertex(v1);
            network.AddVertex(v2);
            network.AddVertex(v3);
            network.AddVertex(v4);

            SimulateSync(network);
            //SimulateAsync(network);

            //FindAsyncBasins(network);
            //FindSyncBasins(network);
            //FindStableStates(network);
        }

        static void FindAsyncBasins(Network network)
        {
            List<List<int>> allStates = new List<List<int>>();
            HashSet<HashSet<List<int>>> Set = new HashSet<HashSet<List<int>>>();
            Set.Clear();

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            allStates.Add(new List<int>() { i, j, k, l });
                        }
                    }
                }
            }

            foreach (var state in allStates)
            {


                for (int i = 0; i < 4; i++)
                {
                    HashSet<List<int>> setA = new HashSet<List<int>>(new Comparer());
                    setA.Clear();

                    foreach (var set in Set)
                    {
                        if (set.Contains(state))
                        {
                            setA = set;
                            break;
                        }
                    }

                    network.ChangeState(state);
                    network.UpdateVertex(i, network.vertices.ConvertAll(new Converter<Vertex, int>(Network.VertToVal)));

                    List<int> newState = network.GetState();
                    HashSet<List<int>> setB = new HashSet<List<int>>(new Comparer());
                    setB.Clear();

                    foreach (var set in Set)
                    {
                        if (set.Contains(newState)) setB = set;
                    }

                    if (setB.Count != 0 && setA.Count != 0)
                    {
                        Set.Remove(setA);
                        Set.Remove(setB);
                        setB.UnionWith(setA);
                        Set.Add(setB);
                    }

                    if (setB.Count != 0 && setA.Count == 0)
                    {
                        Set.Remove(setB);
                        setB.Add(state);
                        Set.Add(setB);
                    }

                    if (setB.Count == 0 && setA.Count != 0)
                    {
                        Set.Remove(setA);
                        setA.Add(newState);
                        Set.Add(setA);
                    }

                    if (setA.Count == 0 && setB.Count == 0)
                    {
                        setB.Add(newState);
                        setA.Add(state);
                        setB.UnionWith(setA);
                        Set.Add(setB);
                    }
                }

            }

            Console.WriteLine("...........................");
            Console.WriteLine("For Async Model Basins found:");
            foreach (var set in Set)
            {
                Console.WriteLine("Next basin:");
                foreach (var state in set)
                {
                    network.ChangeState(state);
                    Console.WriteLine(network.StateToString());
                }
            }
        }

        static void FindSyncBasins(Network network)
        {
            List<List<int>> allStates = new List<List<int>>();
            HashSet<HashSet<List<int>>> Set = new HashSet<HashSet<List<int>>>();
            Set.Clear();

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            allStates.Add(new List<int>() { i, j, k, l });
                        }
                    }
                }
            }

            foreach (var state in allStates)
            {

                HashSet<List<int>> setA = new HashSet<List<int>>(new Comparer());
                setA.Clear();
                HashSet<List<int>> setB = new HashSet<List<int>>(new Comparer());
                setB.Clear();

                foreach (var set in Set)
                {
                    if (set.Contains(state)) setA = set;
                }

                network.ChangeState(state);
                network.UpdateSyncNetwork();

                List<int> newState = network.GetState();
                foreach (var set in Set)
                {
                    if (set.Contains(newState)) setB = set;
                }

                if (setB.Count != 0 && setA.Count != 0)
                {
                    Set.Remove(setA);
                    Set.Remove(setB);
                    setA.UnionWith(setB);
                    Set.Add(setA);
                }

                if (setB.Count != 0 && setA.Count == 0)
                {
                    Set.Remove(setB);
                    setB.Add(state);
                    Set.Add(setB);
                }

                if (setB.Count == 0 && setA.Count != 0)
                {
                    Set.Remove(setA);
                    setA.Add(newState);
                    Set.Add(setA);
                }

                if (setA.Count == 0 && setB.Count == 0)
                {
                    setB.Add(newState);
                    setA.Add(state);
                    setA.UnionWith(setB);
                    Set.Add(setA);
                }
            }

            Console.WriteLine("...........................");
            Console.WriteLine("For Sync Model Basins found:");
            foreach (var set in Set)
            {
                Console.WriteLine("Next basin:");
                foreach (var state in set)
                {
                    network.ChangeState(state);
                    Console.WriteLine(network.StateToString());
                }
            }
        }

        static void PrintBigSet(HashSet<HashSet<List<int>>> set)
        {
            Console.WriteLine("Printing big set:");
            foreach (var s in set)
            {
                Console.WriteLine("Next subset:");
                foreach (var state in s)
                {
                    foreach (var num in state)
                    {
                        Console.Write(num);
                    }
                    Console.WriteLine();
                }
            }
        }

        static void FindStableStates(Network network)
        {
            List<string> asyncStableStates = new List<string>();
            List<string> syncStableStates = new List<string>();

            //Console.WriteLine(".......................");
            //Console.WriteLine("Trying to find sync stable states:");
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < 2; l++)
                        { 
                            network.ChangeState(new List<int>(){ i, j, k, l});
                            if (network.CheckStateSyncStability())
                            {
                                //Console.WriteLine("The state " + i + j + k + l + " is stable");
                                syncStableStates.Add(i.ToString() + j.ToString() + k.ToString() + l.ToString());
                            }
                            //else Console.WriteLine("Couldn't prove the stability of state " + i + j + k + l);
                        }
                    }
                }
            }
            Console.WriteLine(".......................");
            if (syncStableStates.Count == 0) Console.WriteLine("No sync stable states found");
            else Console.WriteLine("Sync stable states found:");
            foreach (var state in syncStableStates) Console.WriteLine(state);

            //Console.WriteLine();
            //Console.WriteLine(".......................");
            //Console.WriteLine("Trying to find async stable states:");
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            network.ChangeState(new List<int>() { i, j, k, l });
                            if (network.CheckStateAsyncStability(false))
                            {
                                //Console.WriteLine("The state " + i + j + k + l + " is stable");
                                asyncStableStates.Add(i.ToString() + j.ToString() + k.ToString() + l.ToString());
                            }
                            //else Console.WriteLine("Couldn't prove the stability of state " + i + j + k + l);
                        }
                    }
                }
            }
            Console.WriteLine(".......................");
            if (asyncStableStates.Count == 0) Console.WriteLine("No async stable states found");
            else Console.WriteLine("Async stable states found:");
            foreach (var state in asyncStableStates) Console.WriteLine(state);
        }

        static void SimulateAsync(Network network)
        {
            Console.WriteLine(network.StateToString());
            for (int i = 0; i < 30; i++)
            {
                network.UpdateRandomVertex(true);
            }
        }

        static void SimulateSync(Network network)
        {
            Console.WriteLine(network.StateToString());
            for (int i = 0; i < 30; i++)
            {
                network.UpdateSyncNetwork();
            }
        }
    }
}
