using System;
using System.Collections.Generic;
using System.Linq;
using CVRP.Utils;

namespace CVRP.Tabu_Search
{
    public enum MoveType { Swap, Relocate, TwoOpt }

    public struct MoveKey : IEquatable<MoveKey>
    {
        public MoveType Type;
        public int R1, I1, R2, I2;
        public MoveKey(MoveType type, int r1, int i1, int r2, int i2)
        {
            Type = type; R1 = r1; I1 = i1; R2 = r2; I2 = i2;
        }
        public bool Equals(MoveKey other) => Type == other.Type && R1 == other.R1 && I1 == other.I1 && R2 == other.R2 && I2 == other.I2;
        public override bool Equals(object obj) => obj is MoveKey m && Equals(m);
        public override int GetHashCode() => HashCode.Combine(Type, R1, I1, R2, I2);
    }

    public class TabuSearch
    {
        private ProblemData instance;
        private int tabuTenure = 50;
        private int maxIterations = 1000;
        private Dictionary<MoveKey, int> tabuList = new Dictionary<MoveKey, int>();

        public TabuSearch(ProblemData instance)
        {
            this.instance = instance;
        }

        public List<List<int>> Solve()
        {
            var best = GenerateInitialSolution();
            var current = CloneSolution(best);
            double bestCost = CalculateTotalDistance(best);

            for (int iter = 0; iter < maxIterations; iter++)
            {
                var neighborhood = GenerateNeighborhood(current, bestCost, iter);
                if (neighborhood.Count == 0) break;

                var (next, move) = neighborhood.OrderBy(n => CalculateTotalDistance(n.solution)).First();
                double nextCost = CalculateTotalDistance(next);
                current = CloneSolution(next);

                if (nextCost < bestCost)
                {
                    best = CloneSolution(next);
                    bestCost = nextCost;
                }

                tabuList[move] = iter + tabuTenure;
            }

            PrintSolution(best);
            return best;
        }

        private List<List<int>> GenerateInitialSolution()
        {
            var unvisited = new HashSet<int>(Enumerable.Range(1, instance.ClientDemand.Length));
            var solution = new List<List<int>>();

            while (unvisited.Count > 0)
            {
                var route = new List<int> { 0 };
                double load = 0;
                int last = 0;

                while (true)
                {
                    var next = unvisited.OrderBy(c => instance.Distances[last, c]).FirstOrDefault();
                    if (next == 0) break;
                    double d = instance.ClientDemand[next - 1];
                    if (load + d > instance.VehiclesCapacity) break;
                    route.Add(next);
                    load += d;
                    unvisited.Remove(next);
                    last = next;
                }

                route.Add(0);
                solution.Add(route);
            }
            return solution;
        }

        private List<(List<List<int>> solution, MoveKey move)> GenerateNeighborhood(List<List<int>> current, double bestCost, int iter)
        {
            var moves = new List<(List<List<int>>, MoveKey)>();

            int R = current.Count;
            for (int r1 = 0; r1 < R; r1++)
            {
                var route1 = current[r1];
                int L1 = route1.Count;
                for (int i = 1; i < L1 - 1; i++)
                {
                    // Relocate
                    for (int r2 = 0; r2 < R; r2++)
                    {
                        var route2 = current[r2];
                        int L2 = route2.Count;
                        if (r1 == r2) continue;
                        for (int j = 1; j < L2; j++)
                        {
                            var temp = CloneSolution(current);
                            int client = temp[r1][i];
                            temp[r1].RemoveAt(i);
                            temp[r2].Insert(j, client);
                            if (!IsValidSolution(temp)) continue;
                            var key = new MoveKey(MoveType.Relocate, r1, i, r2, j);
                            if (IsTabu(key, iter, CalculateTotalDistance(temp), bestCost)) continue;
                            moves.Add((temp, key));
                        }
                    }
                    // Swap
                    for (int r2 = r1; r2 < R; r2++)
                    {
                        var route2 = current[r2];
                        int L2 = route2.Count;
                        int startJ = r1 == r2 ? i + 1 : 1;
                        for (int j = startJ; j < L2 - 1; j++)
                        {
                            var temp = CloneSolution(current);
                            (temp[r1][i], temp[r2][j]) = (temp[r2][j], temp[r1][i]);
                            if (!IsValidSolution(temp)) continue;
                            var key = new MoveKey(MoveType.Swap, r1, i, r2, j);
                            if (IsTabu(key, iter, CalculateTotalDistance(temp), bestCost)) continue;
                            moves.Add((temp, key));
                        }
                    }
                    // TwoOpt
                    for (int j = i + 2; j < L1 - 1; j++)
                    {
                        var temp = CloneSolution(current);
                        temp[r1].Reverse(i, j - i + 1);
                        if (!IsValidSolution(temp)) continue;
                        var key = new MoveKey(MoveType.TwoOpt, r1, i, r1, j);
                        if (IsTabu(key, iter, CalculateTotalDistance(temp), bestCost)) continue;
                        moves.Add((temp, key));
                    }
                }
            }
            return moves;
        }

        private bool IsTabu(MoveKey key, int iter, double cost, double bestCost)
        {
            if (tabuList.TryGetValue(key, out int expiration) && iter < expiration)
                return cost >= bestCost;
            return false;
        }

        private bool IsValidSolution(List<List<int>> sol)
        {
            foreach (var route in sol)
            {
                double load = 0;
                for (int i = 1; i < route.Count - 1; i++)
                {
                    load += instance.ClientDemand[route[i] - 1];
                    if (load > instance.VehiclesCapacity) return false;
                }
            }
            return true;
        }

        private double CalculateTotalDistance(List<List<int>> sol)
        {
            double dist = 0;
            foreach (var route in sol)
                for (int i = 0; i < route.Count - 1; i++)
                    dist += instance.Distances[route[i], route[i + 1]];
            return dist;
        }

        private List<List<int>> CloneSolution(List<List<int>> sol)
        {
            return sol.Select(r => new List<int>(r)).ToList();
        }

        private void PrintSolution(List<List<int>> sol)
        {
            Console.WriteLine("\n----- Tabu Search Solution -----");
            for (int i = 0; i < sol.Count; i++) Console.WriteLine($"Route {i + 1}: {string.Join(" -> ", sol[i])}");
            Console.WriteLine($"Total distance: {CalculateTotalDistance(sol)}");
        }
    }
}
