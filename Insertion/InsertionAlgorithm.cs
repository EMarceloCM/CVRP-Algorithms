using CVRP.Utils;

namespace CVRP.Insertion;

public class InsertionAlgorithm(ProblemData instace)
{
    private readonly ProblemData instance = instace;
    private readonly List<List<int>> routes = [];

    public List<List<int>> Solve()
    {
        var clientsNotRouted = new HashSet<int>(Enumerable.Range(1, instance.NumberOfClients));

        while (clientsNotRouted.Count != 0)
        {
            var route = new List<int> { 0 };
            int initialClient = SelectInitialClient(clientsNotRouted);
            route.Add(initialClient);
            route.Add(0);

            clientsNotRouted.Remove(initialClient);

            while (clientsNotRouted.Count != 0)
            {
                (int bestClient, int bestPosition) = FindBestInsertion(route, clientsNotRouted);

                if (bestClient == -1) break;

                route.Insert(bestPosition, bestClient);
                clientsNotRouted.Remove(bestClient);
            }

            routes.Add(route);
        }

        while (clientsNotRouted.Count != 0)
        {
            foreach (var route in routes)
            {
                var remainingClients = clientsNotRouted.ToList();
                foreach (var client in remainingClients)
                {
                    if (CanInsert(route, client))
                    {
                        route.Insert(route.Count - 1, client);
                        clientsNotRouted.Remove(client);
                    }
                }
            }
        }

        Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine("Rotas geradas (heurística de inserção):");
        for (int i = 0; i < routes.Count; i++)
        {
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", routes[i])}");
        }
        CalculateTotalDistance();

        return routes;
    }

    private int SelectInitialClient(HashSet<int> clientsNotRouted)
    {
        return clientsNotRouted.OrderByDescending(i => instance.Distances[0, i]).First();
    }

    private (int client, int position) FindBestInsertion(List<int> route, HashSet<int> clientsNotRouted)
    {
        double bestCost = double.MaxValue;
        int bestClient = -1;
        int bestPosition = -1;

        foreach (var client in clientsNotRouted)
        {
            for (int i = 0; i < route.Count - 1; i++)
            {
                double additionalCost = CalculateInsertionCost(route[i], client, route[i + 1]);
                if (additionalCost < bestCost && CanInsert(route, client))
                {
                    bestCost = additionalCost;
                    bestClient = client;
                    bestPosition = i + 1;
                }
            }
        }

        return (bestClient, bestPosition);
    }

    private double CalculateInsertionCost(int from, int client, int to)
    {
        return instance.Distances[from, client] + instance.Distances[client, to] - instance.Distances[from, to];
    }

    private bool CanInsert(List<int> route, int client)
    {
        double currentLoad = route.Where(i => i != 0).Sum(i => instance.ClientDemand[i - 1]);
        return (currentLoad + instance.ClientDemand[client - 1]) <= instance.VehiclesCapacity;
    }

    private void CalculateTotalDistance()
    {
        double totalDistance = 0;
        foreach (var route in routes)
        {
            for (int i = 0; i < route.Count - 1; i++)
            {
                totalDistance += instance.Distances[route[i], route[i + 1]];
            }
        }
        Console.WriteLine($"Distância total percorrida: {totalDistance}");
    }
}
