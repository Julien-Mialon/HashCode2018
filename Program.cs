using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HashCode2018
{
	public class Program
	{
		private const string FILE = "a_example.in";

		private static List<string> _files = new List<string>
		{
			"a_example.in",
			"b_should_be_easy.in",
			"c_no_hurry.in",
			"d_metropolis.in",
			"e_high_bonus.in"
		};

		static void RunFile(string file)
		{
			Console.WriteLine($"Start {file}");

			Problem problem = Input(file);

			Solution solution = new Solution(problem);
			solution.Solve();

			Output(file, problem.Vehicles);

			Console.WriteLine("Done!");
		}

		static void Main(string[] args)
		{
			foreach (string file in _files)
			{
				RunFile(file);
			}
		}

		static Problem Input(string file)
		{
			const int HEADER_COUNT = 1;
			string[] lines = ReadFile(file);

			List<int> values = lines[0].Split(' ').Select(int.Parse).ToList();
			Problem result = new Problem
			{
				Vehicles = Enumerable.Range(0, values[2]).Select(_ => new Vehicle()).ToList(),
				Bonus = values[4],
				StepCount = values[5]
			};

			int counter = 0;
			foreach (string line in lines.Skip(HEADER_COUNT))
			{
				values = line.Split(' ').Select(int.Parse).ToList();
				result.Rides.Add(new Ride
				{
					Id = counter++,
					StartX = values[0],
					StartY = values[1],
					EndX = values[2],
					EndY = values[3],
					StartStep = values[4],
					EndStep = values[5],
				});
			}

			return result;
		}

		static void Output(string file, List<Vehicle> data)
		{
			List<string> lines = data.Select(x => $"{x.Rides.Count} {string.Join(" ", x.Rides)}").ToList();

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

	public class Node
	{
		public Node()
		{
			Links = new List<(int, Node)>();
		}

		public bool Done { get; set; }

		public Ride Ride { get; set; }

		public bool IsStart { get; set; }

		public List<(int, Node)> Links { get; set; }
	}

	public class Solution
	{
		private readonly Problem _problem;
		private int _currentStep = 0;

		public Solution(Problem problem)
		{
			_problem = problem;

			_problem.Rides.ForEach(x => x.Distance = Distance(x));
		}

		private Node CreateNode()
		{
			Node startNode = new Node
			{
				Ride = null,
			};

			List<Node> endNodes = new List<Node>();
			startNode.Links.AddRange(_problem.Rides.Select(ride =>
			{
				Node startRide = new Node
				{
					Ride = ride,
					IsStart = true,
				};

				startRide.Links.Add((ride.Distance, new Node
				{
					Ride = ride,
					IsStart = false,
				}));
				endNodes.Add(startRide.Links[0].Item2);
				return (DistanceStart(ride), startRide);
			}));

			foreach (Node startTrip in startNode.Links.Select(x => x.Item2))
			{
				foreach (Node endTrip in endNodes)
				{
					if (startTrip.Ride != endTrip.Ride)
					{
						endTrip.Links.Add((Distance(endTrip.Ride, startTrip.Ride), startTrip));
					}
				}
			}

			return startNode;
		}

		public void Solve()
		{
			Node start = CreateNode();
			_problem.Vehicles.ForEach(x => x.Node = start);

			List<Vehicle> vehicles = _problem.Vehicles.ToArray().ToList();
			for (int stepIndex = 0; stepIndex < _problem.StepCount; stepIndex++)
			{
				_currentStep = stepIndex;
				for (var j = 0; j < vehicles.Count; j++)
				{
					Vehicle v = vehicles[j];

					if (v.CurrentStep > stepIndex)
					{
						continue;
					}

					AffectVehicle(v);

					if (v.Finished)
					{
						vehicles.RemoveAt(j);
						j--;
					}
				}
			}
		}

		public void AffectVehicle(Vehicle v)
		{
			Node next = null;
			int distanceToNode = 0;
			int maxScore = -100_000_000;

			Node start = v.Node;
			foreach ((int distance, Node node) in start.Links)
			{
				if (IsUsable(start, distance, node))
				{
					int result = Score(start, distance, node);

					if (maxScore < result)
					{
						distanceToNode = distance;
						maxScore = result;
						next = node;
					}
				}
			}

			if (next == null)
			{
				v.Finished = true;
			}
			else
			{
				v.Rides.Add(next.Ride.Id);

				v.CurrentStep += distanceToNode + next.Ride.Distance;

				//add waiting time before ride
				int arrival = _currentStep + distanceToNode;
				if (arrival < next.Ride.StartStep)
				{
					v.CurrentStep += next.Ride.StartStep - arrival;
				}

				//mark start & end node as "Done"
				v.Node = next.Links[0].Item2;
				next.Done = true;
				v.Node.Done = true;
			}
		}

		public bool IsUsable(Node x, int distance, Node y)
		{
			if (y.Done)
			{
				return false;
			}

			if (y.IsStart)
			{
				int tripDistance = y.Ride.Distance;
				int arrival = _currentStep + distance;

				//add time before beginning
				int add = 0;
				if (arrival < y.Ride.StartStep)
				{
					add = y.Ride.StartStep - arrival;
				}

				if (arrival + add + tripDistance >= y.Ride.EndStep)
				{
					return false;
				}
			}

			return true;
		}

		public int Score(Node x, int distance, Node y)
		{
			int tripDistance = y.Ride.Distance;
			int arrival = _currentStep + distance;
			int add = 0;
			//add time before ride begin
			if (arrival < y.Ride.StartStep)
			{
				add = y.Ride.StartStep - arrival;
			}

			//add bonus point
			int bonus = 0;
			if (arrival <= y.Ride.StartStep)
			{
				bonus = _problem.Bonus;
			}

			// trajet + bonus - (temps avant démarrage)
			return tripDistance + bonus - (distance + add);
		}

		public static int DistanceStart(Ride r)
		{
			return Math.Abs(r.StartX) + Math.Abs(r.StartY);
		}

		public static int Distance(Ride r1, Ride r2)
		{
			return Math.Abs(r1.EndX - r2.StartX) + Math.Abs(r1.EndY - r2.StartY);
		}

		public static int Distance(Ride r)
		{
			return Math.Abs(r.EndX - r.StartX) + Math.Abs(r.EndY - r.StartY);
		}
	}
}