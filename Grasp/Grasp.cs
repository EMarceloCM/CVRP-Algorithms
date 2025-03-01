using CVRP.Local_Search;
using CVRP.Utils;

namespace CVRP.Grasp;

public class Grasp
{
    private Random _random = new();

    public List<List<int>> Solve(ProblemData problemData, int maxIterations = 100, int maxLocalSearchIterations = 20)
    {
        List<List<int>> bestSolution = null;
        double bestCost = double.MaxValue;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            var greedySolution = ConstructGreedySolution(problemData);
            var improvedSolution = PerformLocalSearch(problemData, greedySolution, maxLocalSearchIterations);

            double cost = CalculateTotalCost(improvedSolution, problemData);
            if (cost < bestCost)
            {
                bestSolution = improvedSolution;
                bestCost = cost;
            }
        }

        Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine("Rotas geradas (GRASP):");

        for (int i = 0; i < bestSolution.Count; i++)
        {
            var route = bestSolution[i];
            if (route.Count > 1 && route.All(client => client == 0))
                continue;

            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", route)}");
        }

        Console.WriteLine($"Custo total: {bestCost}");
        return bestSolution;
    }

    private List<List<int>> ConstructGreedySolution(ProblemData problemData)
    {
        var solution = new List<List<int>>();
        var unvisited = Enumerable.Range(1, problemData.ClientDemand.Length).ToList();

        while (unvisited.Any())
        {
            var route = new List<int> { 0 };
            double capacityRemaining = problemData.VehiclesCapacity;

            while (unvisited.Any())
            {
                var candidates = unvisited
                    .Where(client => problemData.ClientDemand[client - 1] <= capacityRemaining)
                    .ToList();

                if (!candidates.Any())
                    break;

                var candidateCosts = candidates.Select(client =>
                    new { Client = client, Cost = problemData.Distances[route.Last(), client] })
                    .OrderBy(x => x.Cost)
                    .ToList();

                int randomIndex = _random.Next(Math.Min(3, candidateCosts.Count));
                var selectedClient = candidateCosts[randomIndex].Client;

                route.Add(selectedClient);
                capacityRemaining -= problemData.ClientDemand[selectedClient - 1];
                unvisited.Remove(selectedClient);
            }

            route.Add(0);
            solution.Add(route);
        }

        return solution;
    }

    private List<List<int>> PerformLocalSearch(ProblemData problemData, List<List<int>> solution, int maxIterations)
    {
        var localSearch = new LocalSearch();
        return localSearch.ImproveSolution(problemData, solution, "GRASP", maxIterations);
    }

    private double CalculateTotalCost(List<List<int>> solution, ProblemData problemData)
    {
        double totalCost = 0;

        foreach (var route in solution)
        {
            for (int i = 0; i < route.Count - 1; i++)
            {
                totalCost += problemData.Distances[route[i], route[i + 1]];
            }
        }

        return totalCost;
    }
}