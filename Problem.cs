using System.Collections.Generic;
using System.Linq;

namespace HashCode2018
{
	public class Problem
	{
		public int Bonus { get; }

		public int StepCount { get; }

		public List<Ride> Rides { get; }

		public List<Vehicle> Vehicles { get; set; }

		public int CurrentStep { get; set; }

		public int[] NotUsableScores { get; }

		public int SizeToCopyScore { get; }
		
		public Problem(int bonus, int stepCount, List<Ride> rides)
		{
			Bonus = bonus;
			StepCount = stepCount;
			Rides = rides;
			NotUsableScores = Rides.Select(_ => Constant.MIN).ToArray();
			SizeToCopyScore = sizeof(int) * NotUsableScores.Length;
		}
	}
}