using System;
using System.Collections.Generic;
using System.Linq;

namespace HashCode2018
{
	public class Problem
	{
		public int Bonus { get; set; }

		public int StepCount { get; set; }

		public List<Ride> Rides { get; }

		public List<Vehicle> Vehicles { get; }

		public Problem()
		{
			Rides = new List<Ride>();
			Vehicles = new List<Vehicle>();
		}
	}

	public class Ride
	{
		public int Id { get; }
		public int StartX { get; }
		public int StartY { get; }
		public int EndX { get; }
		public int EndY { get; }
		public int StartStep { get; }
		public int EndStep { get; }
		public int Distance { get; }

		public bool Done { get; set; }

		public List<(int, Ride)> NextRideList { get; }

		public Ride(int id, int startX, int startY, int endX, int endY, int startStep, int endStep)
		{
			Id = id;
			StartX = startX;
			StartY = startY;
			EndX = endX;
			EndY = endY;
			StartStep = startStep;
			EndStep = endStep;
			Distance = Math.Abs(startX - endX) + Math.Abs(startY - endY);
			NextRideList = new List<(int, Ride)>();
		}

		public void AddNextRide(Ride r)
		{
			NextRideList.Add((DistanceToRide(r), r));
		}

		public void AddNextRide(IEnumerable<Ride> rideList)
		{
			NextRideList.AddRange(rideList.Select(r => (DistanceToRide(r), r)));
		}

		public void SortNextRideList()
		{
			NextRideList.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1));
		}

		public int DistanceToRide(Ride r)
		{
			return Math.Abs(EndX - r.StartX) + Math.Abs(EndY - r.StartY);
		}

		public bool IsUsable(Vehicle vehicle)
		{
			var distanceToNextRide = vehicle.CurrentRide.DistanceToRide(this);
			return !Done && vehicle.CurrentStep + distanceToNextRide + Distance < EndStep;
		}
	}

	public class Vehicle
	{
		public int CurrentStep { get; set; }

		public Ride CurrentRide { get; set; }

		public List<Ride> Rides { get; }

		public bool Finished { get; set; }

		public Vehicle()
		{
			Rides = new List<Ride>();
		}

		public void ToNextRide(Ride nextRide)
		{
			var distanceToNextRide = CurrentRide.DistanceToRide(nextRide);
			var distanceOfNextRide = nextRide.Distance;
			var attente = Math.Max(nextRide.StartStep - (CurrentStep + distanceToNextRide), 0);
			CurrentStep += distanceOfNextRide + attente + distanceToNextRide;
			Rides.Add(nextRide);
			CurrentRide = nextRide;
			nextRide.Done = true;
		}
	}
}