using System.Collections.Generic;

namespace HashCode2018
{
	public class Problem
	{
		public int Bonus { get; set; }
		
		public int StepCount { get; set; }
		
		public List<Ride> Rides { get; set; }
		
		public List<Vehicle> Vehicles { get; set; }

		public Problem()
		{
			Rides = new List<Ride>();
			Vehicles =new List<Vehicle>();
		}
	}

	public class Ride
	{
		public int Id { get; set; }
		
		public int StartX { get; set; }
		
		public int StartY { get; set; }
		
		public int EndX { get; set; }
		
		public int EndY { get; set; }
		
		public int StartStep { get; set; }
		
		public int EndStep { get; set; }
		
		public int Distance { get; set; }
	}

	public class Vehicle
	{
		public int CurrentStep { get; set; }
		
		public bool Finished { get; set; }
		
		public List<int> Rides { get; set; }

		public Node Node { get; set; }
		
		public Vehicle()
		{
			Rides = new List<int>();
		}
	}
}