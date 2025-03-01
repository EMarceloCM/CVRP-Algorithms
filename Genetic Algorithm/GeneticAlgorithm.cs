using CVRP.Utils;

namespace CVRP.GeneticAlgorithm;

public class GeneticAlgorithm(ProblemData instance)
{
    private ProblemData instance = instance;
    private readonly Random _random = new();
    private readonly int _populationSize = 100;
    private readonly int _generations = 1000;
    private readonly double _mutationRate = 0.1;

    public List<List<int>> Solve()
    {
        List<List<List<int>>> population = InitializePopulation();
        for (int generation = 0; generation < _generations; generation++)
        {
            population = Evolve(population);
        }

        var bestSolution = population.OrderBy(CalculateFitness).First();
        PrintSolution(bestSolution);
        return bestSolution;
    }

    private List<List<List<int>>> InitializePopulation()
    {
        var population = new List<List<List<int>>>();
        for (int i = 0; i < _populationSize; i++)
        {
            var solution = GenerateRandomSolution();
            population.Add(solution);
        }
        return population;
    }

    private List<List<int>> GenerateRandomSolution()
    {
        var clients = Enumerable.Range(1, instance.NumberOfClients).OrderBy(_ => _random.Next()).ToList();
        var routes = new List<List<int>>();
        double currentLoad = 0;
        var route = new List<int> { 0 };

        foreach (var client in clients)
        {
            if (currentLoad + instance.ClientDemand[client - 1] <= instance.VehiclesCapacity)
            {
                route.Add(client);
                currentLoad += instance.ClientDemand[client - 1];
            }
            else
            {
                route.Add(0);
                routes.Add(route);
                route = [0, client];
                currentLoad = instance.ClientDemand[client - 1];
            }
        }
        route.Add(0);
        routes.Add(route);
        return routes;
    }

    private List<List<List<int>>> Evolve(List<List<List<int>>> population)
    {
        var newPopulation = new List<List<List<int>>>();
        for (int i = 0; i < population.Count; i++)
        {
            var parent1 = TournamentSelection(population);
            var parent2 = TournamentSelection(population);
            var offspring = Crossover(parent1, parent2);

            if (_random.NextDouble() < _mutationRate)
            {
                Mutate(offspring);
            }
            newPopulation.Add(offspring);
        }
        return newPopulation;
    }

    private List<List<int>> TournamentSelection(List<List<List<int>>> population)
    {
        int tournamentSize = 5;
        var tournament = new List<List<List<int>>>();
        for (int i = 0; i < tournamentSize; i++)
        {
            tournament.Add(population[_random.Next(population.Count)]);
        }
        return tournament.OrderBy(CalculateFitness).First();
    }

    private List<List<int>> Crossover(List<List<int>> parent1, List<List<int>> parent2)
    {
        int splitPoint = _random.Next(1, parent1.Count - 1);
        var offspring = parent1.Take(splitPoint).ToList();
        var remaining = parent2.SelectMany(route => route.Skip(1).Take(route.Count - 2)).Except(offspring.SelectMany(r => r)).ToList();

        var newRoute = new List<int> { 0 };
        double currentLoad = 0;
        foreach (var client in remaining)
        {
            if (currentLoad + instance.ClientDemand[client - 1] <= instance.VehiclesCapacity)
            {
                newRoute.Add(client);
                currentLoad += instance.ClientDemand[client - 1];
            }
            else
            {
                newRoute.Add(0);
                offspring.Add(newRoute);
                newRoute = new List<int> { 0, client };
                currentLoad = instance.ClientDemand[client - 1];
            }
        }
        newRoute.Add(0);
        offspring.Add(newRoute);
        return offspring;
    }

    private void Mutate(List<List<int>> solution)
    {
        int routeIndex = _random.Next(solution.Count);
        var route = solution[routeIndex];
        if (route.Count > 3)
        {
            int i = _random.Next(1, route.Count - 2);
            int j = _random.Next(1, route.Count - 2);
            (route[i], route[j]) = (route[j], route[i]);
        }
    }

    private double CalculateFitness(List<List<int>> solution)
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

    private void CalculateTotalDistance(List<List<int>> solution)
    {
        double totalDistance = CalculateFitness(solution);
        Console.WriteLine($"Distância total percorrida: {totalDistance}");
    }

    private void PrintSolution(List<List<int>> solution)
    {
        Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine("Melhor solução encontrada (Algorítmo Genético):");
        for (int i = 0; i < solution.Count; i++)
        {
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", solution[i])}");
        }

        CalculateTotalDistance(solution);
    }
}