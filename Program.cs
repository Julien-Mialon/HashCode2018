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
	 */
	
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
			//RunFile(FILE);
			
			int[,] costMatrix = new int[,]
			{
				{ 2, 3, 3, 1},
				{ 3, 3, 2, 4 },
				{ 3, 2, 3, 4 }
			};
			
			Stopwatch watch = Stopwatch.StartNew();
			HungarianAlgorithm algo = new HungarianAlgorithm(costMatrix);
			int[] result = algo.Run();
			
			Console.WriteLine($"{watch.ElapsedMilliseconds}ms");
			Console.WriteLine(string.Join(" ; ", result));
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
			NotUsableScores = Rides.Select(_ => Constant.BONUS_MULTIPLIER).ToArray();
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
		private readonly HashSet<int> _usableRides;
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
			_usableRides = new HashSet<int>(problem.Rides.Select(x => x.Id));
		}

		public int[] UpdateNodesScore()
		{
			for (var i = 0; i < _scores.Length; i++)
			{
				_scores[i] = Constant.MIN;
			}

			for (int index = 0; index < Node.Links.Count; index++)
			{
				(int distance, Node node) = Node.Links[index];
				int id = node.Ride.Id;

				if (_usableRides.Contains(id))
				{
					if (node.Done)
					{
						_usableRides.Remove(id);
						continue;
					}
					
					(bool result, int score) = GetScoreForNode(distance, node);

					if (!result)
					{
						_usableRides.Remove(id);
					}
					
					_scores[node.Ride.Id] = score;
				}
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
			
			// trajet + bonus - (temps perdu avant démarrage)
			return (true, tripDistance + bonus*Constant.BONUS_MULTIPLIER - (distance + add));
		}
	}

	public class Solution
	{
		private readonly Problem _problem;

		public Solution(Problem problem)
		{
			_problem = problem;

			_problem.Rides.ForEach(x => x.Distance = Distance(x));
			Node startNode = CreateNode();
			_problem.Vehicles.ForEach(x => x.Node = startNode);
		}

		private Node CreateNode()
		{
			Node startNode = new Node
			{
				Ride = null,
			};

			List<Node> rideNodes = _problem.Rides.Select(ride => new Node
			{
				Done = false,
				Ride = ride,
			}).ToList();
			startNode.Links.AddRange(rideNodes.Select(node => (DistanceStart(node.Ride), node)));

			for (int i = 0; i < rideNodes.Count; i++)
			{
				Node source = rideNodes[i];
				for (int j = 0; j < rideNodes.Count; j++)
				{
					if (i == j)
					{
						continue;
					}

					Node dest = rideNodes[j];

					source.Links.Add((Distance(source.Ride, dest.Ride), dest));
				}
			}

			return startNode;
		}

		public void Solve()
		{
			List<Vehicle> vehicles = _problem.Vehicles.ToArray().ToList();
			int[][] matrix = new int[vehicles.Count][];
			for (int stepIndex = 0; stepIndex < _problem.StepCount; stepIndex++)
			{
				_problem.CurrentStep = stepIndex;
				
				for (int i = 0; i < vehicles.Count; i++)
				{
					Vehicle v = vehicles[i];
					if (v.CurrentStep > stepIndex) //la voiture est en avance sur la simulation
					{
						matrix[i] = _problem.NotUsableScores;
					}
					else
					{
						matrix[i] = v.UpdateNodesScore();
					}
				}
				/*
				_solver.SetWeights(matrix);
				List<int> bests = _solver.Path();

				for (int i = 0; i < bests.Count; i++)
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
				*/
			}
		}

		public static int DistanceStart(Ride r)
		{
			return Math.Abs(r.StartX) + Math.Abs(r.StartY);
		}

		public static int Distance(Ride source, Ride dest)
		{
			return Math.Abs(source.EndX - dest.StartX) + Math.Abs(source.EndY - dest.StartY);
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

		public bool IsUsable => Weight == -100_000_000;

		public int Weight { get; set; }

		public int Cost { get; set; }

		public bool IsOpened { get; set; }

		public SolverNode Backtrack { get; set; }

		public List<SolverNode> Links { get; set; }
	}

	public class HungarianSolver
	{
		private readonly int _vehiclesCount;
		private readonly int _ridesCount;
		private long[,] _matrix;
		private bool[,] _covering;

		public HungarianSolver(int vehiclesCount, int ridesCount)
		{
			_vehiclesCount = vehiclesCount;
			_ridesCount = ridesCount;
			_matrix = new long[vehiclesCount,ridesCount];
			_covering = new bool[vehiclesCount,ridesCount];
		}

		public void Set(int[][] weights)
		{
			for (int vi = 0; vi < _vehiclesCount; vi++)
			{
				for (int ri = 0; ri < _ridesCount; ri++)
				{
					_covering[vi, ri] = false;
					_matrix[vi, ri] = int.MaxValue - (weights[vi][ri] + Constant.MIN);
				}
			}
		}

		public void Solve()
		{
			//Step 1 => foreach rows, substract the min element to each element
			for (int vi = 0; vi < _vehiclesCount; vi++)
			{
				long min = long.MaxValue;
				for (int ri = 0; ri < _ridesCount; ri++)
				{
					if (_matrix[vi, ri] < min)
					{
						min = _matrix[vi, ri];
					}
				}
				
				for (int ri = 0; ri < _ridesCount; ri++)
				{
					_matrix[vi, ri] -= min;
				}
			}
		}
	}
	
	public sealed class HungarianAlgorithm
    {
        private readonly int[,] _costMatrix;
        private int _inf;
        private int _n; //number of elements
        private int[] _lx; //labels for workers
        private int[] _ly; //labels for jobs 
        private bool[] _s;
        private bool[] _t;
        private int[] _matchX; //vertex matched with x
        private int[] _matchY; //vertex matched with y
        private int _maxMatch;
        private int[] _slack;
        private int[] _slackx;
        private int[] _prev; //memorizing paths

        /// <summary>
        /// 
        /// </summary>
        /// <param name="costMatrix"></param>
        public HungarianAlgorithm(int[,] costMatrix)
        {
            _costMatrix = costMatrix;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int[] Run()
        {
            _n = _costMatrix.GetLength(0);

            _lx = new int[_n];
            _ly = new int[_n];
            _s = new bool[_n];
            _t = new bool[_n];
            _matchX = new int[_n];
            _matchY = new int[_n];
            _slack = new int[_n];
            _slackx = new int[_n];
            _prev = new int[_n];
            _inf = int.MaxValue;


            InitMatches();

            if (_n != _costMatrix.GetLength(1))
                return null;

            InitLbls();

            _maxMatch = 0;

            InitialMatching();

            var q = new Queue<int>();

            #region augment

            while (_maxMatch != _n)
            {
                q.Clear();

                InitSt();
                //Array.Clear(S,0,n);
                //Array.Clear(T, 0, n);


                //parameters for keeping the position of root node and two other nodes
                var root = 0;
                int x;
                var y = 0;

                //find root of the tree
                for (x = 0; x < _n; x++)
                {
                    if (_matchX[x] != -1) continue;
                    q.Enqueue(x);
                    root = x;
                    _prev[x] = -2;

                    _s[x] = true;
                    break;
                }

                //init slack
                for (var i = 0; i < _n; i++)
                {
                    _slack[i] = _costMatrix[root, i] - _lx[root] - _ly[i];
                    _slackx[i] = root;
                }

                //finding augmenting path
                while (true)
                {
                    while (q.Count != 0)
                    {
                        x = q.Dequeue();
                        var lxx = _lx[x];
                        for (y = 0; y < _n; y++)
                        {
                            if (_costMatrix[x, y] != lxx + _ly[y] || _t[y]) continue;
                            if (_matchY[y] == -1) break; //augmenting path found!
                            _t[y] = true;
                            q.Enqueue(_matchY[y]);

                            AddToTree(_matchY[y], x);
                        }
                        if (y < _n) break; //augmenting path found!
                    }
                    if (y < _n) break; //augmenting path found!
                    UpdateLabels(); //augmenting path not found, update labels

                    for (y = 0; y < _n; y++)
                    {
                        //in this cycle we add edges that were added to the equality graph as a
                        //result of improving the labeling, we add edge (slackx[y], y) to the tree if
                        //and only if !T[y] &&  slack[y] == 0, also with this edge we add another one
                        //(y, yx[y]) or augment the matching, if y was exposed

                        if (_t[y] || _slack[y] != 0) continue;
                        if (_matchY[y] == -1) //found exposed vertex-augmenting path exists
                        {
                            x = _slackx[y];
                            break;
                        }
                        _t[y] = true;
                        if (_s[_matchY[y]]) continue;
                        q.Enqueue(_matchY[y]);
                        AddToTree(_matchY[y], _slackx[y]);
                    }
                    if (y < _n) break;
                }

                _maxMatch++;

                //inverse edges along the augmenting path
                int ty;
                for (int cx = x, cy = y; cx != -2; cx = _prev[cx], cy = ty)
                {
                    ty = _matchX[cx];
                    _matchY[cy] = cx;
                    _matchX[cx] = cy;
                }
            }

            #endregion

            return _matchX;
        }

        private void InitMatches()
        {
            for (var i = 0; i < _n; i++)
            {
                _matchX[i] = -1;
                _matchY[i] = -1;
            }
        }

        private void InitSt()
        {
            for (var i = 0; i < _n; i++)
            {
                _s[i] = false;
                _t[i] = false;
            }
        }

        private void InitLbls()
        {
            for (var i = 0; i < _n; i++)
            {
                var minRow = _costMatrix[i, 0];
                for (var j = 0; j < _n; j++)
                {
                    if (_costMatrix[i, j] < minRow) minRow = _costMatrix[i, j];
                    if (minRow == 0) break;
                }
                _lx[i] = minRow;
            }
            for (var j = 0; j < _n; j++)
            {
                var minColumn = _costMatrix[0, j] - _lx[0];
                for (var i = 0; i < _n; i++)
                {
                    if (_costMatrix[i, j] - _lx[i] < minColumn) minColumn = _costMatrix[i, j] - _lx[i];
                    if (minColumn == 0) break;
                }
                _ly[j] = minColumn;
            }
        }

        private void UpdateLabels()
        {
            var delta = _inf;
            for (var i = 0; i < _n; i++)
                if (!_t[i])
                    if(delta>_slack[i])
                        delta = _slack[i];
            for (var i = 0; i < _n; i++)
            {
                if (_s[i])
                    _lx[i] = _lx[i] + delta;
                if (_t[i])
                    _ly[i] = _ly[i] - delta;
                else _slack[i] = _slack[i] - delta;
            }
        }

        private void AddToTree(int x, int prevx)
        {
            //x-current vertex, prevx-vertex from x before x in the alternating path,
            //so we are adding edges (prevx, matchX[x]), (matchX[x],x)

            _s[x] = true; //adding x to S
            _prev[x] = prevx;

            var lxx = _lx[x];
            //updateing slack
            for (var y = 0; y < _n; y++)
            {
                if (_costMatrix[x, y] - lxx - _ly[y] >= _slack[y]) continue;
                _slack[y] = _costMatrix[x, y] - lxx - _ly[y];
                _slackx[y] = x;
            }
        }

        private void InitialMatching()
        {
            for (var x = 0; x < _n; x++)
            {
                for (var y = 0; y < _n; y++)
                {
                    if (_costMatrix[x, y] != _lx[x] + _ly[y] || _matchY[y] != -1) continue;
                    _matchX[x] = y;
                    _matchY[y] = x;
                    _maxMatch++;
                    break;
                }
            }
        }
    }
}