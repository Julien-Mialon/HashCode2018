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

	public static class Program
	{
		private const string FILE = "a_example.in";

		private static List<string> _files = new List<string>
		{
			//"a_example.in",
			"b_should_be_easy.in",
			//"c_no_hurry.in",
			//"d_metropolis.in",
			//"e_high_bonus.in"
		};

		static void RunFile(string file)
		{
			Console.WriteLine($"Start {file}");
			Stopwatch watcher = Stopwatch.StartNew();

			Problem problem = Input(file);
			watcher.Write($"Input done {file}");

			Solution solution = new Solution(problem);
			watcher.Write($"Initialized {file}");
			
			solution.Solve();
			watcher.Write($"Solved {file}");

			int score = solution.Score();
			watcher.Write($"Score done {file}");
			
			Output($"{file}.{score}", problem.Vehicles);
			watcher.Write($"Done! {file} => {score}");
			
			watcher.Stop();
		}

		static void Main(string[] args)
		{
			//_files.AsParallel().ForAll(RunFile);
			RunFile(_files[0]);
			/*
			RunFile(_files[1]);
			RunFile(_files[2]);
			RunFile(_files[3]);
			*/
			
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

		static void Write(this Stopwatch watcher, string message)
		{
			Console.WriteLine($"{message} ({watcher.ElapsedMilliseconds}ms)");
			watcher.Restart();
		}
	}
}