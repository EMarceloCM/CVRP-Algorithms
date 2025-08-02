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
// string instancePath = "cvrp_medium_instance.txt";
string instancePath = "cvrp_big_instance.txt";

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
int seed = 2025;

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
//GeneticAlgorithm geneticAlgorithm = new(instanceData, seed);
//var geneticResult = geneticAlgorithm.Solve();

// Ant Colony Optimization
//AntColonyOptimization antColony = new(instanceData, seed = seed);
//var antResult = antColony.FindSolution();

// Tabu Search
//TabuSearch tabuSearch = new(instanceData);
//var tabuResult = tabuSearch.Solve();

// ALNS
//ALNS alns = new(seed);
//var alnsResult = alns.AdaptiveLargeNeighborhoodSearch(instanceData);

// Grasp
//Grasp grasp = new(seed);
//grasp.Solve(instanceData);

/*
List<List<int>> solution = new List<List<int>>
{
    new List<int> { 0, 190, 31, 70, 116, 121, 134, 170, 67, 73, 21, 53, 0 },
    new List<int> { 0, 6, 114, 8, 174, 124, 168, 36, 143, 175, 48, 52, 167, 0 },
    new List<int> { 0, 152, 58, 198, 110, 197, 56, 22, 15, 144, 117, 97, 93, 104, 99, 0 },
    new List<int> { 0, 12, 80, 163, 24, 130, 55, 25, 187, 139, 39, 23, 186, 4, 179, 0 },
    new List<int> { 0, 68, 169, 78, 81, 185, 79, 3, 158, 77, 196, 0 },
    new List<int> { 0, 184, 150, 29, 34, 164, 135, 35, 136, 65, 71, 103, 120, 0 },
    new List<int> { 0, 11, 126, 63, 181, 128, 30, 122, 50, 157, 129, 76, 138, 28, 105, 0 },
    new List<int> { 0, 132, 176, 102, 33, 51, 188, 66, 160, 108, 189, 0 },
    new List<int> { 0, 9, 161, 20, 131, 32, 90, 159, 62, 123, 194, 166, 0 },
    new List<int> { 0, 40, 171, 57, 42, 37, 193, 191, 86, 61, 173, 118, 89, 0 },
    new List<int> { 0, 112, 83, 199, 113, 17, 125, 46, 106, 7, 182, 10, 69, 27, 146, 0 },
    new List<int> { 0, 180, 74, 133, 75, 155, 165, 109, 195, 149, 26, 154, 0 },
    new List<int> { 0, 153, 82, 19, 49, 64, 107, 148, 88, 127, 162, 101, 1, 111, 0 },
    new List<int> { 0, 137, 172, 43, 142, 14, 38, 140, 141, 44, 119, 192, 100, 91, 60, 18, 0 },
    new List<int> { 0, 47, 45, 84, 16, 85, 98, 151, 92, 59, 96, 156, 0 },
    new List<int> { 0, 177, 54, 72, 41, 145, 115, 178, 2, 87, 95, 13, 0 },
    new List<int> { 0, 183, 94, 5, 147, 0 }
};

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
*/

/* Calcular distância da solução ótima do problema
LocalSearch l = new();
double d = l.CalculateTotalDistance(solution, instanceData);
Console.WriteLine("\nDistancia total percorrida pela solucao otima: " + d + "\n");
*/

stopwatch.Stop();
Console.WriteLine("\nTempo de execução: " + stopwatch.ElapsedMilliseconds + " ms");

//float valor = 1609.61f;
//float melhor = 1628.57f; 
//float resultado = ((valor - melhor) / melhor) * 100;
//Console.WriteLine(resultado);
