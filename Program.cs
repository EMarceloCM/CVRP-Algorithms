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
using System.Diagnostics;

// string instancePath = "cvrp_instance.txt";
string instancePath = "cvrp_medium_instance.txt";
// string instancePath = "cvrp_big_instance.txt";

ProblemData instanceData = InstanceReader.Read(instancePath);
/*
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
}*/

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

// Métodos para instancias médias (50-200 clientes)
// Savings
// S(i,j) = d(0,i) + d(0,j) - d(i,j) , onde d é a distancia de um ponto ao outro
//SavingsAlgorithm savings = new(instanceData);
//var savingsResult = savings.Execute();

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
//AntColonyOptimization antColony = new(instanceData);
//var antResult = antColony.FindSolution();

// Tabu Search
//TabuSearch tabuSearch = new(instanceData);
//var tabuResult = tabuSearch.Solve();

// ALNS
// ALNS alns = new();
// var alnsResult = alns.AdaptiveLargeNeighborhoodSearch(instanceData);

// Grasp
//Grasp grasp = new();
//var graspResult = grasp.Solve(instanceData);

List<List<int>> solution = new List<List<int>>
{
    new List<int> { 0, 107, 108, 106, 109, 120, 114, 115, 127, 122, 123, 128, 117, 6, 94, 0 },
    new List<int> { 0, 121, 124, 129, 126, 110, 113, 111, 125, 116, 131, 112, 81, 119, 0 },
    new List<int> { 0, 132, 69, 70, 130, 65, 84, 85, 86, 68, 7, 83, 80, 0 },
    new List<int> { 0, 8, 5, 9, 33, 4, 11, 12, 10, 14, 88, 15, 2, 3, 41, 42, 13, 40, 43, 44, 19, 95, 16, 37, 39, 45, 38, 67, 36, 35, 99, 98, 100, 97, 93, 92, 87, 89, 101, 104, 90, 105, 102, 103, 56, 53, 29, 57, 55, 54, 79, 0 },
    new List<int> { 0, 96, 133, 63, 58, 51, 50, 52, 28, 30, 62, 134, 0 },
    new List<int> { 0, 49, 61, 31, 27, 34, 48, 18, 20, 78, 17, 32, 0 },
    new List<int> { 0, 82, 64, 59, 1, 60, 77, 26, 71, 76, 75, 25, 74, 66, 47, 118, 24, 46, 23, 22, 21, 73, 72, 91, 0, 0 }
};

// Local Search 2
LocalSearch2 localSearch2 = new();
var result2 = localSearch2.ImproveSolution(instanceData, solution);
localSearch2.PrintSolution(result2, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms\n");

// Local Search
stopwatch.Restart();
LocalSearch localSearch = new();
var result = localSearch.ImproveSolution(instanceData, solution);
localSearch.PrintSolution(result, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");
