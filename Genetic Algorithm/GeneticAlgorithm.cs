using System;
using System.Collections.Generic;
using System.Linq;
using CVRP.Utils;

namespace CVRP.GeneticAlgorithm;

public class GeneticAlgorithm
{
    private readonly ProblemData instance;
    private Random _rnd;
    private readonly int _popSize = 400;
    private readonly int _generations = 3000;
    private readonly double _mutationRate = 0.15;
    private readonly int _eliteCount = 15;
    private readonly int _tournamentSize = 6;

    public GeneticAlgorithm(ProblemData instance, int seed = 2025)
    {
        this.instance = instance;
        // Inicializa o Random com a semente informada ou padrão
        _rnd = new Random(seed);
    }

    public List<List<int>> Solve()
    {
        var population = InitializePopulation();
        _rnd = new Random();

        double prevBestDist = double.MaxValue;

        for (int gen = 0; gen < _generations; gen++)
        {
            var nextPop = new List<List<List<int>>>();

            // Elitismo
            var elites = population.OrderBy(CalculateFitness)
                                   .Take(_eliteCount)
                                   .Select(CloneSolution)
                                   .ToList();
            nextPop.AddRange(elites);

            // Geração de novos
            while (nextPop.Count < _popSize)
            {
                var p1 = TournamentSelect(population);
                var p2 = TournamentSelect(population);
                var child = Crossover(p1, p2);

                if (_rnd.NextDouble() < _mutationRate)
                    Mutate(child);

                // 2-opt local
                for (int i = 0; i < child.Count; i++)
                    child[i] = TwoOpt(child[i]);

                nextPop.Add(child);
            }

            population = nextPop;

            // Imprime a melhor rota desta geração, se melhorou
            var bestGen = population.OrderBy(CalculateFitness).First();
            double bestDist = CalculateFitness(bestGen);
            if (bestDist < prevBestDist)
            {
                Console.WriteLine($"\nGeração {gen + 1} - Nova melhor distância: {bestDist:F2}");
                // PrintRoutes(bestGen);
                prevBestDist = bestDist;
            }
        }

        var best = population.OrderBy(CalculateFitness).First();
        Console.WriteLine("\n=== Solução Final ===");
        PrintRoutes(best);
        return best;
    }

    private List<List<List<int>>> InitializePopulation()
    {
        var pop = new List<List<List<int>>>();
        for (int i = 0; i < _popSize; i++)
        {
            var sol = GenerateRandomSolution();
            for (int r = 0; r < sol.Count; r++)
                sol[r] = TwoOpt(sol[r]);
            pop.Add(sol);
        }
        return pop;
    }

    private List<List<int>> GenerateRandomSolution()
    {
        var clients = Enumerable.Range(1, instance.NumberOfClients)
                                .OrderBy(_ => _rnd.Next())
                                .ToList();
        var routes = new List<List<int>>();
        double load = 0;
        var route = new List<int> { 0 };

        foreach (var c in clients)
        {
            var demand = instance.ClientDemand[c - 1];
            if (load + demand <= instance.VehiclesCapacity)
            {
                route.Add(c);
                load += demand;
            }
            else
            {
                route.Add(0);
                routes.Add(route);
                route = new List<int> { 0, c };
                load = demand;
            }
        }
        route.Add(0);
        routes.Add(route);
        return routes;
    }

    private List<List<int>> Crossover(List<List<int>> p1, List<List<int>> p2)
    {
        var chr1 = p1.SelectMany(r => r.Skip(1).Take(r.Count - 2)).ToList();
        var chr2 = p2.SelectMany(r => r.Skip(1).Take(r.Count - 2)).ToList();
        int size = chr1.Count;
        int a = _rnd.Next(size);
        int b = _rnd.Next(size);
        int start = Math.Min(a, b);
        int end = Math.Max(a, b);

        var childChr = new int[size];
        for (int i = start; i <= end; i++) childChr[i] = chr1[i];
        int idx = 0;
        for (int i = 0; i < size; i++)
        {
            if (i >= start && i <= end) continue;
            while (childChr.Contains(chr2[idx])) idx++;
            childChr[i] = chr2[idx++];
        }

        var offspring = new List<List<int>>();
        double load2 = 0;
        var route = new List<int> { 0 };
        foreach (int client in childChr)
        {
            var demand = instance.ClientDemand[client - 1];
            if (load2 + demand <= instance.VehiclesCapacity)
            {
                route.Add(client);
                load2 += demand;
            }
            else
            {
                route.Add(0);
                offspring.Add(route);
                route = new List<int> { 0, client };
                load2 = demand;
            }
        }
        route.Add(0);
        offspring.Add(route);
        return offspring;
    }

    private void Mutate(List<List<int>> sol)
    {
        var ri = _rnd.Next(sol.Count);
        var route = sol[ri];
        if (route.Count > 4)
        {
            int i = _rnd.Next(1, route.Count - 2);
            int j = _rnd.Next(1, route.Count - 2);
            (route[i], route[j]) = (route[j], route[i]);
        }
    }

    private List<int> TwoOpt(List<int> route)
    {
        bool improved = true;
        var best = new List<int>(route);
        double bestDist = RouteDistance(best);

        while (improved)
        {
            improved = false;
            for (int i = 1; i < best.Count - 2; i++)
            {
                for (int j = i + 1; j < best.Count - 1; j++)
                {
                    var cand = TwoOptSwap(best, i, j);
                    double dist = RouteDistance(cand);
                    if (dist < bestDist)
                    {
                        best = cand;
                        bestDist = dist;
                        improved = true;
                        break;
                    }
                }
                if (improved) break;
            }
        }
        return best;
    }

    private List<int> TwoOptSwap(List<int> route, int i, int j)
    {
        var nr = new List<int>();
        nr.AddRange(route.Take(i));
        nr.AddRange(route.GetRange(i, j - i + 1).AsEnumerable().Reverse());
        nr.AddRange(route.Skip(j + 1));
        return nr;
    }

    private double RouteDistance(List<int> route)
    {
        double d = 0;
        for (int i = 0; i < route.Count - 1; i++)
            d += instance.Distances[route[i], route[i + 1]];
        return d;
    }

    private List<List<int>> TournamentSelect(List<List<List<int>>> pop)
    {
        var tour = new List<List<List<int>>>();
        for (int i = 0; i < _tournamentSize; i++)
            tour.Add(pop[_rnd.Next(pop.Count)]);
        return tour.OrderBy(CalculateFitness)
                   .Take(2)
                   .OrderBy(_ => _rnd.NextDouble())
                   .First();
    }

    private double CalculateFitness(List<List<int>> sol) => sol.Sum(RouteDistance);

    private List<List<int>> CloneSolution(List<List<int>> sol)
        => sol.Select(r => new List<int>(r)).ToList();

    private void PrintRoutes(List<List<int>> routes)
    {
        for (int i = 0; i < routes.Count; i++)
            Console.WriteLine($"Rota {i + 1}: {string.Join(" → ", routes[i])}");
    }
}
