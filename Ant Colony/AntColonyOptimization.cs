using CVRP.Utils;

namespace CVRP.Ant_Colony;

public class AntColonyOptimization
{
    private ProblemData instance;
    private int _numAnts;
    private int _iterations;
    private double _alpha; // Influência do feromônio
    private double _beta;  // Influência da heurística (1/distância)
    private double _evaporationRate;
    private double _pheromoneInit;
    private double[,] _pheromone;
    private Random _random = new();

    public AntColonyOptimization(ProblemData instance, int numAnts = 20, int iterations = 100, double alpha = 1.0, double beta = 2.0, double evaporationRate = 0.1, double pheromoneInit = 1.0)
    {
        this.instance = instance;
        _numAnts = numAnts;
        _iterations = iterations;
        _alpha = alpha;
        _beta = beta;
        _evaporationRate = evaporationRate;
        _pheromoneInit = pheromoneInit;
        _pheromone = new double[instance.NumberOfClients + 1, instance.NumberOfClients + 1];

        InitializePheromones();
    }

    private void InitializePheromones()
    {
        for (int i = 0; i <= instance.NumberOfClients; i++)
        {
            for (int j = 0; j <= instance.NumberOfClients; j++)
            {
                _pheromone[i, j] = _pheromoneInit;
            }
        }
    }

    public List<List<int>>? FindSolution()
    {
        List<List<int>>? bestSolution = null;
        double bestDistance = double.MaxValue;

        for (int iteration = 0; iteration < _iterations; iteration++)
        {
            var allSolutions = new List<List<List<int>>>();

            for (int ant = 0; ant < _numAnts; ant++)
            {
                var solution = ConstructSolution();
                allSolutions.Add(solution);

                double distance = CalculateTotalDistance(solution);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSolution = solution;
                }
            }

            UpdatePheromones(allSolutions);

            // Console.WriteLine($"Iteration {iteration + 1}/{_iterations}, Best Distance: {bestDistance}");
        }

        PrintSolution(bestSolution);
        return bestSolution;
    }

    private List<List<int>> ConstructSolution()
    {
        var solution = new List<List<int>>();
        var unvisited = new HashSet<int>(Enumerable.Range(1, instance.NumberOfClients));

        while (unvisited.Count != 0)
        {
            var route = ConstructRoute(unvisited);
            solution.Add(route);
        }

        return solution;
    }

    private List<int> ConstructRoute(HashSet<int> unvisited)
    {
        var route = new List<int> { 0 };
        double currentLoad = 0;
        int currentNode = 0;

        while (unvisited.Count != 0)
        {
            var nextNode = SelectNextNode(currentNode, unvisited);
            if (nextNode == -1 || currentLoad + instance.ClientDemand[nextNode - 1] > instance.VehiclesCapacity)
            {
                break;
            }

            route.Add(nextNode);
            currentLoad += instance.ClientDemand[nextNode - 1];
            unvisited.Remove(nextNode);
            currentNode = nextNode;
        }

        route.Add(0);
        return route;
    }

    private int SelectNextNode(int currentNode, HashSet<int> unvisited)
    {
        var probabilities = new List<(int node, double probability)>();
        double denominator = 0;

        foreach (var nextNode in unvisited)
        {
            double pheromone = Math.Pow(_pheromone[currentNode, nextNode], _alpha);
            double heuristic = Math.Pow(1.0 / instance.Distances[currentNode, nextNode], _beta);
            double probability = pheromone * heuristic;
            probabilities.Add((nextNode, probability));
            denominator += probability;
        }

        if (denominator == 0)
        {
            return -1;
        }

        double randomValue = _random.NextDouble();
        double cumulative = 0;

        foreach (var (node, probability) in probabilities)
        {
            cumulative += probability / denominator;
            if (randomValue <= cumulative)
            {
                return node;
            }
        }

        return probabilities.Last().node;
    }

    private void UpdatePheromones(List<List<List<int>>> allSolutions)
    {
        for (int i = 0; i <= instance.NumberOfClients; i++)
        {
            for (int j = 0; j <= instance.NumberOfClients; j++)
            {
                _pheromone[i, j] *= (1 - _evaporationRate);
            }
        }

        foreach (var solution in allSolutions)
        {
            double distance = CalculateTotalDistance(solution);
            double contribution = 1.0 / distance;

            foreach (var route in solution)
            {
                for (int k = 0; k < route.Count - 1; k++)
                {
                    int from = route[k];
                    int to = route[k + 1];
                    _pheromone[from, to] += contribution;
                }
            }
        }
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

    private void PrintSolution(List<List<int>>? solution)
    {
        if (solution == null) return;

        Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine("Melhor solução encontrada (Colônia de Formigas):");

        for (int i = 0; i < solution.Count; i++)
        {
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", solution[i])}");
        }

        double totalDistance = CalculateTotalDistance(solution);
        Console.WriteLine($"Distância total percorrida: {totalDistance}");
    }
}