using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashCode2018
{
    public class AssignmentProblem
    {
		//matrix => [height, width]
	    public static int?[] Solve(int[][] matrix,int height, int width)
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
		    (int value, int root)[] margeV = new(int value, int root)[width];
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

			return new int?[] { choosen };
	    }
    }
}
