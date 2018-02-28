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

            Input(FILE);

            int data = 2;
            Output(FILE, data);
        }

        static void Input(string file)
        {
            const int HEADER_COUNT = 1;
            string[] lines = ReadFile(file);

            //TODO: read header
            
            foreach (string line in lines.Skip(HEADER_COUNT))
            {
                //TODO: read content
            }
        }

        static void Output(string file, int data)
        {
            List<string> lines = new List<string>();
            
            //TODO: add header lines
            
            //TODO: write response lines 
            lines.AddRange(Enumerable.Range(0, data).Select(x => x.ToString()));

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
