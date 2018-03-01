using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HashCode2018
{
    class Program
    {
        private const string FILE = "small.in";
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Problem problem = Input(FILE);

            Output(FILE, problem.Vehicles);
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
}
