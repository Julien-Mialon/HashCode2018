using System;
using System.Collections.Generic;
using System.Linq;

namespace HashCode2018
{
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
		}

		private void ShowMaxScore()
		{
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

					//TODO: add only if possible 
					source.Links.Add((Distance(source.Ride, dest.Ride), dest));
				}
				
				source.InitializeClosest();
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
			//AssignmentProblem assignmentSolver = new AssignmentProblem(vehicles.Count, _problem.Rides.Count);
			
			for (int stepIndex = 0; stepIndex < _problem.StepCount; stepIndex++)
			{
				/*
				if (stepIndex % 100 == 0)
				{
					Console.WriteLine($"Step {stepIndex}");
				}
				*/

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

				int?[] result = AssignmentProblemStatic.Solve(matrix, vCount, nodeCount);
				bool hasChanged = false;
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

					hasChanged = true;
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

				if (hasChanged)
				{
					for (var i = 0; i < _rideNodes.Count; i++)
					{
						_rideNodes[i].UpdateClosest();
					}
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