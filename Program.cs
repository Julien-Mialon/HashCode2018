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

			Console.WriteLine($"Done! {file}");
		}

		static void Main(string[] args)
		{
			//_files.AsParallel().ForAll(RunFile);
			RunFile(FILE);
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
		private AffectSolver _solver;
		private Dictionary<int, Node> _startNodesRides;

		public Solution(Problem problem)
		{
			_problem = problem;

			_problem.Rides.ForEach(x => x.Distance = Distance(x));
			_solver = new AffectSolver(problem);
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

			_startNodesRides = startNode.Links.ToDictionary(x => x.Item2.Ride.Id, x => x.Item2);

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
				List<Vehicle> vs = vehicles.Where(v => v.CurrentStep <= stepIndex).ToList();
				List<Dictionary<int, int>> matrix = vs.Select(GetMatrix).ToList();

				_solver.SetWeights(matrix);
				List<int> bests = _solver.Path();
				
				for (var i = 0; i < bests.Count; i++)
				{
					int rideId = bests[i];
					Vehicle v = vs[i];

					if (rideId < 0)
					{
						v.Finished = true;
						continue;
					}
					

					Node startNode = _startNodesRides[rideId];
					Node endNode = start.Links[0].Item2;
					
					v.Rides.Add(rideId);

					int distanceToNode = matrix[i][rideId];
					v.CurrentStep += distanceToNode + startNode.Ride.Distance;

					//add waiting time before ride
					int arrival = _currentStep + distanceToNode;
					if (arrival < startNode.Ride.StartStep)
					{
						v.CurrentStep += startNode.Ride.StartStep - arrival;
					}

					//mark start & end node as "Done"
					v.Node = endNode;
					endNode.Done = true;
					startNode.Done = true;
					
				}
				
				for (var j = 0; j < vehicles.Count; j++)
				{
					Vehicle v = vehicles[j];

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
		
		public Dictionary<int, int> GetMatrix(Vehicle v)
		{
			Dictionary<int, int> result = new Dictionary<int, int>();
			Node start = v.Node;

			foreach ((int distance, Node node) in start.Links)
			{
				result[node.Ride.Id] = -100_000_000;
				if (IsUsable(start, distance, node))
				{
					int score = Score(start, distance, node);
					result[node.Ride.Id] = score;
				}
			}

			return result;
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

	public class SolverNode
	{
		public SolverNode()
		{
			Links = new List<SolverNode>();
		}

		public Ride Ride;

		public int Weight { get; set; }
		
		public int Cost { get; set; }
		
		public bool IsOpened { get; set; }
		
		public SolverNode Backtrack { get; set; }

		public List<SolverNode> Links { get; set; }
	}

	public class AffectSolver
	{
		private SolverNode _startNode;
		private SolverNode _endNode;

		private List<Dictionary<int, SolverNode>> _nodes;

		public AffectSolver(Problem problem)
		{
			_startNode = new SolverNode();
			_endNode = new SolverNode();

			_nodes = problem.Vehicles.Select(_ =>
			{
				return problem.Rides.ToDictionary(x => x.Id, x => new SolverNode
				{
					Ride = x,
					Weight = -100_000_000,
				});
			}).ToList();

			_startNode.Links.AddRange(_nodes[0].Values);
			
			for (var i = 0; i < _nodes.Count; i++)
			{
				var items = _nodes[i];
				if (i + 1 < _nodes.Count)
				{
					foreach (SolverNode item in items.Values)
					{
						item.Links.AddRange(_nodes[i + 1].Values);
					}
				}
				else
				{
					foreach (SolverNode item in items.Values)
					{
						item.Links.Add(_endNode);
					}
				}
			}
		}
		
		public void SetWeights(List<Dictionary<int, int>> weights)
		{
			for (var i = 0; i < _nodes.Count; i++)
			{
				var dest = _nodes[i];
				var source = weights[i];
				
				foreach (KeyValuePair<int,int> bouh in source)
				{
					dest[bouh.Key].Weight = bouh.Value;
				}
			}
		}

		public List<int> Path()
		{
			List<SolverNode> opened = new List<SolverNode>() { _startNode };
			HashSet<SolverNode> closed = new HashSet<SolverNode>();
			_startNode.IsOpened = true;
			List<int> result = new List<int>();
			while (opened.Count > 0)
			{
				SolverNode current = opened[0];
				opened.RemoveAt(0);

				if (current == _endNode)
				{
					while (current != null)
					{
						current = current.Backtrack;
						if (current != _startNode && current != null)
						{
							result.Insert(0, current.Weight == -100_000_000 ? -1 : current.Ride.Id);
						}
					}

					return result;
				}
				
				foreach (SolverNode currentLink in current.Links)
				{
					if (closed.Contains(currentLink))
					{
						continue;
					}

					if (currentLink.IsOpened)
					{
						if (current.Cost + currentLink.Weight < currentLink.Cost)
						{
							continue;
						}
					}

					currentLink.Backtrack = current;
					currentLink.Cost = current.Cost + currentLink.Weight;

					bool added = false;
					for (int i = 0; i < opened.Count; ++i)
					{
						if (opened[i].Cost < currentLink.Cost)
						{
							opened.Insert(i, currentLink);
							added = true;
							break;
						}
					}

					if (!added)
					{
						opened.Add(currentLink);
					}

					currentLink.IsOpened = true;
				}

				closed.Add(current);
			}

			return result;
		}
	}
}