using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public class Coordinate
{
  public double X { get; }
  public double Y { get; }
  public string VehicleRegistration { get; } = string.Empty;

  public Coordinate(double x, double y, string vehicleRegistration)
  {
    X = x;
    Y = y;
    VehicleRegistration = vehicleRegistration;
  }
  public double DistanceTo(Coordinate other)
  {
    double xDiff = X - other.X;
    double yDiff = Y - other.Y;
    return xDiff * xDiff + yDiff * yDiff;
  }
}



public class Program
{
  private const string filename = "VehiclePositions.dat";

  public static void Main(string[] args)
  {
    //latitude, longitude or X, Y
    List<Coordinate> coordinates = new List<Coordinate>{
      new Coordinate(34.544909, -102.100843, ""),
      new Coordinate(32.345544, -99.123124, ""),
      new Coordinate(33.234235, -100.214124, ""),
      new Coordinate(35.195739, -95.348899, ""),
      new Coordinate(31.895839, -97.789573, ""),
      new Coordinate(32.895839, -101.789573, ""),
      new Coordinate(34.115839, -100.225732, ""),
      new Coordinate(32.335839, -99.992232, ""),
      new Coordinate(33.535339, -94.792232, ""),
      new Coordinate(32.234235, -100.22222, ""),
    };

    List<(Coordinate, Coordinate)> coordinateWithNearestCoordinate = new List<(Coordinate, Coordinate)>();

    Stopwatch stopwatch = Stopwatch.StartNew();
    List<Coordinate> fileCoordinates = ParseCoordinatesFromFile(filename);
    stopwatch.Stop();

    long readTime = stopwatch.ElapsedMilliseconds;

    stopwatch.Restart();
    foreach (Coordinate queryCoordinate in CollectionsMarshal.AsSpan(coordinates))
    {
      Coordinate nearestCoordinate = FindNearestCoordinateDivideAndConquer(queryCoordinate, fileCoordinates);

      // Add coordinates to list first
      // coordinateWithNearestCoordinate.Add((queryCoordinate, nearestCoordinate));

      Console.WriteLine($"The nearest coordinate to ({queryCoordinate.X}, {queryCoordinate.Y}) is ({nearestCoordinate.X},{nearestCoordinate.Y}) and has registration number ({nearestCoordinate.VehicleRegistration})");
    }

    //Output of nearest coordinates
    // foreach (var (queryCoordinate, nearestCoordinate) in coordinateWithNearestCoordinate)
    // {
    //   Console.WriteLine($"The nearest coordinate to ({queryCoordinate.X}, {queryCoordinate.Y}) is ({nearestCoordinate.X},{nearestCoordinate.Y}) and has registration number ({nearestCoordinate.VehicleRegistration})");
    // }

    stopwatch.Stop();
    long algorithmTime = stopwatch.ElapsedMilliseconds;

    Console.WriteLine($"\nData file read execution time : {readTime} ms \nClosest position calculation execution time : {algorithmTime} ms \nTotal execution time : {readTime + algorithmTime} ms \n");
  }


  static List<Coordinate> ParseCoordinatesFromFile(string filename)
  {
    List<Coordinate> fileCoordinates = new();
    if (File.Exists(filename))
    {
      byte[] data = File.ReadAllBytes(filename);
      int vehicleId;
      double latitude;
      double longitude;
      ulong recordedTimeUTC;
      StringBuilder vehicleRegistrationBuilder = new StringBuilder();

      int position = 0;
      while (position < data.Length)
      {

        vehicleId = BitConverter.ToInt32(data, position);
        position += 4;

        vehicleRegistrationBuilder.Clear();
        while (data[position] != (byte)0)
        {
          vehicleRegistrationBuilder.Append((char)data[position]);
          position++;
        }
        position++;

        latitude = BitConverter.ToSingle(data, position);
        position += 4;


        longitude = BitConverter.ToSingle(data, position);
        position += 4;

        recordedTimeUTC = BitConverter.ToUInt64(data, position);
        position += 8;

        fileCoordinates.Add(new Coordinate(latitude, longitude, vehicleRegistrationBuilder.ToString()));
      }
    }
    return fileCoordinates;
  }


  static Coordinate FindNearestCoordinateDivideAndConquer(Coordinate queryCoordinate, List<Coordinate> coordinates)
  {
    Coordinate nearestCoordinate;
    double distanceToLeft;
    double distanceToRight;

    if (coordinates.Count == 1)
    {
      return coordinates[0];
    }
    else if (coordinates.Count == 2)
    {
      distanceToLeft = queryCoordinate.DistanceTo(coordinates[0]);
      distanceToRight = queryCoordinate.DistanceTo(coordinates[1]);
      return distanceToLeft < distanceToRight ? coordinates[0] : coordinates[1];
    }

    int middleIndex = coordinates.Count / 2;
    List<Coordinate> leftHalf = coordinates.GetRange(0, middleIndex);
    List<Coordinate> rightHalf = coordinates.GetRange(middleIndex, coordinates.Count - middleIndex);

    Coordinate nearestLeft = FindNearestCoordinateDivideAndConquer(queryCoordinate, leftHalf);
    Coordinate nearestRight = FindNearestCoordinateDivideAndConquer(queryCoordinate, rightHalf);

    distanceToLeft = queryCoordinate.DistanceTo(nearestLeft);
    distanceToRight = queryCoordinate.DistanceTo(nearestRight);

    nearestCoordinate = distanceToLeft < distanceToRight ? nearestLeft : nearestRight;

    return nearestCoordinate;
  }
}
