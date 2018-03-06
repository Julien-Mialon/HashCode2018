using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HashCode2018
{
	/*
	 REFS
	 https://resources.mpi-inf.mpg.de/departments/d1/teaching/ss12/AdvancedGraphAlgorithms/Slides06.pdf
	 https://brilliant.org/wiki/hungarian-matching/
	 https://en.wikipedia.org/wiki/Hungarian_algorithm
	 https://www.topcoder.com/community/data-science/data-science-tutorials/assignment-problem-and-hungarian-algorithm/
	 https://stackoverflow.com/a/37956987/1479638
	 
	 
	 https://github.com/antifriz/hungarian-algorithm-n3/blob/master/src/HungarianAlgorithm.cs

max score
  b = 180 798 (177 027)
  c = 16 750 973
  d = 14 272 704 (14 186 252)
  e = 21 601 343 (21 568 975)
max = 52 805 818 (+10)

	 */

	public class Program
	{
		private const string FILE = "a_example.in";

		private static List<string> _files = new List<string>
		{
			//"a_example.in",
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
			Stopwatch watcher = Stopwatch.StartNew();
			solution.Solve();
			long elapsed = watcher.ElapsedMilliseconds;
			watcher.Stop();

			int score = solution.Score();
			Output($"{file}.{score}", problem.Vehicles);

			Console.WriteLine($"Done! {file} => {score} ({elapsed}ms)");
		}

		static void Main(string[] args)
		{
			//_files.AsParallel().ForAll(RunFile);
			RunFile(_files[0]);
			RunFile(_files[1]);
			RunFile(_files[2]);
			RunFile(_files[3]);
			
			Console.ReadLine();
		}

		static int[][] Create(int height, int width)
		{
			Random random = new Random();
			int[][] result = new int[height][];
			for (var i = 0; i < height; i++)
			{
				result[i] = new int[width];
				for (var j = 0; j < width; j++)
				{
					result[i][j] = random.Next(0, 100);
				}
			}
			return result;
		}

		static Problem Input(string file)
		{
			const int HEADER_COUNT = 1;
			string[] lines = ReadFile(file);

			List<int> values = lines[0].Split(' ').Select(int.Parse).ToList();
			int vehiclesCount = values[2];
			int ridesCount = values[3];
			int bonus = values[4];
			int stepCount = values[5];
			List<Ride> rides = new List<Ride>(ridesCount);

			for (int lineIndex = 1, rideId = 0; rideId < ridesCount; ++rideId, ++lineIndex)
			{
				values = lines[lineIndex].Split(' ').Select(int.Parse).ToList();
				rides.Add(new Ride
				{
					Id = rideId,
					StartX = values[0],
					StartY = values[1],
					EndX = values[2],
					EndY = values[3],
					StartStep = values[4],
					EndStep = values[5],
				});
			}

			Problem result = new Problem(bonus, stepCount, rides);
			List<Vehicle> vehicles = new List<Vehicle>(vehiclesCount);
			for (int i = 0; i < vehiclesCount; ++i)
			{
				vehicles.Add(new Vehicle(i, result));
			}

			result.Vehicles = vehicles;

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

		public List<(int, Node)> Links { get; set; }
	}

	public class Constant
	{
		public const int MIN = -100_000_000;
		public const int BONUS_MULTIPLIER = 1;
	}

	public class Problem
	{
		public int Bonus { get; }

		public int StepCount { get; }

		public List<Ride> Rides { get; }

		public List<Vehicle> Vehicles { get; set; }

		public int CurrentStep { get; set; }

		public int[] NotUsableScores { get; }

		public Problem(int bonus, int stepCount, List<Ride> rides)
		{
			Bonus = bonus;
			StepCount = stepCount;
			Rides = rides;
			NotUsableScores = Rides.Select(_ => Constant.MIN).ToArray();
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
		private readonly Problem _problem;
		private readonly int[] _scores;

		public int Id { get; }

		public int CurrentStep { get; set; }

		public bool Finished { get; set; }

		public List<int> Rides { get; set; }

		public Node Node { get; set; }

		public Vehicle(int id, Problem problem)
		{
			Id = id;
			_problem = problem;
			Rides = new List<int>();
			_scores = problem.Rides.Select(_ => Constant.BONUS_MULTIPLIER).ToArray();
		}

		public int[] UpdateNodesScore(int[] scoreIndexMapping)
		{
			for (var i = 0; i < _scores.Length; i++)
			{
				_scores[i] = Constant.MIN;
			}

			for (int index = 0; index < Node.Links.Count; index++)
			{
				(int distance, Node node) = Node.Links[index];
				int id = node.Ride.Id;
				int scoreIndex = scoreIndexMapping[id];
				if (scoreIndex < 0)
				{
					continue;
				}

				(bool result, int score) = GetScoreForNode(distance, node);
				if (!result)
				{
					continue;
				}

				_scores[scoreIndex] = score;

			}

			return _scores;
		}

		private (bool, int) GetScoreForNode(int distance, Node dest)
		{
			int tripDistance = dest.Ride.Distance;
			int arrival = _problem.CurrentStep + distance;

			//add time before beginning
			int add = 0;
			if (arrival < dest.Ride.StartStep)
			{
				add = dest.Ride.StartStep - arrival;
			}

			if (arrival + add + tripDistance >= dest.Ride.EndStep)
			{
				return (false, Constant.MIN);
			}


			//add bonus point
			int bonus = 0;
			if (arrival <= dest.Ride.StartStep)
			{
				bonus = _problem.Bonus;
			}

			int score = tripDistance + bonus;
			int waitingTime = add;

			return (true, score - (distance + 10 * waitingTime));

			// trajet + bonus - (temps perdu avant démarrage)
			return (true, tripDistance + bonus * Constant.BONUS_MULTIPLIER - (distance + add));
		}
	}

	public class Solution
	{
		private readonly Problem _problem;
		private List<Node> _rideNodes;
		public Solution(Problem problem)
		{
			_problem = problem;

			_problem.Rides.ForEach(x => x.Distance = Distance(x));
			Node startNode = CreateNode();
			_problem.Vehicles.ForEach(x => x.Node = startNode);

			int max = _problem.Rides.Sum(x =>
			{
				int distanceFromStart = x.StartX + x.StartY;

				if (distanceFromStart + x.Distance >= x.EndStep)
				{
					return 0;
				}

				if (x.Distance + x.StartStep >= x.EndStep)
				{
					return 0;
				}

				return x.Distance + _problem.Bonus;
			});
			Console.WriteLine($"Max score: {max}");
		}

		private Node CreateNode()
		{
			Node startNode = new Node
			{
				Ride = null,
			};

			_rideNodes = _problem.Rides.Select(ride => new Node
			{
				Done = false,
				Ride = ride,
			}).ToList();
			startNode.Links.AddRange(_rideNodes.Select(node => (DistanceStart(node.Ride), node)));

			for (int i = 0; i < _rideNodes.Count; i++)
			{
				Node source = _rideNodes[i];
				for (int j = 0; j < _rideNodes.Count; j++)
				{
					if (i == j)
					{
						continue;
					}

					Node dest = _rideNodes[j];

					source.Links.Add((Distance(source.Ride, dest.Ride), dest));
				}
			}

			return startNode;
		}

		public void Solve()
		{
			List<Vehicle> vehicles = _problem.Vehicles.ToArray().ToList();
			int[][] matrix = new int[vehicles.Count][];
			int[] vehiclesMapping = new int[vehicles.Count];
			int[] nodesMapping = new int[_problem.Rides.Count];
			int[] invertNodesMapping = new int[_problem.Rides.Count];
			for (int stepIndex = 0; stepIndex < _problem.StepCount; stepIndex++)
			{
				if (stepIndex % 10000 == 0)
				{
					Console.WriteLine($"Step {stepIndex}");
				}

				_problem.CurrentStep = stepIndex;

				int nodeCount = 0;
				for (int i = 0, ni = 0; i < _problem.Rides.Count; ++i)
				{
					Node n = _rideNodes[i];
					if (n.Done)
					{
						invertNodesMapping[i] = -1;
						continue;
					}

					int arrivalTime = _problem.CurrentStep + n.Ride.Distance;
					if (arrivalTime >= _problem.StepCount || arrivalTime >= n.Ride.EndStep)
					{
						invertNodesMapping[i] = -1;
						continue;
					}

					nodesMapping[ni] = i;
					invertNodesMapping[i] = ni;
					ni++;
					nodeCount++;
				}

				if (nodeCount == 0)
				{
					continue;
				}

				int vCount = 0;
				for (int i = 0, vi = 0; i < vehicles.Count; i++)
				{
					Vehicle v = vehicles[i];
					if (v.CurrentStep > stepIndex || v.Finished) //la voiture est en avance sur la simulation
					{
						continue;
					}

					int[] score = v.UpdateNodesScore(invertNodesMapping);

					bool anyGood = false;
					for (var j = 0; j < nodeCount; j++)
					{
						if (score[j] != Constant.MIN)
						{
							anyGood = true;
							break;
						}
					}

					if (!anyGood)
					{
						v.Finished = true;
						continue;
					}

					matrix[vi] = score;
					vehiclesMapping[vi] = i;
					vi++;
					vCount++;
				}

				if (vCount == 0)
				{
					continue;
				}

				int?[] result = AssignmentProblem.Solve(matrix, vCount, nodeCount);
				for (int i = 0; i < vCount; ++i)
				{
					if (result[i] == null)
					{
						continue;
					}
					int matrixRideIndex = result[i].Value;

					int rideId = nodesMapping[matrixRideIndex];
					Vehicle v = vehicles[vehiclesMapping[i]];

					Node rideNode = _rideNodes[rideId];
					if (rideNode.Done || matrix[i][matrixRideIndex] == Constant.MIN)
					{
						v.Finished = true;
						continue;
					}

					v.Rides.Add(rideId);

					int distanceToNode = Distance(v.Node.Ride, rideNode.Ride);
					v.CurrentStep += distanceToNode + rideNode.Ride.Distance;

					//add waiting time before ride
					int arrival = _problem.CurrentStep + distanceToNode;
					if (arrival < rideNode.Ride.StartStep)
					{
						v.CurrentStep += rideNode.Ride.StartStep - arrival;
					}

					//mark start & end node as "Done"
					v.Node = rideNode;
					rideNode.Done = true;
				}
			}
		}

		public int Score()
		{
			int score = 0;

			Dictionary<Vehicle, (int currentStep, int rideIndex, int x, int y)> vehicles = _problem.Vehicles.ToDictionary(x => x, _ => (0, 0, 0, 0));

			for (var step = 0; step < _problem.StepCount; step++)
			{
				foreach (Vehicle v in _problem.Vehicles)
				{
					(int currentStep, int rideIndex, int x, int y) = vehicles[v];
					if (step < currentStep || rideIndex >= v.Rides.Count)
					{
						continue;
					}

					int rideId = v.Rides[rideIndex];
					Ride ride = _problem.Rides[rideId];

					int distanceToRide = Distance(x, y, ride);
					int arrivalStep = step + distanceToRide;
					int endStep = step + distanceToRide + ride.Distance;

					if (arrivalStep <= ride.StartStep)
					{
						score += _problem.Bonus;
						endStep += (ride.StartStep - arrivalStep);
					}

					score += ride.Distance;
					
					vehicles[v] = (endStep, rideIndex + 1, ride.EndX, ride.EndY);
				}
			}

			return score;
		}

		public static int DistanceStart(Ride r)
		{
			return Math.Abs(r.StartX) + Math.Abs(r.StartY);
		}

		public static int Distance(Ride source, Ride dest)
		{
			int endX = source?.EndX ?? 0;
			int endY = source?.EndY ?? 0;

			return Math.Abs(endX - dest.StartX) + Math.Abs(endY - dest.StartY);
		}

		public static int Distance(int endX, int endY, Ride dest)
		{
			return Math.Abs(endX - dest.StartX) + Math.Abs(endY - dest.StartY);
		}

		public static int Distance(Ride r)
		{
			return Math.Abs(r.EndX - r.StartX) + Math.Abs(r.EndY - r.StartY);
		}
	}
}