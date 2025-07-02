using CVRP.Utils;

namespace CVRP.Ant_Colony;

public class AntColonyOptimization
{
    private readonly ProblemData _instance;
    private readonly int _numAnts;
    private readonly int _iterations;
    private readonly double _alpha;
    private readonly double _beta;
    private readonly double _evaporationRate;
    private readonly double _pheromoneInit;
    private readonly double _pheromoneMin;
    private readonly double _pheromoneMax;
    private readonly int _candidateSize;
    private double[,] _pheromone;
    private readonly Random _rnd = new();

    public AntColonyOptimization(
        ProblemData instance,
        int numAnts = 20,
        int iterations = 100,
        double alpha = 1.0,
        double beta = 2.0,
        double evaporationRate = 0.1,
        double pheromoneInit = 1.0,
        int candidateSize = 15)
    {
        _instance = instance;
        _numAnts = numAnts;
        _iterations = iterations;
        _alpha = alpha;
        _beta = beta;
        _evaporationRate = evaporationRate;
        _pheromoneInit = pheromoneInit;
        _candidateSize = candidateSize;

        _pheromoneMax = pheromoneInit;
        _pheromoneMin = pheromoneInit * 0.1;

        _pheromone = new double[instance.NumberOfClients + 1, instance.NumberOfClients + 1];
        InitializePheromones();
    }

    private void InitializePheromones()
    {
        for (int i = 0; i <= _instance.NumberOfClients; i++)
            for (int j = 0; j <= _instance.NumberOfClients; j++)
                _pheromone[i, j] = _pheromoneInit;
    }

    public List<List<int>>? FindSolution()
    {
        List<List<int>>? bestSolution = null;
        double bestDistance = double.MaxValue;
        double previousBest = double.MaxValue;

        for (int iter = 0; iter < _iterations; iter++)
        {
            var allSolutions = new List<List<List<int>>>();

            for (int ant = 0; ant < _numAnts; ant++)
            {
                var sol = ConstructSolution();
                allSolutions.Add(sol);
                double dist = CalculateTotalDistance(sol);

                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestSolution = sol;
                }
            }

            UpdatePheromones(allSolutions);

            if (bestDistance < previousBest)
            {
                previousBest = bestDistance;
                Console.WriteLine($"Iteration {iter + 1}/{_iterations}, Best Distance: {bestDistance:F2}");
            }
        }

        PrintSolution(bestSolution);
        return bestSolution;
    }

    private List<List<int>> ConstructSolution()
    {
        var solution = new List<List<int>>();
        var unvisited = new HashSet<int>(Enumerable.Range(1, _instance.NumberOfClients));

        while (unvisited.Count > 0)
        {
            var route = ConstructRoute(unvisited);
            solution.Add(route);
        }
        return solution;
    }

    private List<int> ConstructRoute(HashSet<int> unvisited)
    {
        var route = new List<int> { 0 };
        double load = 0;
        int current = 0;

        while (unvisited.Count > 0)
        {
            int next = SelectNextNode(current, unvisited);
            if (next == -1 || load + _instance.ClientDemand[next - 1] > _instance.VehiclesCapacity)
                break;

            route.Add(next);
            load += _instance.ClientDemand[next - 1];
            unvisited.Remove(next);
            current = next;
        }
        route.Add(0);
        return TwoOptRoute(route);
    }

    private int SelectNextNode(int current, HashSet<int> unvisited)
    {
        // Calcular scores
        var scores = new List<(int node, double score)>();
        foreach (var n in unvisited)
        {
            double tau = Math.Pow(_pheromone[current, n], _alpha);
            double eta = Math.Pow(1.0 / _instance.Distances[current, n], _beta);
            scores.Add((n, tau * eta));
        }

        if (scores.Count == 0) return -1;

        // Lista de candidatos
        var candidates = scores
            .OrderByDescending(x => x.score)
            .Take(_candidateSize)
            .ToList();

        double sum = candidates.Sum(x => x.score);
        if (sum <= 0) return -1;

        double pick = _rnd.NextDouble() * sum;
        double cum = 0;

        foreach (var (node, sc) in candidates)
        {
            cum += sc;
            if (pick <= cum) return node;
        }
        return candidates.Last().node;
    }

    private void UpdatePheromones(List<List<List<int>>> allSolutions)
    {
        // Evaporação com limites
        for (int i = 0; i <= _instance.NumberOfClients; i++)
            for (int j = 0; j <= _instance.NumberOfClients; j++)
                _pheromone[i, j] = Math.Max(_pheromoneMin, _pheromone[i, j] * (1 - _evaporationRate));

        // Reforço apenas top-K formigas
        var bestSols = allSolutions
            .Select(sol => (sol, dist: CalculateTotalDistance(sol)))
            .OrderBy(x => x.dist)
            .Take(Math.Min(10, allSolutions.Count));

        foreach (var (sol, dist) in bestSols)
        {
            double delta = 1.0 / dist;
            foreach (var route in sol)
            {
                var improved = TwoOptRoute(route);
                for (int k = 0; k < improved.Count - 1; k++)
                {
                    int a = improved[k], b = improved[k + 1];
                    _pheromone[a, b] = Math.Min(_pheromoneMax, _pheromone[a, b] + delta);
                }
            }
        }
    }

    private List<int> TwoOptRoute(List<int> route)
    {
        // Aplica 2-opt até a primeira melhora
        for (int i = 1; i < route.Count - 2; i++)
        {
            for (int j = i + 1; j < route.Count - 1; j++)
            {
                var candidate = route.Take(i)
                    .Concat(route.GetRange(i, j - i + 1).AsEnumerable().Reverse())
                    .Concat(route.Skip(j + 1))
                    .ToList();
                if (CalculateRouteDistance(candidate) < CalculateRouteDistance(route))
                    return candidate;
            }
        }
        return route;
    }

    private double CalculateTotalDistance(List<List<int>> sol)
    {
        double total = 0;
        foreach (var route in sol)
            total += CalculateRouteDistance(route);
        return total;
    }

    private double CalculateRouteDistance(List<int> route)
    {
        double d = 0;
        for (int i = 0; i < route.Count - 1; i++)
            d += _instance.Distances[route[i], route[i + 1]];
        return d;
    }

    private void PrintSolution(List<List<int>>? sol)
    {
        if (sol == null) return;
        Console.WriteLine("\n--- Best Ant Colony Solution ---");
        for (int i = 0; i < sol.Count; i++)
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", sol[i])}");
        Console.WriteLine($"Distância total percorrida: {CalculateTotalDistance(sol):F2}");
    }
}
