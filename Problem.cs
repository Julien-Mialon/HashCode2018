using System;
using System.Collections.Generic;

namespace HashCode2018
{
    public class Problem
    {
        public int Bonus { get; set; }

        public int StepCount { get; set; }

        public List<Ride> Rides { get; set; }

        public List<Vehicle> Vehicles { get; set; }

        public Problem()
        {
            Rides = new List<Ride>();
            Vehicles = new List<Vehicle>();
        }
    }

    public class Ride
    {
        public readonly int StartX, StartY;

        public readonly int EndX, EndY;

        public readonly int StartStep, EndStep, Id, Distance;

        public bool Done = false;

        public Ride(int id, int startX, int startY, int endX, int endY, int startStep, int endStep)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            StartStep = startStep;
            EndStep = endStep;
            Id = id;
            Distance = Math.Abs(StartX - EndY) + Math.Abs(StartY - EndY);
        }

        public int DistanceToRide(Ride r) => Math.Abs(EndX - r.StartX) + Math.Abs(EndY - r.StartY);

        public int Attente(int currentStep) => Math.Max(StartStep - currentStep, 0);
    }

    public class Vehicle
    {
        public int NextPlayableStep { get; set; }

        public int CurrentX, CurrentY;

        public List<int> Rides { get; }

        public Vehicle()
        {
            Rides = new List<int>();
        }

        public bool IsDispo(int currentStep) => currentStep >= NextPlayableStep;

        public int DistanceToRide(Ride r) => Math.Abs(CurrentX - r.StartX) + Math.Abs(CurrentY - r.StartY);

        public void ToNextRide(Ride r, int currentStep)
        {
            var attente = r.Attente(currentStep);
            r.Done = true;
            NextPlayableStep = currentStep + r.Distance + DistanceToRide(r) + attente;
            Rides.Add(r.Id);
            CurrentX = r.EndX;
            CurrentY = r.EndY;
        }
    }
}