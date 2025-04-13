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
            int maxIterations = 10000000,
            int maxNoImprovement = 2000000,
            double initialTemperature = 5000.0,
            double coolingRate = 0.7)
        {
            var currentSolution = InitializeSolution(problemData);
            var bestSolution = CloneSolution(currentSolution);
            double currentTemperature = initialTemperature;
            int noImprovementCounter = 0;

            for (int iteration = 0; iteration < maxIterations && noImprovementCounter < maxNoImprovement; iteration++)
            {
                int numClientsToRemove = Math.Max(10, (int)(0.1 * problemData.NumberOfClients));
                var partialSolution = SmartRemoval(currentSolution, problemData, numClientsToRemove);

                var newSolution = GreedyInsertion(partialSolution, problemData);

                if (IsValidSolution(newSolution, problemData)
                    && AcceptSolution(currentSolution, newSolution, currentTemperature, problemData))
                {
                    currentSolution = CloneSolution(newSolution);

                    double currDist = CalculateTotalDistance(currentSolution, problemData);
                    double bestDist = CalculateTotalDistance(bestSolution, problemData);
                    if (currDist < bestDist)
                    {
                        bestSolution = CloneSolution(currentSolution);
                        noImprovementCounter = 0;
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

            // Uma única passagem final que GARANTE inserir todos os clientes
            bestSolution = GreedyInsertion(bestSolution, problemData);

            // Se ainda restarem clientes, crie rotas unitárias para eles
            var unassigned = GetUnassignedClients(bestSolution, problemData);
            foreach (var client in unassigned)
                bestSolution.Add(new List<int> { 0, client, 0 });

            // Impressão
            double totalDistance = CalculateTotalDistance(bestSolution, problemData);
            Console.WriteLine("\n--------------------------------------------------------");
            Console.WriteLine("Rotas geradas (ALNS):");
            var filtered = bestSolution.Where(r => r.Count > 1 && r.Any(c => c != 0)).ToList();
            for (int i = 0; i < filtered.Count; i++)
                Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", filtered[i])}");
            Console.WriteLine($"Distância total percorrida: {totalDistance}");

            return bestSolution;
        }

        private List<List<int>> InitializeSolution(ProblemData problemData)
        {
            var sol = new List<List<int>>();
            for (int v = 0; v < problemData.NumberOfVehicles; v++)
                sol.Add(new List<int> { 0, 0 });

            var unvisited = Enumerable.Range(1, problemData.NumberOfClients)
                                      .OrderBy(_ => _random.Next())
                                      .ToList();

            foreach (var client in unvisited)
            {
                bool placed = false;
                double demand = problemData.ClientDemand[client - 1];
                foreach (var route in sol)
                {
                    double load = route.Where(c => c != 0)
                                       .Sum(c => problemData.ClientDemand[c - 1]);
                    if (load + demand <= problemData.VehiclesCapacity)
                    {
                        route.Insert(route.Count - 1, client);
                        placed = true;
                        break;
                    }
                }
                if (!placed)
                    sol.Add(new List<int> { 0, client, 0 });
            }
            return sol;
        }

        private List<List<int>> RandomRemoval(
            List<List<int>> solution,
            ProblemData problemData,
            int numToRemove)
        {
            var mod = CloneSolution(solution);
            for (int k = 0; k < numToRemove; k++)
            {
                int r = _random.Next(mod.Count);
                if (mod[r].Count > 2)
                {
                    int idx = _random.Next(1, mod[r].Count - 1);
                    mod[r].RemoveAt(idx);
                }
            }
            mod.RemoveAll(route => route.Count <= 2);
            return mod;
        }

        private List<List<int>> GreedyInsertion(
            List<List<int>> partial,
            ProblemData problemData)
        {
            var sol = CloneSolution(partial);
            var toInsert = GetUnassignedClients(sol, problemData);
            double epsilon = 0.1;

            foreach (var client in toInsert)
            {
                double bestCost = double.MaxValue;
                int bestR = -1, bestPos = -1;
                var candidates = new List<(int r, int pos, double cost)>();

                for (int r = 0; r < sol.Count; r++)
                {
                    for (int pos = 1; pos < sol[r].Count; pos++)
                    {
                        double c = CalculateInsertionCost(sol[r], client, pos, problemData);
                        candidates.Add((r, pos, c));
                        if (c < bestCost)
                        {
                            bestCost = c;
                            bestR = r;
                            bestPos = pos;
                        }
                    }
                }

                var good = candidates
                    .Where(x => x.cost <= bestCost * (1 + epsilon))
                    .ToList();

                if (good.Count > 0)
                {
                    var pick = good[_random.Next(good.Count)];
                    sol[pick.r].Insert(pick.pos, client);
                }
                else
                {
                    // fallback: cria rota unitária
                    sol.Add(new List<int> { 0, client, 0 });
                }
            }

            sol.RemoveAll(route => route.Count <= 2);
            return sol;
        }

        private double CalculateInsertionCost(
            List<int> route,
            int client,
            int pos,
            ProblemData problemData)
        {
            int a = route[pos - 1], b = route[pos];
            return problemData.Distances[a, client]
                 + problemData.Distances[client, b]
                 - problemData.Distances[a, b];
        }

        private List<int> GetUnassignedClients(
            List<List<int>> solution,
            ProblemData problemData)
        {
            var assigned = solution.SelectMany(r => r)
                                   .Where(c => c != 0)
                                   .ToHashSet();
            var all = Enumerable.Range(1, problemData.NumberOfClients);
            return all.Except(assigned).ToList();
        }

        private bool IsValidSolution(
            List<List<int>> solution,
            ProblemData problemData)
        {
            foreach (var route in solution)
            {
                double load = route.Where(c => c != 0)
                                   .Sum(c => problemData.ClientDemand[c - 1]);
                if (load > problemData.VehiclesCapacity)
                    return false;
            }
            return true;
        }

        private bool AcceptSolution(
            List<List<int>> curr,
            List<List<int>> next,
            double temp,
            ProblemData problemData)
        {
            double c0 = CalculateTotalDistance(curr, problemData);
            double c1 = CalculateTotalDistance(next, problemData);
            if (c1 < c0) return true;
            double p = Math.Exp((c0 - c1) / temp);
            return _random.NextDouble() < p;
        }

        private double CalculateTotalDistance(
            List<List<int>> solution,
            ProblemData problemData)
        {
            double dist = 0;
            foreach (var route in solution)
                for (int i = 0; i < route.Count - 1; i++)
                    dist += problemData.Distances[route[i], route[i + 1]];
            return dist;
        }

        private List<List<int>> CloneSolution(List<List<int>> sol) =>
            sol.Select(r => new List<int>(r)).ToList();

        private List<List<int>> SmartRemoval(List<List<int>> solution, ProblemData problemData, int numClientsToRemove)
        {
            var modified = CloneSolution(solution);
            var allClients = new List<(int client, int routeIndex, int pos)>();

            for (int r = 0; r < modified.Count; r++)
            {
                for (int pos = 1; pos < modified[r].Count - 1; pos++)
                {
                    allClients.Add((modified[r][pos], r, pos));
                }
            }

            if (allClients.Count == 0) return modified;

            var selectedClients = allClients
                .OrderBy(_ => _random.Next())
                .Take(numClientsToRemove)
                .ToList();

            var clientsToRemove = selectedClients.Select(x => x.client).ToHashSet();

            for (int r = 0; r < modified.Count; r++)
            {
                modified[r] = modified[r].Where((c, i) =>
                    i == 0 || i == modified[r].Count - 1 || !clientsToRemove.Contains(c)
                ).ToList();
            }

            modified.RemoveAll(route => route.Count <= 2);

            return modified;
        }
    }
}
