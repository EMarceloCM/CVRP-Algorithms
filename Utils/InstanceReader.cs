using System.Globalization;

namespace CVRP.Utils;

public static class InstanceReader
{
    private static int NumberOfClients = 0;
    private static int NumberOfVehicles = 0;
    private static float VehicleCapacity = 0.0f;
    private static int[] ClientDemand = [];
    private static double[,] Distances = new double[0,0];

    public static ProblemData Read(string instancePath)
    {
        if (!File.Exists(instancePath))
            throw new Exception("Arquivo especificado não existe!");

        StreamReader sr = new(instancePath);
        sr.ReadLine();

        string[] variables = sr.ReadLine()!.Split('-');
        NumberOfClients = int.Parse(variables[0]);
        NumberOfVehicles = int.Parse(variables[1]);
        VehicleCapacity = int.Parse(variables[2]);
        ClientDemand = new int[NumberOfClients];

        string[] demands = variables[3].Trim('[', ']').Split(',');

        for (int i = 0; i < NumberOfClients; i++)
        {
            ClientDemand[i] = int.Parse(demands[i]);
        }
        sr.ReadLine();
        sr.ReadLine();

        Distances = new double[NumberOfClients + 1, NumberOfClients + 1];
        
        string? line;
        while((line = sr.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(','); // o depósito será considerado como o ponto '0' e os clientes os demais pontos
            int start = int.Parse(parts[0].Trim());
            int end = int.Parse(parts[1].Trim());
            double distance = double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);

            Distances[start, end] = distance;
            Distances[end, start] = distance;
        }
        sr.Dispose();
        sr.Close();

        return new ProblemData(NumberOfClients, NumberOfVehicles, VehicleCapacity, ClientDemand, Distances);
    }
}