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

string instancePath = "cvrp_instance.txt";
//string mediumInstancePath = "cvrp_medium_instance.txt";
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
Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
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

List<List<int>> savingsSolution = new List<List<int>>
{
    new List<int> { 0, 4, 11, 8, 28, 9, 22, 18, 3, 23, 10, 29, 0 },
    new List<int> { 0, 2, 17, 19, 15, 31, 14, 26, 0 },
    new List<int> { 0, 25, 21, 5, 6, 13, 7, 20, 0 },
    new List<int> { 0, 1, 12, 27, 16, 30, 0 },
    new List<int> { 0, 24, 0 }
};

List<List<int>> insertionSolution = new List<List<int>>
{
    new List<int> { 0, 14, 6, 3, 28, 4, 11, 8, 18, 24, 0 },
    new List<int> { 0, 26, 27, 23, 22, 9, 15, 10, 29, 5, 20, 0 },
    new List<int> { 0, 30, 16, 7, 13, 21, 2, 0 },
    new List<int> { 0, 12, 1, 31, 19, 25, 0 },
    new List<int> { 0, 17, 0 }
};

List<List<int>> nearestNeighborSolution = new List<List<int>>
{
    new List<int> { 0, 30, 26, 16, 12, 1, 7, 14, 29, 22, 18, 0 },
    new List<int> { 0, 24, 27, 20, 5, 25, 10, 8, 0 },
    new List<int> { 0, 13, 21, 31, 19, 17, 3, 23, 0 },
    new List<int> { 0, 6, 2, 28, 4, 11, 9, 0 },
    new List<int> { 0, 15, 0 }
};

List<List<int>> geneticV1Solution = new List<List<int>>
{
    new List<int> { 0, 22, 4, 11, 12, 28, 24, 0 },
    new List<int> { 0, 16, 1, 31, 19, 17, 3, 26, 0 },
    new List<int> { 0, 21, 7, 14, 27, 29, 18, 2, 6, 20, 0 },
    new List<int> { 0, 5, 25, 10, 15, 9, 8, 23, 0 },
    new List<int> { 0, 13, 30, 0 }
};

List<List<int>> geneticV2Solution = new List<List<int>>
{
    new List<int> { 0, 22, 9, 11, 28, 4, 13, 0 },
    new List<int> { 0, 26, 31, 19, 17, 2, 3, 23, 8, 18, 14, 0 },
    new List<int> { 0, 6, 21, 7, 1, 12, 16, 0 },
    new List<int> { 0, 30, 5, 29, 15, 10, 25, 20, 0 },
    new List<int> { 0, 27, 24, 0 }
};

List<List<int>> antColonyV1Solution = new List<List<int>>
{
    new List<int> { 0, 20, 5, 25, 10, 15, 29, 22, 9, 8, 18, 0 },
    new List<int> { 0, 30, 26, 16, 7, 1, 12, 0 },
    new List<int> { 0, 13, 31, 19, 17, 21, 0 },
    new List<int> { 0, 6, 3, 23, 2, 11, 4, 28, 0 },
    new List<int> { 0, 27, 24, 14, 0 }
};

List<List<int>> antColonyV2Solution = new List<List<int>>
{
    new List<int> { 0, 30, 26, 16, 12, 1, 7, 0 },
    new List<int> { 0, 19, 17, 31, 21, 13, 0 },
    new List<int> { 0, 6, 3, 2, 23, 28, 4, 11, 0 },
    new List<int> { 0, 20, 5, 25, 10, 29, 15, 9, 22, 18, 8, 0 },
    new List<int> { 0, 14, 24, 27, 0 }
};

// Local Search
LocalSearch localSearch = new();
var savingsResult = localSearch.ImproveSolution(instanceData, savingsSolution, "Savings");
localSearch.PrintSolution(savingsResult, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");
/*
stopwatch.Restart();
var insertionResult = localSearch.ImproveSolution(instanceData, insertionSolution, "Insertion");
localSearch.PrintSolution(insertionResult, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");

stopwatch.Restart();
var nearestResult = localSearch.ImproveSolution(instanceData, nearestNeighborSolution, "Nearest Neighbor");
localSearch.PrintSolution(nearestResult, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");

stopwatch.Restart();
var geneticV1Result = localSearch.ImproveSolution(instanceData, geneticV1Solution, "Genetic Algorithm");
localSearch.PrintSolution(geneticV1Result, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");

stopwatch.Restart();
var geneticV2Result = localSearch.ImproveSolution(instanceData, geneticV2Solution, "Genetic Algorithm");
localSearch.PrintSolution(geneticV2Result, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");

stopwatch.Restart();
var antColonyV1Result = localSearch.ImproveSolution(instanceData, antColonyV1Solution, "Ant Colony");
localSearch.PrintSolution(antColonyV1Result, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");

stopwatch.Restart();
var antColonyV2Result = localSearch.ImproveSolution(instanceData, antColonyV2Solution, "Ant Colony");
localSearch.PrintSolution(antColonyV2Result, instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");
*/

// Grasp
// Grasp grasp = new();
// var graspResult = grasp.Solve(instanceData);
stopwatch.Stop();
Console.WriteLine("Tempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");
// Utilizar o resultado prévio das heurísticas aqui para aprimorar utilizando as meta-heurísticas
