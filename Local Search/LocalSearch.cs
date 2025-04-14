using CVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CVRP.Local_Search;

public class LocalSearch
{
    private Random _random = new Random();
    private const double tolerance = 1e-6;

    /// <summary>
    /// Realiza a busca local para melhorar a solução inicial.
    /// O algoritmo varre de forma sistemática as possibilidades de troca (swap) e inversão (inversion)
    /// e aplica o movimento que produzir a maior melhoria no custo total (distância).
    /// </summary>
    public List<List<int>> ImproveSolution(ProblemData problemData, List<List<int>> initialSolution, int maxIterations = 100)
    {
        var currentSolution = CloneSolution(initialSolution);
        int iteration = 0;
        bool improvementFound;

        do
        {
            improvementFound = false;
            double bestImprovement = 0;
            List<List<int>> bestCandidate = null;

            for (int r1 = 0; r1 < currentSolution.Count; r1++)
            {
                for (int r2 = r1; r2 < currentSolution.Count; r2++)
                {
                    if (currentSolution[r1].Count > 2 && currentSolution[r2].Count > 2)
                    {
                        int startIndexR1 = 1;
                        int endIndexR1 = currentSolution[r1].Count - 1;
                        int startIndexR2 = 1;
                        int endIndexR2 = currentSolution[r2].Count - 1;

                        for (int i = startIndexR1; i < endIndexR1; i++)
                        {
                            for (int j = startIndexR2; j < endIndexR2; j++)
                            {
                                if (r1 == r2 && i == j) continue;

                                var candidate = CloneSolution(currentSolution);
                                int temp = candidate[r1][i];
                                candidate[r1][i] = candidate[r2][j];
                                candidate[r2][j] = temp;

                                if (IsFeasible(candidate, problemData))
                                {
                                    double currentCost = CalculateTotalDistance(currentSolution, problemData);
                                    double candidateCost = CalculateTotalDistance(candidate, problemData);
                                    double improvement = currentCost - candidateCost;
                                    if (improvement > bestImprovement + tolerance)
                                    {
                                        bestImprovement = improvement;
                                        bestCandidate = candidate;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int r = 0; r < currentSolution.Count; r++)
            {
                if (currentSolution[r].Count > 3)
                {
                    for (int start = 1; start < currentSolution[r].Count - 2; start++)
                    {
                        for (int end = start + 1; end < currentSolution[r].Count - 1; end++)
                        {
                            var candidate = CloneSolution(currentSolution);
                            var segment = candidate[r].GetRange(start, end - start + 1);
                            segment.Reverse();
                            candidate[r].RemoveRange(start, end - start + 1);
                            candidate[r].InsertRange(start, segment);

                            if (IsFeasible(candidate, problemData))
                            {
                                double currentCost = CalculateTotalDistance(currentSolution, problemData);
                                double candidateCost = CalculateTotalDistance(candidate, problemData);
                                double improvement = currentCost - candidateCost;
                                if (improvement > bestImprovement + tolerance)
                                {
                                    bestImprovement = improvement;
                                    bestCandidate = candidate;
                                }
                            }
                        }
                    }
                }
            }

            if (bestCandidate != null)
            {
                currentSolution = bestCandidate;
                improvementFound = true;
            }

            iteration++;

        } while (improvementFound && iteration < maxIterations);

        return currentSolution;
    }

    private bool IsFeasible(List<List<int>> solution, ProblemData problemData)
    {
        foreach (var route in solution)
        {
            double totalDemand = route.Where(c => c != 0).Sum(c => problemData.ClientDemand[c - 1]);
            if (totalDemand > problemData.VehiclesCapacity)
                return false;
        }
        return true;
    }

    private double CalculateTotalDistance(List<List<int>> solution, ProblemData problemData)
    {
        double totalDistance = 0.0;
        foreach (var route in solution)
        {
            for (int i = 0; i < route.Count - 1; i++)
            {
                totalDistance += problemData.Distances[route[i], route[i + 1]];
            }
        }
        return totalDistance;
    }

    private List<List<int>> CloneSolution(List<List<int>> solution)
    {
        return solution.Select(route => new List<int>(route)).ToList();
    }

    public void PrintSolution(List<List<int>> solution, ProblemData problemData)
    {
        double totalDistance = CalculateTotalDistance(solution, problemData);
        Console.WriteLine("Rotas aprimoradas:");
        for (int i = 0; i < solution.Count; i++)
        {
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", solution[i])}");
        }
        Console.WriteLine($"Distância total percorrida: {totalDistance:F2}");
    }
}
