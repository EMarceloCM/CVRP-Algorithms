using CVRP.Utils;

namespace CVRP.Nearest_Neighbor;

public class NearestNeighborAlgorithm(ProblemData instance)
{
    private readonly ProblemData instance = instance;
    private readonly List<List<int>> routes = [];

    public List<List<int>> Solve()
    {
        var clientsNotRouted = new HashSet<int>(Enumerable.Range(1, instance.NumberOfClients));

        while (clientsNotRouted.Count != 0)
        {
            var route = new List<int> { 0 };
            double currentLoad = 0;

            while (clientsNotRouted.Count != 0)
            {
                int nearestClient = FindNearestNeighbor(route.Last(), clientsNotRouted, currentLoad);
                if (nearestClient == -1) break;

                if (currentLoad + instance.ClientDemand[nearestClient != 0 ? nearestClient - 1 : 0] > instance.VehiclesCapacity)
                    break;

                route.Add(nearestClient);
                currentLoad += instance.ClientDemand[nearestClient != 0 ? nearestClient - 1 : 0];
                clientsNotRouted.Remove(nearestClient);
            }

            route.Add(0);
            routes.Add(route);
        }

        PrintRoutes();
        CalculateTotalDistance();
        return routes;
    }

    private int FindNearestNeighbor(int currentClient, HashSet<int> clientsNotRouted, double currentLoad)
    {
        var nearest = clientsNotRouted.OrderBy(i => instance.Distances[currentClient, i]).FirstOrDefault();
        HashSet<int> aux = [.. clientsNotRouted];

        while (currentLoad + instance.ClientDemand[nearest != 0 ? nearest - 1 : 0] > instance.VehiclesCapacity && aux.Count > 0)
        {
            aux.Remove(nearest);
            nearest = aux.OrderBy(i => instance.Distances[currentClient, i]).FirstOrDefault();
        }

        return nearest;
    }

    private void PrintRoutes()
    {
        Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine("Rotas geradas (Nearest Neighbor Modified):");
        for (int i = 0; i < routes.Count; i++)
        {
            Console.WriteLine($"Rota {i + 1}: {string.Join(" -> ", routes[i])}");
        }
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