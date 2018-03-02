using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Decider.Csp.BaseTypes;
using Decider.Csp.Global;
using Decider.Csp.Integer;

namespace HashCode2018
{
    public class Program
    {
        private const string FILE = "a_example.in";

        private static readonly List<string> _files = new List<string>
        {
            "a_example.in",
            "b_should_be_easy.in",
            //"c_no_hurry.in",
            //"d_metropolis.in",
            //"e_high_bonus.in"
        };

        static void RunFile(string file)
        {
            Console.WriteLine($"Start {file}");

            Problem problem = Input(file);

            Solve(problem);

            Output(file, problem.Vehicles);

            Console.WriteLine("Done!");
        }

        static void Main(string[] args)
        {
            //_files.AsParallel().ForAll(RunFile);
            RunFile(FILE);
        }

        static int Score(Vehicle v, Ride r, int currentStep, int bonusPoint)
        {
            if (currentStep + r.Distance + v.DistanceToRide(r) >= r.EndStep)
            {
                return -100_000_000;
            }

            int bonus;
            int attente = 0;
            if (r.StartStep == currentStep)
            {
                bonus = bonusPoint;
            }
            else
            {
                attente = r.Attente(currentStep);
                bonus = attente < 0 ? bonusPoint : 0;
            }

            return r.Distance - v.DistanceToRide(r) + bonus - attente;
        }

        static void Solve(Problem p)
        {
            int[][] allScore;

            for (var currentStep = 0; currentStep < p.StepCount; currentStep++)
            {

                var vehicleArray = p.Vehicles.Where(x => x.IsDispo(currentStep)).ToArray();
                if (vehicleArray.Length == 0)
                    continue;
                var rideArray = p.Rides.Where(x => !x.Done).ToArray();
                if (rideArray.Length == 0)
                    break;
                var nbVehicleDispo = vehicleArray.Length;
                var nbOpenRide = rideArray.Length;

                allScore = new int[nbVehicleDispo][];

                for (var index = 0; index < nbVehicleDispo; index++)
                {
                    allScore[index] = new int[nbOpenRide];
                }

                vehicleArray.Select((v, i) => (v, i)).AsParallel().ForAll(
                     tuple =>
                     {
                         int vehicleIndex = tuple.Item2;
                         Vehicle vehicle = tuple.Item1;
                         int rideIndex = 0;
                         foreach (var ride in rideArray)
                         {
                             allScore[vehicleIndex][rideIndex] = Score(vehicle, ride, currentStep, p.Bonus);

                             rideIndex++;
                         }
                     });

                if (vehicleArray.Length == 1)
                {
                    vehicleArray[0].ToNextRide(rideArray[IndexOfMax(allScore[0])], currentStep);
                    continue;
                }

                if (rideArray.Length == 1)
                {
                    int index = 0;
                    int max = allScore[0][0];
                    for (var i = 1; i < vehicleArray.Length; i++)
                    {
                        if (allScore[i][0] > max)
                        {
                            max = allScore[i][0];
                            index = i;
                        }
                    }
                    vehicleArray[index].ToNextRide(rideArray[0], currentStep);
                    continue;
                }

                var affectation = new VariableInteger[nbVehicleDispo];
                ConstrainedArray[] constrainedData = new ConstrainedArray[nbVehicleDispo];
                for (var index = 0; index < nbVehicleDispo; index++)
                {
                    constrainedData[index] = new ConstrainedArray(allScore[index]);
                }

                var scoreArray = new VariableInteger[nbVehicleDispo];
                Enumerable.Range(0, nbVehicleDispo).AsParallel().Select(x => (x, new VariableInteger($"traj {x}", 0, nbOpenRide - 1), new VariableInteger($"sco {x}", 0, 60_000_00))).ForAll(tuple =>
                        {
                            affectation[tuple.Item1] = tuple.Item2;
                            scoreArray[tuple.Item1] = tuple.Item3;
                        });
                var score = new VariableInteger("score", 0, 60_000_000);
                var constraints = new List<IConstraint>
                {
                    new AllDifferentInteger(affectation),
                    new ConstraintInteger(score==ArraySum(scoreArray))
                };

                for (var index = 0; index < nbVehicleDispo; index++)
                {
                    var scoreA = scoreArray[index];
                    var constraintTab = constrainedData[index];
                    var aff = affectation[index];
                    var val = constraintTab[aff];
                    constraints.Add(new ConstraintInteger(scoreA == val));
                    //constraints.Add(new ConstraintInteger(scoreArray[index] == constrainedData[index][affectation[index]]));
                }

                IState<int> state = new StateInteger(affectation, constraints);
                StateOperationResult searchResult;
                var solution = default(IDictionary<string, IVariable<int>>);
                state.StartSearch(out searchResult, score, out solution, 60);

                for (var index = 0; index < vehicleArray.Length; index++)
                {
                    var vehicle = vehicleArray[index];
                    vehicle.ToNextRide(rideArray[affectation[index].Value], currentStep);
                }

                Console.WriteLine($"Step {currentStep} finished");
            }
        }

        static ExpressionInteger ArraySum(VariableInteger[] array)
        {
            ExpressionInteger e = 0;
            foreach (var variable in array)
            {
                e = e + variable;
            }

            return e;
        }

