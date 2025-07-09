using CVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CVRP.ALNS
{
    public class ALNS
    {
        private Random _random = new Random();

        public List<List<int>> AdaptiveLargeNeighborhoodSearch(
            ProblemData problemData,
            int maxIterations = 1500000,
            int maxNoImprovement = 200000,
            double initialTemperature = 10000.0,
            double coolingRate = 0.999)
        {
            var currentSolution = GreedyInsertion(new List<List<int>>(), problemData, randomized: true);
            var bestSolution = CloneSolution(currentSolution);
            double bestDistance = CalculateTotalDistance(bestSolution, problemData);
            double currentTemperature = initialTemperature;
            int noImprovementCounter = 0;

            for (int iter = 0; iter < maxIterations && noImprovementCounter < maxNoImprovement; iter++)
            {
                int numToRemove = Math.Max(5, (int)(0.2 * problemData.NumberOfClients));
                var partial = RandomRemoval(currentSolution, numToRemove);
                var candidate = GreedyInsertion(partial, problemData);

                if (IsValidSolution(candidate, problemData) &&
                    AcceptSolution(currentSolution, candidate, currentTemperature, problemData))
                {
                    currentSolution = CloneSolution(candidate);
                    double currentDistance = CalculateTotalDistance(currentSolution, problemData);
                    if (currentDistance < bestDistance)
                    {
                        bestSolution = CloneSolution(currentSolution);
                        bestDistance = currentDistance;
                        noImprovementCounter = 0;

                        // Imprime evolução quando há melhoria
                        Console.WriteLine($"Iteração {iter + 1}/{maxIterations} - Nova melhor distância: {bestDistance:F2}");
                        PrintRoutes(bestSolution);
                    }
                    else
                    {
                        noImprovementCounter++;
                    }
                }
                else
                {
                    noImprovementCounter++;
                }

                currentTemperature *= coolingRate;
            }

            // Impressão do resultado final
            Console.WriteLine("\n--- Solução Final (ALNS) ---");
            PrintRoutes(bestSolution);
            Console.WriteLine($"Distância total percorrida: {bestDistance:F2}");

            return bestSolution;
        }

        private List<List<int>> GreedyInsertion(List<List<int>> partial, ProblemData data, bool randomized = false)
        {
            var solution = CloneSolution(partial);
            var clients = GetUnassignedClients(solution, data);
            if (randomized) clients = clients.OrderBy(_ => _random.Next()).ToList();

            foreach (var client in clients)
            {
                double bestCost = double.MaxValue;
                int bestR = -1, bestPos = -1;

                for (int r = 0; r < solution.Count; r++)
                {
                    double load = solution[r].Where(c => c != 0).Sum(c => data.ClientDemand[c - 1]);
                    if (load + data.ClientDemand[client - 1] > data.VehiclesCapacity) continue;

                    for (int pos = 1; pos < solution[r].Count; pos++)
                    {
                        double cost = CalculateInsertionCost(solution[r], client, pos, data);
                        if (cost < bestCost)
                        {
                            bestCost = cost;
                            bestR = r;
                            bestPos = pos;
                        }
                    }
                }

                if (bestR != -1)
                    solution[bestR].Insert(bestPos, client);
                else
                    solution.Add(new List<int> { 0, client, 0 });
            }

            return solution;
        }

        private List<List<int>> RandomRemoval(List<List<int>> solution, int numToRemove)
        {
            var modified = CloneSolution(solution);
            var clients = modified.SelectMany(r => r).Where(c => c != 0).OrderBy(_ => _random.Next()).Take(numToRemove).ToHashSet();

            for (int i = 0; i < modified.Count; i++)
                modified[i] = modified[i].Where((c, idx) => idx == 0 || idx == modified[i].Count - 1 || !clients.Contains(c)).ToList();

            modified.RemoveAll(r => r.Count <= 2);
            return modified;
        }

        private List<int> GetUnassignedClients(List<List<int>> solution, ProblemData data)
        {
            var assigned = solution.SelectMany(r => r).Where(c => c != 0).ToHashSet();
            return Enumerable.Range(1, data.NumberOfClients).Except(assigned).ToList();
        }

        private bool IsValidSolution(List<List<int>> solution, ProblemData data)
        {
            foreach (var route in solution)
            {
                double load = route.Where(c => c != 0).Sum(c => data.ClientDemand[c - 1]);
                if (load > data.VehiclesCapacity)
                    return false;
            }
            return true;
        }

        private bool AcceptSolution(List<List<int>> curr, List<List<int>> next, double temp, ProblemData data)
        {
            double c0 = CalculateTotalDistance(curr, data);
            double c1 = CalculateTotalDistance(next, data);
            if (c1 < c0) return true;
            double p = Math.Exp((c0 - c1) / temp);
            return _random.NextDouble() < p;
        }

        private double CalculateInsertionCost(List<int> route, int client, int pos, ProblemData data)
        {
            int a = route[pos - 1], b = route[pos];
            return data.Distances[a, client] + data.Distances[client, b] - data.Distances[a, b];
        }

        private double CalculateTotalDistance(List<List<int>> solution, ProblemData data)
        {
            double dist = 0;
            foreach (var route in solution)
                for (int i = 0; i < route.Count - 1; i++)
                    dist += data.Distances[route[i], route[i + 1]];
            return dist;
        }

        private List<List<int>> CloneSolution(List<List<int>> sol) => sol.Select(r => new List<int>(r)).ToList();

        private void PrintRoutes(List<List<int>> sol)
        {
            for (int i = 0; i < sol.Count; i++)
                Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", sol[i])}");
        }
    }
}
