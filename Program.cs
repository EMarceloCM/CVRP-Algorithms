using CVRP.ALNS;
using CVRP.Ant_Colony;
using CVRP.GeneticAlgorithm;
using CVRP.Grasp;
using CVRP.Insertion;
using CVRP.Local_Search;
using CVRP.Nearest_Neighbor;
using CVRP.Savings;
using CVRP.Tabu_Search;
using CVRP.Utils;

string instancePath = "cvrp_instance.txt";
//string bigInstancePath = "cvrp_big_instance.txt";

ProblemData instanceData = InstanceReader.Read(instancePath);

Console.WriteLine("Número de Clientes: " + instanceData.NumberOfClients);
Console.WriteLine("Capacidade do Veículo: " + instanceData.VehiclesCapacity);
Console.WriteLine("Demandas dos Clientes: " + string.Join(", ", instanceData.ClientDemand));
Console.WriteLine("--------------------------------------------------------");
Console.WriteLine("Matriz de Distâncias: ");
for (int i = 0; i < instanceData.Distances.GetLength(0); i++)
{
    for (int j = 0; j < instanceData.Distances.GetLength(1); j++)
    {
        Console.Write($"início: {i}, fim: {j}, distância: " + instanceData.Distances[i, j] + "\n");
    }
    Console.WriteLine();
}

// Métodos para instancias médias (50-200 clientes)
// Savings
// S(i,j) = d(0,i) + d(0,j) - d(i,j) , onde d é a distancia de um ponto ao outro
SavingsAlgorithm savings = new(instanceData);
var savingsResult = savings.Execute();
// Insertion
// IC(from, client, to) = IC(from, client) + IC(client, to) - IC(from, to);
//InsertionAlgorithm insertion = new(instanceData);
//var insertionResult = insertion.Solve();
// Nearest Neighbor Modified
//NearestNeighborAlgorithm nearestNeighbor = new(instanceData);
//var nearestResult = nearestNeighbor.Solve();

// Métodos para instancias grandes (> 200 clientes)
// Genetic Algorithm
//GeneticAlgorithm geneticAlgorithm = new(instanceData);
//var geneticResult = geneticAlgorithm.Solve();
// Ant Colony Optimization
// AntColonyOptimization antColony = new(instanceData);
// var antResult = antColony.FindSolution();
// Tabu Search
//TabuSearch tabuSearch = new(instanceData);
//var tabuResult = tabuSearch.Solve();
// ALNS
// ALNS alns = new();
// var alnsResult = alns.AdaptiveLargeNeighborhoodSearch(instanceData);
// Local Search
// LocalSearch localSearch = new();
//localSearch.ImproveSolution(instanceData, savingsResult, initialSolutionAlgorithm: "Savings");
//localSearch.ImproveSolution(instanceData, insertionResult, initialSolutionAlgorithm: "Insertion");
//localSearch.ImproveSolution(instanceData, nearestResult, initialSolutionAlgorithm: "Nearest Neighbor");
//localSearch.ImproveSolution(instanceData, geneticResult, initialSolutionAlgorithm: "Genetic Algorithm");
//localSearch.ImproveSolution(instanceData, antResult!, initialSolutionAlgorithm: "Ant Colony Optmization");
//localSearch.ImproveSolution(instanceData, tabuResult, initialSolutionAlgorithm: "Tabu Search");
//localSearch.ImproveSolution(instanceData, alnsResult, initialSolutionAlgorithm: "ALNS");

// Grasp
// Grasp grasp = new();
// var graspResult = grasp.Solve(instanceData);

// Utilizar o resultado prévio das heurísticas aqui para aprimorar utilizando as meta-heurísticas