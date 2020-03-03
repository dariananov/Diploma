using System;
using System.Collections.Generic;

namespace InstabilityFinder
{
    public enum Mode
    {
        SYNC, ASYNC
    };

    public struct Region
    {
        public int[] lowPoint;
        public int[] highPoint;
    }

    public struct Frontier
    {
        public List<int[]> fromReg1ToReg2;
        public List<int[]> fromReg2ToReg1;
    }

    public struct CutFrontier
    {
        public Region p1;
        public Region p2;
        public Frontier frontier;
    }

    public class Finder
    {
        int dim;
        Mode mode;
        Region region;
        int maxVal;
        List<Func<List<int>, int>> targetFunctions;
        List<Func<List<int>, int, int>> updateFunctions;

        public Finder(int d, int mVal, Mode m, Region p, List<Func<List<int>, int>> tfs)
        {
            dim = d;
            mode = m;
            maxVal = mVal;
            region = new Region();
            region.highPoint = new int[dim];
            region.lowPoint = new int[dim];
            for (int i = 0; i < dim; i++)
            {
                region.lowPoint[i] = p.lowPoint[i];
                region.highPoint[i] = p.highPoint[i];
            }
            targetFunctions = new List<Func<List<int>, int>>(tfs);
            updateFunctions = new List<Func<List<int>, int, int>>();
            foreach(var tf in targetFunctions)
            {
                updateFunctions.Add(new Func<List<int>, int, int>((List<int> vars, int val) =>
                {
                    if (tf(vars) > val) return val += 1;
                    if (tf(vars) < val) return val -= 1;
                    return val;
                }));
            }
        }

        public List<int[]> Begin()
        {
            return FindInstability(region);
        }

        List<int[]> FindInstability(Region inputReg)
        {
            inputReg = Shrink(inputReg);
            if (ContainsSingleState(inputReg))
                return null;
            else
            {

                CutFrontier afterCut = Cut(inputReg, mode == Mode.ASYNC);
                List<int[]> res1 = FindInstability(afterCut.p1);
                if (res1 != null) return res1;
                List<int[]> res2 = FindInstability(afterCut.p2);
                if (res2 != null) return res2;
                return FindCycleAccrossCut(afterCut, mode == Mode.ASYNC);
            }
        }

        bool ContainsSingleState(Region p)
        {
            for (int i = 0; i < dim; i++)
            {
                if (p.lowPoint[i] != p.highPoint[i])
                    return false;
            }
            return true;
        }

        List<int[]> FindAsyncCycle(int[] state, ref List<int[]> cycle)
        {
            for (int i = 0; i < dim; i++)
            {
                var newState = (int[])state.Clone();
                newState[i] = updateFunctions[i](new List<int>(state), state[i]);
                bool back = IsStateInList(newState, cycle);
                if (!back)
                {
                    cycle.Add(newState);
                    foreach (var st in FindAsyncCycle(newState, ref cycle))
                    {
                        if (!IsStateInList(st, cycle))
                        {
                            cycle.Add((int[])st.Clone());
                        }
                    }
                }
            }

            return cycle;
        }

        List<int[]> FindSoftAsyncCycle(int[] state, ref List<int[]> cycle)
        {
            for (int i = dim - 1; i >= 0; i--)
            {
                var newState = (int[])state.Clone();
                newState[i] = updateFunctions[i](new List<int>(state), state[i]);
                bool back = IsStateInList(newState, cycle);
                if (!back)
                {
                    cycle.Add(newState);
                    FindSoftAsyncCycle(newState, ref cycle);
                }
                else
                    break;
            }
            return cycle;
        }

        bool IsStateInList(int[] state, List<int[]> cycle)
        {
            bool res = false;
            foreach (var st in cycle)
            {
                bool tmp = true;
                for (int i = 0; i < dim; i++)
                {
                    if (st[i] != state[i])
                    {
                        tmp = false;
                        break;
                    }
                }
                res |= tmp;
                if (res) break;
            }
            return res;
        }

        List<int[]> CopyCycle(List<int[]> cycle)
        {
            var copy = new List<int[]>();
            foreach(var p in cycle)
            {
                copy.Add((int[])p.Clone());
            }
            return copy;
        }

