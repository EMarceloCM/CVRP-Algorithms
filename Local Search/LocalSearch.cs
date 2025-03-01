using CVRP.Utils;

namespace CVRP.Local_Search;

public class LocalSearch
{
    private Random _random = new();

    public List<List<int>> ImproveSolution(ProblemData problemData, List<List<int>> initialSolution, string initialSolutionAlgorithm, int maxIterations = 100)
    {
        var currentSolution = CloneSolution(initialSolution);
        bool improvementFound = true;
        int iteration = 0;

        while (improvementFound && iteration < maxIterations)
        {
            improvementFound = false;
            improvementFound = improvementFound || TrySwapClients(currentSolution, problemData);
            improvementFound = improvementFound || TryInvertRouteSegment(currentSolution, problemData);

            iteration++;
        }

        /*Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine($"Rotas geradas (Busca Local + {initialSolutionAlgorithm}):");

        for (int i = 0; i < currentSolution.Count; i++)
        {
            var route = currentSolution[i];

            if (route.Count > 1 && route.All(client => client == 0))
            {
                continue;
            }

            if (route.Count > 1)
            {
                Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", route)}");
            }
        }

        Console.WriteLine($"Distância total percorrida: {CalculateTotalDistance(currentSolution, problemData)}");*/

        return currentSolution;
    }

    private bool TrySwapClients(List<List<int>> solution, ProblemData problemData)
    {
        int route1Index = _random.Next(solution.Count);
        int route2Index = _random.Next(solution.Count);

        if (route1Index == route2Index || solution[route1Index].Count <= 1 || solution[route2Index].Count <= 1)
            return false;

        int client1Index = GetRandomClientIndex(solution[route1Index]);
        int client2Index = GetRandomClientIndex(solution[route2Index]);

        int client1 = solution[route1Index][client1Index];
        int client2 = solution[route2Index][client2Index];

        var previousSolution = CloneSolution(solution);
        solution[route1Index][client1Index] = client2;
        solution[route2Index][client2Index] = client1;

        if (IsValidSolution(solution, problemData, previousSolution))
        {
            return true;
        }
        else
        {
            solution[route1Index][client1Index] = client1;
            solution[route2Index][client2Index] = client2;
            return false;
        }
    }

    private bool TryInvertRouteSegment(List<List<int>> solution, ProblemData problemData)
    {
        int routeIndex = _random.Next(solution.Count);
        if (solution[routeIndex].Count <= 3) return false;

        int start = _random.Next(1, solution[routeIndex].Count - 2);
        int end = _random.Next(start + 1, solution[routeIndex].Count - 1);

        var previousSolution = CloneSolution(solution);
        var segment = solution[routeIndex].GetRange(start, end - start + 1);
        segment.Reverse();
        solution[routeIndex].RemoveRange(start, end - start + 1);
        solution[routeIndex].InsertRange(start, segment);

        if (IsValidSolution(solution, problemData, previousSolution))
        {
            return true;
        }
        else
        {
            segment.Reverse();
            solution[routeIndex].RemoveRange(start, segment.Count);
            solution[routeIndex].InsertRange(start, segment);
            return false;
        }
    }

    private int GetRandomClientIndex(List<int> route)
    {
        return _random.Next(1, route.Count - 1);
    }

    private bool IsValidSolution(List<List<int>> solution, ProblemData problemData, List<List<int>> previousSolution)
    {
        double previousTotalDistance = CalculateTotalDistance(previousSolution, problemData);
        double currentTotalDistance = CalculateTotalDistance(solution, problemData);

        if (currentTotalDistance > previousTotalDistance)
        {
            return false;
        }

        foreach (var route in solution)
        {
            double totalDemand = route.Sum(c => problemData.ClientDemand[c > 0 ? c - 1 : 0]);
            if (totalDemand/2 > problemData.VehiclesCapacity)
            {
                return false;
            }
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
}
