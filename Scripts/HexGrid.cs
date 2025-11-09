using Godot;

namespace Game;

public partial class HexGrid : Node2D
{
	[Export] float cellOuterRadius = 1;


	public float CellOuterRadius => cellOuterRadius;

	public Vector2I WorldToHex(Vector2 worldCoords)
	{
		Vector2 localCoords = worldCoords * GlobalTransform / cellOuterRadius;
		Vector2 hexCoords = localCoords * hexTransform;
		return CubeToAxial(CubeRound(AxialToCubeF(hexCoords)));
	}

	Transform2D hexTransform = new(new Vector2(1.5f, Mathf.Sqrt(3f) * 0.5f), new Vector2(0f, Mathf.Sqrt(3f)),
		Vector2.Zero);

	public Vector2 HexToWorld(Vector2I hexCoords)
	{
		Vector2 localCoords = hexTransform * hexCoords;
		Vector2 worldCoords = GlobalTransform * localCoords * cellOuterRadius;
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