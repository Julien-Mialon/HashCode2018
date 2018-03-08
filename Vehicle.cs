using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HashCode2018
{
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
			Buffer.BlockCopy(_problem.NotUsableScores, 0, _scores, 0, _problem.SizeToCopyScore);
			/*
			for (var i = 0; i < _scores.Length; i++)
			{
				_scores[i] = Constant.MIN;
			}
			*/
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

			int endTripTimeIfSelected = arrival + add + tripDistance;
			if (endTripTimeIfSelected >= dest.Ride.EndStep)
			{
				return (false, Constant.MIN);
			}


			//add bonus point
			int bonus = 0;
			bool hasBonus = false;
			if (arrival <= dest.Ride.StartStep)
			{
				bonus = _problem.Bonus;
			}

			int score = tripDistance + bonus;
			int waitingTime = add;
			
			//prendre en compte le temps perdu pour revenir
			int timeForNext = dest.ClosestDistance;
			int arrivalForNext = endTripTimeIfSelected + timeForNext;
			if (arrivalForNext < _problem.StepCount) 
			{
				score -= timeForNext;
			}
			else
			{
				score -= timeForNext - (_problem.StepCount - arrivalForNext);
			}
			
			
			
			return (true, score - (distance + waitingTime));

			// trajet + bonus - (temps perdu avant démarrage)
			return (true, tripDistance + bonus * Constant.BONUS_MULTIPLIER - (distance + add));
		}
	}
}