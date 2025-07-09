using CVRP.Utils;

namespace CVRP.Savings;

public class SavingsAlgorithm(ProblemData instance)
{
    private readonly ProblemData instance = instance;
    private readonly double[,] saved = new double[instance.NumberOfClients + 1, instance.NumberOfClients + 1];
    private List<(double economy, int i, int j)> savingsList = [];

    public List<List<int>> Execute()
    {
        CalculateSavings();

        List<List<int>> routes = new(instance.NumberOfVehicles);

        HashSet<int> visitedClients = [];

        for (int x = 0; x < instance.NumberOfVehicles; x++)
        {
            routes.Add([]);
            routes[x].Add(0); // veículo x sai da base
            double remainingCapacity = instance.VehiclesCapacity;

            Console.WriteLine($"\nConstruindo rota do veículo {x + 1}:");
            Console.WriteLine($"Estado inicial: {string.Join(" -> ", routes[x])} (capacidade restante: {remainingCapacity})");

            foreach (var (economy, i, j) in savingsList)
            {
                if (visitedClients.Contains(i) && visitedClients.Contains(j))
                    continue;

                // tenta adicionar par (i,j)
                if (!visitedClients.Contains(i) && !visitedClients.Contains(j)
                    && instance.ClientDemand[i == 0 ? 0 : i - 1] + instance.ClientDemand[j == 0 ? 0 : j - 1] <= remainingCapacity)
                {
                    routes[x].Add(i);
                    routes[x].Add(j);
                    remainingCapacity -= instance.ClientDemand[i == 0 ? 0 : i - 1]
                                      + instance.ClientDemand[j == 0 ? 0 : j - 1];
                    visitedClients.Add(i);
                    visitedClients.Add(j);

                    Console.WriteLine(
                        $"Adicionado par ({i}, {j}); rota agora: {string.Join(" -> ", routes[x])} " +
                        $"(capacidade restante: {remainingCapacity})");
                }
                else if (!visitedClients.Contains(i) && !visitedClients.Contains(j)
                         && instance.ClientDemand[i == 0 ? 0 : i - 1] + instance.ClientDemand[j == 0 ? 0 : j - 1] > remainingCapacity)
                {
                    // sem capacidade para o par inteiro
                    continue;
                }
                else if (!visitedClients.Contains(i)
                         && instance.ClientDemand[i == 0 ? 0 : i - 1] <= remainingCapacity)
                {
                    routes[x].Add(i);
                    remainingCapacity -= instance.ClientDemand[i == 0 ? 0 : i - 1];
                    visitedClients.Add(i);

                    Console.WriteLine(
                        $"Adicionado i = {i}; rota agora: {string.Join(" -> ", routes[x])} " +
                        $"(capacidade restante: {remainingCapacity})");
                }
                else if (!visitedClients.Contains(j)
                         && instance.ClientDemand[j == 0 ? 0 : j - 1] <= remainingCapacity)
                {
                    routes[x].Add(j);
                    remainingCapacity -= instance.ClientDemand[j == 0 ? 0 : j - 1];
                    visitedClients.Add(j);

                    Console.WriteLine(
                        $"Adicionado j = {j}; rota agora: {string.Join(" -> ", routes[x])} " +
                        $"(capacidade restante: {remainingCapacity})");
                }
            }

            // fecha a rota
            routes[x].Add(0);
            Console.WriteLine($"Finalizada rota do veículo {x + 1}: {string.Join(" -> ", routes[x])}");
        }

        // clientes remanescentes
        for (int client = 1; client <= instance.NumberOfClients; client++)
        {
            if (!visitedClients.Contains(client))
            {
                routes.Add(new List<int> { 0, client, 0 });
                Console.WriteLine(
                    $"\nCliente isolado {client} gerou rota: {string.Join(" -> ", routes.Last())}");
            }
        }

        Console.WriteLine("\n--------------------------------------------------------");
        Console.WriteLine("Rotas geradas (heurística de economias):");
        for (int vehicle = 0; vehicle < routes.Count; vehicle++)
        {
            if (routes[vehicle].All(c => c == 0)) continue;
            Console.WriteLine($"Rota {vehicle + 1}: {string.Join(" -> ", routes[vehicle])}");
        }

        CalculateTotalDistance(routes);

        return routes;
    }

    private void CalculateSavings()
    {
        for (int i = 0; i < instance.Distances.GetLength(0); i++)
        {
            for (int j = 0; j < instance.Distances.GetLength(1); j++)
            {
                if (i == j) continue;
                saved[i, j] = instance.Distances[0, i] + instance.Distances[0, j] - instance.Distances[i, j];
            }
        }

        for (int i = 0; i < instance.NumberOfClients + 1; i++)
        {
            for (int j = i + 1; j < instance.NumberOfClients + 1; j++)
            {
                double arredondado = Math.Round(saved[i, j], 2);
                savingsList.Add((arredondado, i, j));
            }
        }

        savingsList = savingsList.OrderByDescending(s => s.economy).ToList();

        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine("Economias ordenadas de forma decrescente:");
        foreach (var (economy, i, j) in savingsList)
        {
            Console.WriteLine($"Economia: {economy}, Clientes: ({i}, {j})");
        }
    }

    private void CalculateTotalDistance(List<List<int>> routes)
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
