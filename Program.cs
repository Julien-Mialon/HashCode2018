using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;

namespace HashCode2018
{
	public class Program
	{
		private const string FILE = "a_example.in";

		private static readonly List<string> _files = new List<string>
		{
			"a_example.in",
			"b_should_be_easy.in",
			"c_no_hurry.in",
			"d_metropolis.in",
			"e_high_bonus.in"
		};

		static int RunFile(string file)
		{
			Console.WriteLine($"Start {file}");

			Problem problem = Input(file);

			Solution solution = new Solution(problem);
			solution.Solve();

			Output(file, problem.Vehicles);

			var point = Visualize(problem);
			Console.WriteLine($"Done {file} {point}!");
			return point;
		}

		static int Visualize(Problem problem)
		{
			var startRide = new Ride(-1, 0, 0, 0, 0, 0, 1);
			int point = 0;
			foreach (var vehicle in problem.Vehicles)
			{
				int currentStep = 0;
				var currentRide = startRide;
				foreach (var ride in vehicle.Rides)
				{
					var distanceToRide = currentRide.DistanceToRide(ride);
					var distanceOfRide = ride.Distance;
					var bonus = (currentStep + distanceToRide) <= ride.StartStep ? problem.Bonus : 0;
					var attente = Math.Max(ride.StartStep - (currentStep + distanceToRide), 0);
					currentRide = ride;
					currentStep += distanceToRide + attente + distanceOfRide;
					point += distanceOfRide + bonus;
				}
			}

			return point;
		}

		static void Main(string[] args)
		{
			var point = _files.AsParallel().Sum(RunFile);
			Console.WriteLine(point);
			Console.ReadLine();
		}

		static Problem Input(string file)
		{
			const int HEADER_COUNT = 1;
			string[] lines = ReadFile(file);

			List<int> values = lines[0].Split(' ').Select(int.Parse).ToList();
			Problem result = new Problem
			{
				Bonus = values[4],
				StepCount = values[5]
			};
			result.Vehicles.AddRange(Enumerable.Range(0, values[2]).Select(_ => new Vehicle()));

			int counter = 0;
			foreach (string line in lines.Skip(HEADER_COUNT))
			{
				values = line.Split(' ').Select(int.Parse).ToList();
				result.Rides.Add(new Ride(counter++, values[0], values[1], values[2], values[3], values[4], values[5]));
			}

			return result;
		}

		static void Output(string file, List<Vehicle> data)
		{
			List<string> lines = data.Select(x => $"{x.Rides.Count} {string.Join(" ", x.Rides.Select(ride => ride.Id))}").ToList();

			WriteFile(file, lines);
		}

		static string[] ReadFile(string file)
		{
			return File.ReadAllLines(file);
		}

		static void WriteFile(string file, List<string> lines)
		{
			File.WriteAllText($"{file}.out", string.Join("\n", lines));
		}
	}

	public class Solution
	{
		private readonly Problem _problem;

		public Solution(Problem problem)
		{
			_problem = problem;
		}

		public void Solve()
		{
			//Init
			var startRide = new Ride(-1, 0, 0, 0, 0, 0, 1);
			foreach (var currentRide in _problem.Rides)
			{
				startRide.AddNextRide(currentRide);
				foreach (var ride in _problem.Rides)
				{
					currentRide.AddNextRide(ride);
				}
			}

			foreach (var vehicle in _problem.Vehicles)
			{
				vehicle.CurrentRide = startRide;
				vehicle.CurrentStep = 0;
			}

			//Tour de jeu
			if (_problem.Bonus > _problem.Rides.Count / 30)
			{
				for (var currentStep = 0; currentStep < _problem.StepCount; currentStep++)
				{
					var usableVehicle = _problem.Vehicles.AsParallel().Where(x => !x.Finished).ToList();
					if (usableVehicle.Count == 0)
					{
						break;
					}

					foreach (var vehicle in usableVehicle)
					{
						var usableRide = vehicle.CurrentRide.NextRideList.AsParallel().Where(tuple => tuple.ride.IsUsable(vehicle)).ToList();
						if (usableRide.Count == 0)
						{
							vehicle.Finished = true;
							continue;
						}

						var ( _, nextRide) = usableRide.OrderByDescending(tuple => Score(tuple.ride, vehicle, _problem.Bonus)).FirstOrDefault();

						vehicle.ToNextRide(nextRide);
					}
				}
			}
			else
			{
				foreach (var vehicle in _problem.Vehicles)
				{
					while (true)
					{
						var usableRide = vehicle.CurrentRide.NextRideList.AsParallel().Where(tuple => tuple.ride.IsUsable(vehicle));
						var(_, nextRide) = usableRide.OrderByDescending(tuple => Score(tuple.ride, vehicle, _problem.Bonus)).FirstOrDefault();
						if (nextRide == null)
						{
							break;
						}

						vehicle.ToNextRide(nextRide);
					}
				}
			}
		}

		public int Score(Ride ride, Vehicle vehicle, int bonusPoint)
		{
			var distanceToNextRide = vehicle.CurrentRide.DistanceToRide(ride);
			var distanceOfRide = ride.Distance;
			var deltaStart = ride.StartStep - (vehicle.CurrentStep + distanceToNextRide);
			var attente = Math.Max(deltaStart, 0);
			var bonus = deltaStart == 0 ? bonusPoint : 0;
			var malus = ride.NextRideList.AsParallel().Where(tuple => !tuple.ride.Done).Min(tuple => tuple.distance);
			return distanceOfRide - distanceToNextRide - malus + (bonus - attente);
		}
	}
}