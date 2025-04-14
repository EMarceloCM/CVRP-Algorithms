using CVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CVRP.Local_Search;

public class LocalSearch2
{
    public List<List<int>> ImproveSolution(ProblemData problemData, List<List<int>> initialSolution, int maxIterations = 1000)
    {
        var currentSolution = CloneSolution(initialSolution);
        bool improvementFound;
        int iteration = 0;
        double tolerance = 1e-6;

        do
        {
            improvementFound = false;
            double bestImprovement = 0;
            List<List<int>> bestCandidate = null;

            // Busca por Swaps
            for (int r1 = 0; r1 < currentSolution.Count; r1++)
            {
                for (int r2 = r1; r2 < currentSolution.Count; r2++)
                {
                    for (int i = 1; i < currentSolution[r1].Count - 1; i++)
                    {
                        for (int j = 1; j < currentSolution[r2].Count - 1; j++)
                        {
                            var candidate = CloneSolution(currentSolution);
                            int temp = candidate[r1][i];
                            candidate[r1][i] = candidate[r2][j];
                            candidate[r2][j] = temp;

                            if (IsFeasible(candidate, problemData))
                            {
                                double currentDistance = CalculateTotalDistance(currentSolution, problemData);
                                double candidateDistance = CalculateTotalDistance(candidate, problemData);
                                double improvement = currentDistance - candidateDistance;

                                if (improvement > bestImprovement + tolerance)
                                {
                                    bestImprovement = improvement;
                                    bestCandidate = candidate;
                                    // Console.WriteLine($"Swap encontrado: Melhoria = {improvement}");
                                }
                            }
                        }
                    }
                }
            }

            // Busca por Inversions
            for (int r = 0; r < currentSolution.Count; r++)
            {
                for (int start = 1; start < currentSolution[r].Count - 1; start++)
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
                            double currentDistance = CalculateTotalDistance(currentSolution, problemData);
                            double candidateDistance = CalculateTotalDistance(candidate, problemData);
                            double improvement = currentDistance - candidateDistance;

                            if (improvement > bestImprovement + tolerance)
                            {
                                bestImprovement = improvement;
                                bestCandidate = candidate;
                                // Console.WriteLine($"Inversion encontrado: Melhoria = {improvement}");
                            }
                        }
                    }
                }
            }

            // Aplicar a melhor melhoria encontrada
            if (bestCandidate != null)
            {
                currentSolution = bestCandidate;
                improvementFound = true;
                double newDistance = CalculateTotalDistance(currentSolution, problemData);
                // Console.WriteLine($"Nova distância total após iteração {iteration + 1}: {newDistance}");
                Console.WriteLine("Solução atualizada:");
                PrintSolution(currentSolution, problemData);
            }

            iteration++;
        } while (improvementFound && iteration < maxIterations);

        Console.WriteLine($"Busca local terminada após {iteration} iterações.");
        return currentSolution;
    }

    private List<List<int>> CloneSolution(List<List<int>> solution)
    {
        return solution.Select(route => new List<int>(route)).ToList();
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

    private bool IsFeasible(List<List<int>> solution, ProblemData problemData)
    {
        foreach (var route in solution)
        {
            double totalDemand = route.Where(c => c != 0).Sum(c => problemData.ClientDemand[c-1]);
            if (totalDemand > problemData.VehiclesCapacity) return false;
        }
        return true;
    }

    public void PrintSolution(List<List<int>> solution, ProblemData problemData)
    {
        Console.WriteLine("\nRotas aprimoradas:");
        for (int i = 0; i < solution.Count; i++)
        {
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", solution[i])}");
        }
        double totalDistance = CalculateTotalDistance(solution, problemData);
        Console.WriteLine($"Distância total percorrida: {totalDistance:F2}");
    }
}
