using System;
using System.Linq;
using System.Collections.Generic;

namespace AsyncModels
{
    public class Network
    {
        public List<Vertex> vertices;
        Random rnd;
        public List<List<int>> possibleStates;

        public Network(int seed) 
        {
            vertices = new List<Vertex>();
            rnd = new Random(seed);
        }

        public void InitializeAllStates()
        {
            possibleStates = new List<List<int>>();
            var state = new List<int>(new int[vertices.Count
                ]);
            possibleStates.Add(new List<int>(state));
            for (int i = 0; i < (int)Math.Pow(Vertex.MAXVAL + 1, vertices.Count); i++)
            {
                int j = vertices.Count - 1;
                state[vertices.Count - 1] += 1;
                while (state[j] > Vertex.MAXVAL && j > 0)
                {
                    state[j] = 0;
                    state[j - 1] += 1;
                    j -= 1;
                }
                possibleStates.Add(new List<int>(state));
            }
        }

        public void AddVertex(Vertex v)
        {
            vertices.Add(v);
        }

        public int UpdateRandomVertex(bool writeState)
        {
            int number = rnd.Next(vertices.Count);
            UpdateVertex(number, vertices.ConvertAll(new Converter<Vertex, int>(VertToVal)));
            if (writeState)
            {
                Console.WriteLine("Vert ind: " + number.ToString() + " State: " + StateToString());
            }
            return number;
        }

        public void UpdateVertex(int number, List<int> allVals)
        {
            if (vertices[number].InputNumbers == null) return;

            List<int> inputNums = vertices[number].InputNumbers;
            List<int> inputVals = new List<int>();
            foreach (var inNum in inputNums)
            {
                inputVals.Add(allVals[inNum]);
            }
            if (vertices[number].Val > vertices[number].TargetFunction[]) 
                vertices[number].Val +=1;
            else if (vertices[number].Val < vertices[number].TargetFunction[])
                vertices[number].Val += -1;
        }

        public void UpdateSyncNetwork()
        {
            List<int> oldValues = vertices.ConvertAll(new Converter<Vertex, int>(VertToVal));
            for (int i = 0; i < vertices.Count; i++)
            {
                UpdateVertex(i, oldValues);
            }
            Console.WriteLine(StateToString());
        }

        public static int VertToVal(Vertex v)
        {
            return v.Val;
        }

        public string StateToString()
        {
            string s = "";
            foreach(var v in vertices)
            {
                s += v.Val.ToString() + " ";
            }
            return s;
        }

        public bool CheckStateAsyncStability(bool writeStates)
        {
            List<int> verticiesChanged = new List<int>(new int[vertices.Count]);
            List<int> oldValues = vertices.ConvertAll(new Converter<Vertex, int>(VertToVal));
            bool tryMore = true;

            while (tryMore)
            {
                tryMore = false;
                foreach (var num in verticiesChanged)
                {
                    if (num == 0) tryMore = true;
                }
                int numVertex = UpdateRandomVertex(writeStates);
                verticiesChanged[numVertex]++;
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (vertices[i].Val != oldValues[i]) return false;
                }
            }
            return true;
        }

        public bool CheckStateSyncStability()
        {
            List<int> oldValues = vertices.ConvertAll(new Converter<Vertex, int>(VertToVal));
            UpdateSyncNetwork();
            for(int i = 0; i < vertices.Count; i++)
                {
                if (vertices[i].Val != oldValues[i]) return false;
            }
            return true;
        }

        public void ChangeState(List<int> newVals)
        {
            int i = 0;
            foreach (var v in vertices)
            {
                v.Val = newVals[i];
                i++;
            }
        }

        public List<int> GetState()
        {
            List<int> state = new List<int>();
            foreach(var v in vertices)
            {
                state.Add(v.Val);
            }
            return state;
        }
    }
}
