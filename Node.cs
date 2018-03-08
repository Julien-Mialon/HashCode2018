using System.Collections.Generic;

namespace HashCode2018
{
	public class Node
	{
		public Node()
		{
			Links = new List<(int, Node)>();
		}

		public bool Done { get; set; }

		public Ride Ride { get; set; }

		public List<(int, Node)> Links { get; set; }

		public int ClosestDistance { get; private set; }

		private int _index = 0;
		private List<(int, Node)> _closest = null;

		public void InitializeClosest()
		{
			_closest = new List<(int, Node)>(Links);
			_closest.Sort(CompareNodesDistance);
		}

		private int CompareNodesDistance((int, Node) x, (int, Node) y)
		{
			return x.Item1.CompareTo(y.Item1);
		}

		public void UpdateClosest()
		{
			for (; _index < _closest.Count && _closest[_index].Item2.Done; _index++)
			{
			}

			if (_index < _closest.Count)
			{
				ClosestDistance = _closest[_index].Item1;
			}
			else
			{
				ClosestDistance = 0;
			}
			/*
			int closest = int.MaxValue;
			bool found = false;
			foreach ((int nextDistance, Node nextNode) in Links)
			{
				if (nextNode.Done) //add finished trip ?
				{
					continue;
				}

				if (nextDistance < closest)
				{
					found = true;
					closest = nextDistance;
				}
			}

			if (found)
			{
				ClosestDistance = closest;
			}
			else
			{
				ClosestDistance = 0;
			}
			*/
		}
	}
}