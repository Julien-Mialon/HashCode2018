using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashCode2018
{
	public class AssignmentProblemStatic
	{
		//matrix => [height, width]
		public static int?[] Solve(int[][] matrix, int height, int width)
		{
			if (height == 1)
			{
				return SolveHeight(matrix, width);
			}

			if (width == 1)
			{
				return SolveWidth(matrix, height);
			}

			//U les opérateurs, V les tâches
			int[] U = Enumerable.Range(0, height).ToArray();
			int[] V = Enumerable.Range(0, width).ToArray();
			int?[] mu = Enumerable.Repeat<int?>(null, height).ToArray();
			int?[] mv = Enumerable.Repeat<int?>(null, width).ToArray();
			int[] lu = Enumerable.Range(0, height).Select(y => matrix[y].Max()).ToArray();
			int[] lv = Enumerable.Repeat(0, width).ToArray();

			bool[] au = Enumerable.Repeat(false, height).ToArray();
			int?[] av = Enumerable.Repeat<int?>(null, width).ToArray();
			(int value, int root)[] margeV = new (int value, int root)[width];
			foreach (int root in U)
			{
				for (var i = 0; i < au.Length; i++)
				{
					au[i] = false;
				}

				for (var i = 0; i < av.Length; i++)
				{
					av[i] = null;
					margeV[i] = (lu[root] + lv[i] - matrix[root][i], root);
				}

				au[root] = true;
				int? vResult = null;
				while (true)
				{
					int delta = int.MaxValue;
					int u = 0;
					int v = 0;
					bool found = false;

					foreach (int vIndex in V.Where(x => av[x] == null))
					{
						(int current, int uIndex) = margeV[vIndex];
						if (delta > current)
						{
							delta = current;
							u = uIndex;
							v = vIndex;
							found = true;
						}
					}

					if (!found)
					{
					}

					if (delta > 0) //arbre complet
					{
						foreach (int u0 in U)
						{
							if (au[u0])
							{
								lu[u0] -= delta;
							}
						}

						foreach (int v0 in V)
						{
							if (av[v0] != null)
							{
								lv[v0] += delta;
							}
							else
							{
								(int val, int arg) = margeV[v0];
								margeV[v0] = (val - delta, arg);
							}
						}
					}

					//assert lu[u] + lv[v] == matrix[u][v]
					av[v] = u;
					if (mv[v] == null)
					{
						vResult = v;
						break;
					}

					int u1 = mv[v].Value;
					//assert au[u1] == false
					au[u1] = true;
					foreach (int v1 in V)
					{
						if (av[v1] == null)
						{
							int alt = lu[u1] + lv[v1] - matrix[u1][v1];
							if (margeV[v1].value > alt)
							{
								margeV[v1] = (alt, u1);
							}
						}
					}
				}

				while (vResult != null)
				{
					int v = vResult.Value;
					int u = av[v].Value;
					int? prec = mu[u];

					mv[v] = u;
					mu[u] = v;

					vResult = prec;
				}
			}

			return mu;
		}

		private static int?[] SolveWidth(int[][] matrix, int height)
		{
			int?[] result = Enumerable.Repeat<int?>(null, height).ToArray();

			int choosen = 0;
			int score = int.MinValue;

			for (var i = 0; i < height; i++)
			{
				if (matrix[i][0] > score)
				{
					score = matrix[i][0];
					choosen = i;
				}
			}

			result[choosen] = 0;
			return result;
		}

		private static int?[] SolveHeight(int[][] matrix, int width)
		{
			int choosen = 0;
			int score = int.MinValue;

			for (var i = 0; i < width; i++)
			{
				if (matrix[0][i] > score)
				{
					score = matrix[0][i];
					choosen = i;
				}
			}

			return new int?[] {choosen};
		}
	}

	public class AssignmentProblem
	{
		private int[] init0;
		private int?[] init_null;
		private bool[] init_false;

		private int _init_U_Size;
		private int _init_V_Size;
		private int _init_mu_Size;
		private int _init_mv_Size;
		private int _init_lv_Size;
		private int _init_au_Size;
		private int _init_av_Size;

		private int[] U;
		private int[] V;
		private int?[] mu;
		private int?[] mv;
		private int[] lu;
		private int[] lv;
		private bool[] au;
		private int?[] av;

		public AssignmentProblem(int maxHeight, int maxWidth)
		{
			U = new int[maxHeight]; //init to 0
			V = new int[maxWidth]; //init to 0

			mu = new int?[maxHeight]; //init to null
			mv = new int?[maxWidth]; //init to null
			lu = new int[maxHeight]; //init nothing, must be calculated
			lv = new int[maxWidth]; //init to 0

			au = new bool[maxHeight]; //init to false
			av = new int?[maxWidth]; //init to null

			_init_U_Size = sizeof(int) * U.Length;
			_init_V_Size = sizeof(int) * V.Length;
			_init_mu_Size = mu.Length;
			_init_mv_Size = mv.Length;
			_init_lv_Size = sizeof(int) * lv.Length;
			_init_au_Size = sizeof(bool) * au.Length;
			_init_av_Size = av.Length;

			int initTableSize = Math.Max(maxHeight, maxWidth);
			init0 = new int[initTableSize];
			init_null = new int?[initTableSize];
			init_false = new bool[initTableSize];

			for (var i = 0; i < initTableSize; i++)
			{
				init0[i] = 0;
				init_null[i] = null;
				init_false[i] = false;
			}
		}

		private void Init()
		{
			Buffer.BlockCopy(init0, 0, U, 0, _init_U_Size);
			Buffer.BlockCopy(init0, 0, V, 0, _init_V_Size);

			Array.Copy(init_null, 0, mu, 0, _init_mu_Size);
			Array.Copy(init_null, 0, mv, 0, _init_mv_Size);

			Buffer.BlockCopy(init0, 0, lv, 0, _init_lv_Size);

			Buffer.BlockCopy(init_false, 0, au, 0, _init_au_Size);
			Array.Copy(init_null, 0, av, 0, _init_av_Size);
		}

		//matrix => [height, width]
		public int?[] Solve(int[][] matrix, int height, int width)
		{
			if (height == 1)
			{
				return SolveHeight(matrix, width);
			}

			if (width == 1)
			{
				return SolveWidth(matrix, height);
			}

			/*
		    for (int y = 0; y < height; ++y)
		    {
			    for (int x = 0; x < width; ++x)
			    {
				    matrix[y][x] += -Constant.MIN;
			    }
		    }
			*/
			/*
			//U les opérateurs, V les tâches
			int[] U = Enumerable.Range(0, height).ToArray();
			int[] V = Enumerable.Range(0, width).ToArray();
			int?[] mu = Enumerable.Repeat<int?>(null, height).ToArray();
			int?[] mv = Enumerable.Repeat<int?>(null, width).ToArray();
			int[] lu = Enumerable.Range(0, height).Select(y => matrix[y].Max()).ToArray();
			int[] lv = Enumerable.Repeat(0, width).ToArray();

			bool[] au = Enumerable.Repeat(false, height).ToArray();
			int?[] av = Enumerable.Repeat<int?>(null, width).ToArray();
			*/
			for (var i = 0; i < height; i++)
			{
				lu[i] = matrix[i].Max();
			}

			Init();

			(int value, int root)[] margeV = new (int value, int root)[width];
			for (int uForeachIndex = 0; uForeachIndex < height; uForeachIndex++)
			{
				int root = U[uForeachIndex];
				Buffer.BlockCopy(init_false, 0, au, 0, _init_au_Size);
				Array.Copy(init_null, 0, av, 0, _init_av_Size);
				/*
				for (var i = 0; i < au.Length; i++)
				{
					au[i] = false;
				}
				*/

				for (var i = 0; i < width; i++)
				{
					margeV[i] = (lu[root] + lv[i] - matrix[root][i], root);
				}

				au[root] = true;
				int? vResult = null;
				while (true)
				{
					int delta = int.MaxValue;
					int u = 0;
					int v = 0;
					bool found = false;

					for (var vForeachIndex = 0; vForeachIndex < width; vForeachIndex++)
					{
						int vIndex = V[vForeachIndex];

						if (av[vIndex] != null)
						{
							continue;
						}

						(int current, int uIndex) = margeV[vIndex];
						if (delta > current)
						{
							delta = current;
							u = uIndex;
							v = vIndex;
							found = true;
						}
					}

					if (!found)
					{
					}

					if (delta > 0) //arbre complet
					{
						for (int uForeachIndex2 = 0; uForeachIndex2 < height; uForeachIndex2++)
						{
							int u0 = U[uForeachIndex2];
							if (au[u0])
							{
								lu[u0] -= delta;
							}
						}

						for (var vForeachIndex = 0; vForeachIndex < width; vForeachIndex++)
						{
							int v0 = V[vForeachIndex];
							if (av[v0] != null)
							{
								lv[v0] += delta;
							}
							else
							{
								(int val, int arg) = margeV[v0];
								margeV[v0] = (val - delta, arg);
							}
						}
					}

					//assert lu[u] + lv[v] == matrix[u][v]
					av[v] = u;
					if (mv[v] == null)
					{
						vResult = v;
						break;
					}

					int u1 = mv[v].Value;
					//assert au[u1] == false
					au[u1] = true;
					for (var vForeachIndex2 = 0; vForeachIndex2 < width; vForeachIndex2++)
					{
						int v1 = V[vForeachIndex2];
						if (av[v1] == null)
						{
							int alt = lu[u1] + lv[v1] - matrix[u1][v1];
							if (margeV[v1].value > alt)
							{
								margeV[v1] = (alt, u1);
							}
						}
					}
				}

				while (vResult != null)
				{
					int v = vResult.Value;
					int u = av[v].Value;
					int? prec = mu[u];

					mv[v] = u;
					mu[u] = v;

					vResult = prec;
				}
			}

			return mu;
		}

		private static int?[] SolveWidth(int[][] matrix, int height)
		{
			int?[] result = Enumerable.Repeat<int?>(null, height).ToArray();

			int choosen = 0;
			int score = int.MinValue;

			for (var i = 0; i < height; i++)
			{
				if (matrix[i][0] > score)
				{
					score = matrix[i][0];
					choosen = i;
				}
			}

			result[choosen] = 0;
			return result;
		}

		private static int?[] SolveHeight(int[][] matrix, int width)
		{
			int choosen = 0;
			int score = int.MinValue;

			for (var i = 0; i < width; i++)
			{
				if (matrix[0][i] > score)
				{
					score = matrix[0][i];
					choosen = i;
				}
			}

			return new int?[] {choosen};
		}
	}
}