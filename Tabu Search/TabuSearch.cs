using CVRP.Utils;

namespace CVRP.Tabu_Search;

public class TabuSearch(ProblemData instance)
{
    private ProblemData instance = instance;
    private int _tabuTenure = 10;
    private int _maxIterations = 500;

    public List<List<int>> Solve()
    {
        var initialSolution = GenerateInitialSolution();
        var bestSolution = CloneSolution(initialSolution);
        var currentSolution = CloneSolution(initialSolution);
        var tabuList = new Queue<(int, int)>();

        for (int iteration = 0; iteration < _maxIterations; iteration++)
        {
            var neighborhood = GenerateNeighborhood(currentSolution, tabuList);

            if (neighborhood.Count == 0)
                break;

            var bestNeighbor = neighborhood.OrderBy(neighbor => CalculateTotalDistance(neighbor)).FirstOrDefault();

            if (CalculateTotalDistance(bestNeighbor) < CalculateTotalDistance(bestSolution))
            {
                bestSolution = CloneSolution(bestNeighbor);
            }

            currentSolution = CloneSolution(bestNeighbor);

            UpdateTabuList(tabuList, currentSolution);

            // Console.WriteLine($"Iteração {iteration + 1}: Melhor distância = {CalculateTotalDistance(bestSolution)}");
        }

        PrintSolution(bestSolution);
        CalculateTotalDistance(bestSolution);
        return bestSolution;
    }

    private List<List<int>> GenerateInitialSolution()
    {
        var initialSolution = new List<List<int>>();
        var unvisitedClients = new HashSet<int>(Enumerable.Range(1, instance.ClientDemand.Length));
        var currentRoute = new List<int> { 0 };
        double currentLoad = 0;

        while (unvisitedClients.Count != 0)
        {
            var nextClient = unvisitedClients.First();
            var demand = instance.ClientDemand[nextClient - 1];

            if (currentLoad + demand <= instance.VehiclesCapacity)
            {
                currentRoute.Add(nextClient);
                currentLoad += demand;
                unvisitedClients.Remove(nextClient);
            }
            else
            {
                currentRoute.Add(0);
                initialSolution.Add(currentRoute);
                currentRoute = new List<int> { 0 };
                currentLoad = 0;
            }
        }

        currentRoute.Add(0);
        initialSolution.Add(currentRoute);
        return initialSolution;
    }

    private List<List<List<int>>> GenerateNeighborhood(List<List<int>> solution, Queue<(int, int)> tabuList)
    {
        var neighborhood = new List<List<List<int>>>();

        for (int r = 0; r < solution.Count; r++)
        {
            var route = solution[r];

            for (int i = 1; i < route.Count - 1; i++)
            {
                for (int j = i + 1; j < route.Count - 1; j++)
                {
                    if (tabuList.Contains((route[i], route[j])))
                        continue;

                    var neighbor = CloneSolution(solution);
                    (neighbor[r][i], neighbor[r][j]) = (neighbor[r][j], neighbor[r][i]);

                    if (IsValidSolution(neighbor))
                        neighborhood.Add(neighbor);
                }
            }
        }

        return neighborhood;
    }


    private void UpdateTabuList(Queue<(int, int)> tabuList, List<List<int>> solution)
    {
        foreach (var route in solution)
        {
            for (int i = 1; i < route.Count - 1; i++)
            {
                tabuList.Enqueue((route[i], route[i + 1]));
                if (tabuList.Count > _tabuTenure)
                {
                    tabuList.Dequeue();
                }
            }
        }
    }

    private bool IsValidSolution(List<List<int>> solution)
    {
        foreach (var route in solution)
        {
            double load = 0;
            foreach (var client in route.Skip(1).Take(route.Count - 2))
            {
                load += instance.ClientDemand[client - 1];
                if (load > instance.VehiclesCapacity)
                    return false;
            }
        }

        return true;
    }

    private double CalculateTotalDistance(List<List<int>> solution)
    {
        double totalDistance = 0;

        foreach (var route in solution)
        {
            for (int i = 0; i < route.Count - 1; i++)
            {
                totalDistance += instance.Distances[route[i], route[i + 1]];
            }
        }

        return totalDistance;
    }

    private List<List<int>> CloneSolution(List<List<int>> solution)
    {
        return solution.Select(route => new List<int>(route)).ToList();
    }

    private void PrintSolution(List<List<int>> solution)
    {
        Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine("Melhor solução encontrada (Tabu Search):");

        for (int i = 0; i < solution.Count; i++)
        {
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", solution[i])}");
        }

        double totalDistance = 0;
        foreach (var route in solution)
        {
            for (int i = 0; i < route.Count - 1; i++)
            {
                totalDistance += instance.Distances[route[i], route[i + 1]];
            }
        }
        Console.WriteLine($"Distância total percorrida: {totalDistance}");
    }
}