        List<int[]> FindCycleAccrossCut(CutFrontier inputCutFr, bool asyn)
        {
            if (inputCutFr.frontier.fromReg1ToReg2.Count < 1 || inputCutFr.frontier.fromReg2ToReg1.Count < 1) return null;
            List<int[]> cycle = new List<int[]>();

            if (asyn)
            {
                foreach (var point in inputCutFr.frontier.fromReg2ToReg1)
                {
                    cycle.Add((int[])point.Clone());
                    var res = FindSoftAsyncCycle(point, ref cycle);
                    if (res.Count > 0) return res;
                }
            }
            
            foreach (var point in inputCutFr.frontier.fromReg1ToReg2)
            {
                var firstState = (int[])point.Clone();
                var oldState = (int[])point.Clone();
                var state = (int[])point.Clone();
                cycle = new List<int[]>();
                while (InRegion(state))
                {
                    cycle.Add((int[])state.Clone());
                    for (int i = 0; i < dim; i++)
                    {
                        state[i] = updateFunctions[i](new List<int>(oldState), oldState[i]);
                    }
                    oldState = (int[])state.Clone();
                    bool back = true;
                    for(int i = 0; i < dim; i++)
                    {
                        if (state[i] != firstState[i])
                        {
                            back = false;
                            break;
                        }
                    }
                    if (back) return cycle;
                }
                if (cycle.Count > 0)
                    break;
            }
            return cycle;
        }

        bool InRegion(int[] state)
        {
            for (int i = 0; i < dim; i++)
            {
                if (state[i] > region.highPoint[i] || state[i] < region.lowPoint[i])
                    return false;
            }
            return true;
        }


        CutFrontier Cut(Region p, bool asyn)
        {
            CutFrontier res = new CutFrontier();
            res.p1 = new Region();
            res.p1.lowPoint = (int[])p.lowPoint.Clone();
            res.p2 = new Region();
            res.p2.highPoint = (int[])p.highPoint.Clone();

            int index = 0;
            for (int i = 0; i < dim; i++)
            {
                if (p.highPoint[i] - p.lowPoint[i] > p.highPoint[index] - p.lowPoint[index])
                {
                    index = i;
                }
            }
            int alpha = (p.highPoint[index] - p.lowPoint[index]) / 2;
            res.p1.highPoint = (int[])p.highPoint.Clone();
            res.p1.highPoint[index] = alpha;
            res.p2.lowPoint = (int[])p.lowPoint.Clone();
            res.p2.lowPoint[index] = alpha + 1;

            res.frontier = new Frontier();
            res.frontier.fromReg1ToReg2 = new List<int[]>();
            res.frontier.fromReg2ToReg1 = new List<int[]>();

            int amount = 1;
            for (int i = 0; i < dim; i++)
            {
                if (i == index) continue;
                amount *= p.highPoint[i] - p.lowPoint[i] + 1;
            }

            var point = new int[dim];

            for (int j = 0; j < amount; j++)
            {
                int count = j;
                for (int i = dim - 1; i >= 0; i--)
                {
                    if (i == index) point[i] = alpha;
                    point[i] = count % (res.p1.highPoint[i] - res.p1.lowPoint[i] + 1) + res.p1.lowPoint[i];
                    count = count / (res.p1.highPoint[i] - res.p1.lowPoint[i] + 1);
                }

                if (updateFunctions[index](new List<int>(point), point[index]) > alpha)
                        res.frontier.fromReg1ToReg2.Add((int[])point.Clone());

                point[index] = alpha + 1;
                if (updateFunctions[index](new List<int>(point), point[index]) < alpha + 1)
                    res.frontier.fromReg2ToReg1.Add((int[])point.Clone());

            }
            return res;
        }

        Region Shrink(Region p)
        {
            Region res = new Region();
            res.lowPoint = new int[dim];
            res.highPoint = new int[dim];
            for (int i = 0; i < dim; i++)
            {
                Tuple<int, int> minMax = MinMaxPossibleVals(p, i);
                res.lowPoint[i] = minMax.Item1;
                res.highPoint[i] = minMax.Item2;
            }
            return res;
        }

        Tuple<int, int> MinMaxPossibleVals(Region p, int number)
        {
            int min = Int32.MaxValue, max = Int32.MinValue;

            int amount = 1;
            for (int i = 0; i < dim; i++)
            {
                amount *= p.highPoint[i] - p.lowPoint[i] + 1;
            }

            var point = new int[dim];
            for (int j = 0; j < amount; j++)
            {
                int count = j;
                for (int i = dim - 1; i >= 0; i--)
                {
                    point[i] = count % (p.highPoint[i] - p.lowPoint[i] + 1) + p.lowPoint[i];
                    count = count / (p.highPoint[i] - p.lowPoint[i] + 1);
                }

                int val = targetFunctions[number](new List<int>(point));
                if (val < 0) val = 0;
                if (val > maxVal) val = maxVal;
                if (val < min) min = val;
                if (val > max) max = val;
            }
            return new Tuple<int, int>(min, max);
         }
    }
}
