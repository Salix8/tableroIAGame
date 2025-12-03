using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game.State;

public class HexGrid(float cellOuterRadius)
{
	public float CellOuterRadius => cellOuterRadius;
	public static readonly Vector2I Up = new Vector2I(0,-1);
	public static readonly Vector2I UpRight = new Vector2I(1, -1);
	public static readonly Vector2I DownRight = new Vector2I(1, 0);
	public static readonly Vector2I Down = -Up;
	public static readonly Vector2I DownLeft = -UpRight;
	public static readonly Vector2I UpLeft = -DownRight;

	public static readonly IReadOnlyList<Vector2I> Directions =[
		DownLeft, Down, DownRight, UpRight, Up, UpLeft,
	];

	public static IList<Vector2I> GetNeighborCoords(Vector2I center)
	{
		return Directions.Select(dir => center + dir).ToList();
	}

	public static int GetHexDistance(Vector2I a, Vector2I b)
	{
		var ac = AxialToCube(a);
		var bc = AxialToCube(b);

		return (Mathf.Abs(ac.X - bc.X) +
		        Mathf.Abs(ac.Y - bc.Y) +
		        Mathf.Abs(ac.Z - bc.Z)) / 2;
	}


	public static IEnumerable<Vector2I> GetNeighbourSpiralCoords(Vector2I center, int radius, bool includeCenter = false)
	{
		if (includeCenter){
			yield return center;
		}
		Vector2I cur = center + DownLeft;
		for (int rad = 1; rad <= radius; rad++){
			for (int edge = 0; edge < 6; edge++){
				for (int step = 0; step < rad; step++){
					yield return cur;
					cur += RingDirections[edge];
				}
			}
			cur += DownLeft;
		}
	}

	static readonly IReadOnlyList<Vector2I> RingDirections =[
		DownRight, UpRight, Up, UpLeft, DownLeft, Down
	];

	public Vector2I WorldToHex(Vector2 worldCoords)
	{
		Vector2 localCoords = worldCoords / cellOuterRadius;
		float q = 0.6666667f * localCoords.X;
		float r = -0.33333334f * localCoords.X + 0.57735026f * localCoords.Y;
		Vector2 hexCoords = new(q,r);
		return CubeToAxial(CubeRound(AxialToCubeF(hexCoords)));
	}

	const float Sqrt3 = 1.73205080757f;
	public Vector2 HexToWorld(Vector2I hexCoords)
	{
		float x = 1.5f * hexCoords.X;
		float y = 0.8660254f * hexCoords.X + Sqrt3 * hexCoords.Y;
		Vector2 localCoords = new(x,y);
		Vector2 worldCoords = localCoords * cellOuterRadius;
		return worldCoords;
	}




	static Vector2I CubeToAxial(Vector3I cubeCoords) => new(cubeCoords.X, cubeCoords.Y);
	static Vector2 CubeToAxialF(Vector3 cubeCoords) => new(cubeCoords.X, cubeCoords.Y);

	static Vector3I AxialToCube(Vector2I axialCoords) =>
		new(axialCoords.X, axialCoords.Y, -axialCoords.X - axialCoords.Y);

	static Vector3 AxialToCubeF(Vector2 axialCoords) =>
		new(axialCoords.X, axialCoords.Y, -axialCoords.X - axialCoords.Y);

	static Vector3I CubeRound(Vector3 fracCube)
	{
		Vector3I rounded = new(
			Mathf.RoundToInt(fracCube.X),
			Mathf.RoundToInt(fracCube.Y),
			Mathf.RoundToInt(fracCube.Z)
		);
		Vector3 diff = new(
			Mathf.Abs(rounded.X - fracCube.X),
			Mathf.Abs(rounded.Y - fracCube.Y),
			Mathf.Abs(rounded.Z - fracCube.Z)
		);
		if (diff.X > diff.Y && diff.X > diff.Z){
			rounded.X = -rounded.Y - rounded.Z;
		}
		else if (diff.X > diff.Z){
			rounded.Y = -rounded.X - rounded.Z;
		}
		else{
			rounded.Z = -rounded.X - rounded.Y;
		}

		return rounded;
	}
}