        static int IndexOfMax(int[] array)
        {
            int index = 0;
            int max = array[0];

            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] > max)
                {
                    index = i;
                    max = array[i];
                }
            }

            return index;
        }

        #region Input & Output

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
                result.Rides.Add(new Ride(counter++, values[0], values[1], values[2], values[3], values[4], values[5]));
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

        #endregion
    }

    //public class Node
    //	{
    //		public Node()
    //		{
    //			Links = new List<(int, Node)>();
    //		}
    //
    //		public bool Done { get; set; }
    //
    //		public Ride Ride { get; set; }
    //
    //		public bool IsStart { get; set; }
    //
    //		public List<(int, Node)> Links { get; set; }
    //	}
    //
    //	public class Solution
    //	{
    //		private readonly Problem _problem;
    //		private int _currentStep = 0;
    //
    //		public Solution(Problem problem)
    //		{
    //			_problem = problem;
    //		}
    //
    //		private Node CreateNode()
    //		{
    //			Node startNode = new Node
    //			{
    //				Ride = null,
    //			};
    //
    //			List<Node> endNodes = new List<Node>();
    //			startNode.Links.AddRange(_problem.Rides.Select(ride =>
    //			{
    //				Node startRide = new Node
    //				{
    //					Ride = ride,
    //					IsStart = true,
    //				};
    //
    //				startRide.Links.Add((ride.Distance, new Node
    //				{
    //					Ride = ride,
    //					IsStart = false,
    //				}));
    //				endNodes.Add(startRide.Links[0].Item2);
    //				return (DistanceStart(ride), startRide);
    //			}));
    //
    //			foreach (Node startTrip in startNode.Links.Select(x => x.Item2))
    //			{
    //				foreach (Node endTrip in endNodes)
    //				{
    //					if (startTrip.Ride != endTrip.Ride)
    //					{
    //						endTrip.Links.Add((Distance(endTrip.Ride, startTrip.Ride), startTrip));
    //					}
    //				}
    //			}
    //
    //			return startNode;
    //		}
    //
    //		public void Solve()
    //		{
    //			Node start = CreateNode();
    //			_problem.Vehicles.ForEach(x => x.Node = start);
    //
    //			List<Vehicle> vehicles = _problem.Vehicles.ToArray().ToList();
    //			for (int stepIndex = 0; stepIndex < _problem.StepCount; stepIndex++)
    //			{
    //				_currentStep = stepIndex;
    //				for (var j = 0; j < vehicles.Count; j++)
    //				{
    //					Vehicle v = vehicles[j];
    //
    //					if (v.CurrentStep > stepIndex)
    //					{
    //						continue;
    //					}
    //
    //					AffectVehicle(v);
    //
    //					if (v.Finished)
    //					{
    //						vehicles.RemoveAt(j);
    //						j--;
    //					}
    //				}
    //			}
    //		}
    //
    //		public void AffectVehicle(Vehicle v)
    //		{
    //			Node next = null;
    //			int distanceToNode = 0;
    //			int maxScore = -100_000_000;
    //
    //			Node start = v.Node;
    //			foreach ((int distance, Node node) in start.Links)
    //			{
    //				if (IsUsable(start, distance, node))
    //				{
    //					int result = Score(start, distance, node);
    //
    //					if (maxScore < result)
    //					{
    //						distanceToNode = distance;
    //						maxScore = result;
    //						next = node;
    //					}
    //				}
    //			}
    //
    //			if (next == null)
    //			{
    //				v.Finished = true;
    //			}
    //			else
    //			{
    //				v.Rides.Add(next.Ride.Id);
    //
    //				v.CurrentStep += distanceToNode + next.Ride.Distance;
    //
    //				//add waiting time before ride
    //				int arrival = _currentStep + distanceToNode;
    //				if (arrival < next.Ride.StartStep)
    //				{
    //					v.CurrentStep += next.Ride.StartStep - arrival;
    //				}
    //
    //				//mark start & end node as "Done"
    //				v.Node = next.Links[0].Item2;
    //				next.Done = true;
    //				v.Node.Done = true;
    //			}
    //		}
    //
    //		public bool IsUsable(Node x, int distance, Node y)
    //		{
    //			if (y.Done)
    //			{
    //				return false;
    //			}
    //
    //			if (y.IsStart)
    //			{
    //				int tripDistance = y.Ride.Distance;
    //				int arrival = _currentStep + distance;
    //
    //				//add time before beginning
    //				int add = 0;
    //				if (arrival < y.Ride.StartStep)
    //				{
    //					add = y.Ride.StartStep - arrival;
    //				}
    //
    //				if (arrival + add + tripDistance >= y.Ride.EndStep)
    //				{
    //					return false;
    //				}
    //			}
    //
    //			return true;
    //		}
    //
    //		public int Score(Node x, int distance, Node y)
    //		{
    //			int tripDistance = y.Ride.Distance;
    //			int arrival = _currentStep + distance;
    //			int add = 0;
    //			//add time before ride begin
    //			if (arrival < y.Ride.StartStep)
    //			{
    //				add = y.Ride.StartStep - arrival;
    //			}
    //
    //			//add bonus point
    //			int bonus = 0;
    //			if (arrival <= y.Ride.StartStep)
    //			{
    //				bonus = _problem.Bonus;
    //			}
    //
    //			// trajet + bonus - (temps avant démarrage)
    //			return tripDistance + bonus - (distance + add);
    //		}
}