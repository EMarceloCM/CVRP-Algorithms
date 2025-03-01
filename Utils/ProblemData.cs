namespace CVRP.Utils;
public class ProblemData
{
    public int NumberOfClients { get; set; }
    public int NumberOfVehicles { get; set; }
    public double VehiclesCapacity { get; set; }
    public int[] ClientDemand { get; set; }
    public double[,] Distances { get; set; }

    public ProblemData(int clientsNumber, int vehiclesNumber, double capacity, int[] demands, double[,] distances)
    {
        NumberOfClients = clientsNumber;
        NumberOfVehicles = vehiclesNumber;
        VehiclesCapacity = capacity;
        ClientDemand = demands;
        Distances = distances;
    }

    public ProblemData()
    {
        NumberOfClients = 0;
        NumberOfVehicles = 0;
        VehiclesCapacity = 0.0f;
        ClientDemand = [];
        Distances = new double[0, 0];
    }
}