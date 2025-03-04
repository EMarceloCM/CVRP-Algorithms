using CVRP.Utils;

namespace CVRP.ALNS;

public class ALNS
{
    private Random _random = new Random();

    public List<List<int>> AdaptiveLargeNeighborhoodSearch(
        ProblemData problemData,
        int maxIterations = 5000,
        int maxNoImprovement = 2000,
        double initialTemperature = 90.0,
        double coolingRate = 0.997)
    {
        var currentSolution = InitializeSolution(problemData);
        var bestSolution = CloneSolution(currentSolution);
        double currentTemperature = initialTemperature;
        int noImprovementCounter = 0;

        for (int iteration = 0; iteration < maxIterations && noImprovementCounter < maxNoImprovement; iteration++)
        {
            var partialSolution = RandomRemoval(currentSolution, problemData, 3);
            var newSolution = GreedyInsertion(partialSolution, problemData);

            if (IsValidSolution(newSolution, problemData) && AcceptSolution(currentSolution, newSolution, currentTemperature, problemData))
            {
                currentSolution = CloneSolution(newSolution);

                if (CalculateTotalDistance(currentSolution, problemData) < CalculateTotalDistance(bestSolution, problemData))
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

            currentTemperature *= coolingRate; // Resfriamento
        }

        double totalDistance = CalculateTotalDistance(bestSolution, problemData);
        Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine("Rotas geradas (ALNS):");

        var filteredRoutes = bestSolution.Where(route => route.Count > 1 && route.Any(client => client != 0)).ToList();
        for (int i = 0; i < filteredRoutes.Count; i++)
        {
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", filteredRoutes[i])}");
        }
        Console.WriteLine($"Distância total percorrida: {totalDistance}");

        return bestSolution;
    }

    private List<List<int>> InitializeSolution(ProblemData problemData)
    {
        var solution = new List<List<int>>();
        for (int v = 0; v < problemData.NumberOfVehicles; v++)
        {
            solution.Add(new List<int> { 0, 0 });
        }

        var unvisitedClients = Enumerable.Range(1, problemData.NumberOfClients).ToList();
        unvisitedClients = unvisitedClients.OrderBy(x => _random.Next()).ToList();

        foreach (var client in unvisitedClients)
        {
            bool inserted = false;
            foreach (var route in solution)
            {
                double totalDemand = route.Where(c => c != 0).Sum(c => problemData.ClientDemand[c - 1]);
                if (totalDemand + problemData.ClientDemand[client - 1] <= problemData.VehiclesCapacity)
                {
                    route.Insert(route.Count - 1, client);
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                solution.Add(new List<int> { 0, client, 0 });
            }
        }

        return solution;
    }

    private List<List<int>> RandomRemoval(List<List<int>> solution, ProblemData problemData, int numClientsToRemove)
    {
        var modifiedSolution = CloneSolution(solution);
        for (int i = 0; i < numClientsToRemove; i++)
        {
            int routeIndex = _random.Next(modifiedSolution.Count);
            if (modifiedSolution[routeIndex].Count > 2)
            {
                int clientIndex = _random.Next(1, modifiedSolution[routeIndex].Count - 1);
                modifiedSolution[routeIndex].RemoveAt(clientIndex);
            }
        }
        modifiedSolution.RemoveAll(route => route.Count <= 2);
        return modifiedSolution;
    }

    private List<List<int>> GreedyInsertion(List<List<int>> partialSolution, ProblemData problemData)
    {
        var clientsToInsert = GetUnassignedClients(partialSolution, problemData);
        double epsilon = 0.1;

        foreach (var client in clientsToInsert)
        {
            int bestRoute = -1;
            int bestPosition = -1;
            double bestCost = double.MaxValue;
            var candidatePositions = new List<(int routeIndex, int pos, double cost)>();

            for (int r = 0; r < partialSolution.Count; r++)
            {
                for (int pos = 1; pos < partialSolution[r].Count; pos++)
                {
                    double cost = CalculateInsertionCost(partialSolution[r], client, pos, problemData);
                    candidatePositions.Add((r, pos, cost));
                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestRoute = r;
                        bestPosition = pos;
                    }
                }
            }

            var goodCandidates = candidatePositions.Where(c => c.cost <= bestCost * (1 + epsilon)).ToList();
            if (goodCandidates.Count > 0)
            {
                var chosen = goodCandidates[_random.Next(goodCandidates.Count)];
                partialSolution[chosen.routeIndex].Insert(chosen.pos, client);
            }
        }

        partialSolution.RemoveAll(route => route.Count <= 2);
        return partialSolution;
    }

    private double CalculateInsertionCost(List<int> route, int client, int position, ProblemData problemData)
    {
        int prevClient = route[position - 1];
        int nextClient = route[position];
        double cost = problemData.Distances[prevClient, client] +
                      problemData.Distances[client, nextClient] -
                      problemData.Distances[prevClient, nextClient];
        return cost;
    }

    private List<int> GetUnassignedClients(List<List<int>> solution, ProblemData problemData)
    {
        var assignedClients = solution.SelectMany(r => r).Where(c => c != 0).ToHashSet();
        var allClients = Enumerable.Range(1, problemData.NumberOfClients).ToHashSet();
        return allClients.Except(assignedClients).ToList();
    }

    private bool IsValidSolution(List<List<int>> solution, ProblemData problemData)
    {
        foreach (var route in solution)
        {
            double totalDemand = route.Where(c => c != 0).Sum(c => problemData.ClientDemand[c - 1]);
            if (totalDemand > problemData.VehiclesCapacity)
                return false;
        }
        return true;
    }

    private bool AcceptSolution(List<List<int>> currentSolution, List<List<int>> newSolution, double temperature, ProblemData problemData)
    {
        double currentCost = CalculateTotalDistance(currentSolution, problemData);
        double newCost = CalculateTotalDistance(newSolution, problemData);

        if (newCost < currentCost)
            return true;

        double acceptanceProbability = Math.Exp((currentCost - newCost) / temperature);
        return _random.NextDouble() < acceptanceProbability;
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
