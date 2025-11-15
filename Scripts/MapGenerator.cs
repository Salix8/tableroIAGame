using Game;
using Godot;

public partial class MapGenerator : Node3D
{
	[Export] int MapRadius { get; set; } = 10;

	[Export] PackedScene hexTileScene { get; set; }
	
	[Export] HexGrid hexGrid { get; set; }

	// El tamaño (radio) de tu malla hexagonal 3D.
	// Ajústalo para que coincida con el tamaño de tu modelo.
	private float hexSize;
	private FastNoiseLite noise;

	public override void _Ready()
	{
		noise = new FastNoiseLite();
		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
		noise.Frequency = 0.05f;

		CalculateHexSize();

		GenerateMap();
	}

	private void CalculateHexSize()
	{
		if (hexTileScene == null)
		{
			GD.PrintErr("La escena 'HexTileScene' no está asignada. No se puede calcular el tamaño.");
			return;
		}

		Node3D tempHex = hexTileScene.Instantiate<Node3D>();
		MeshInstance3D meshInstance = tempHex.GetNode<MeshInstance3D>("grass");

		if (meshInstance == null)
		{
			GD.PrintErr("No se encontró un nodo llamado 'grass' en la escena HexTile.");
			tempHex.QueueFree();
			return;
		}

		Aabb aabb = meshInstance.GetAabb();
		hexSize = aabb.Size.X / 2.0f;

		// Liberamos la memoria de la instancia temporal
		tempHex.QueueFree();
	}

	public void GenerateMap()
	{
		if (hexTileScene == null)
		{
			GD.PrintErr("La escena 'hexTileScene' no está asignada en el MapGenerator.");
			return;
		}

		for (int q = -MapRadius; q <= MapRadius; q++)
		{
			for (int r = -MapRadius; r <= MapRadius; r++)
			{
				// Calculamos la tercera coordenada cúbica 's'
				int s = -q - r;

				// Esta condición asegura que estamos generando dentro de un radio hexagonal, no en un cuadrado, lo que evita que se generen teselas en las esquinas.
				if (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s) <= MapRadius * 2)
				{
					Node3D newHex = hexTileScene.Instantiate<Node3D>();
					MeshInstance3D meshInstance = newHex.GetNode<MeshInstance3D>("grass");
					Aabb aabb = meshInstance.GetAabb();
					hexSize = aabb.Size.X / 2.0f;

					float height = noise.GetNoise2D(q, r);
					// Convertir coordenadas del hexágono a posición 3D en el mundo fórmula estándar para convertir coordenadas axiales (q, r) a cartesianas (x, z)
					float xPos = hexSize * 3.0f / 2.0f * q;
					float zPos = hexSize * Mathf.Sqrt(3.0f) * (r + q / 2.0f);

					Vector3 hexPos = new Vector3(xPos, height, zPos);

					newHex.Position = hexPos;
					AddChild(newHex);

					Vector2I coords = new Vector2I(q, r);
					TerrainType terrain = DetermineTerrainFromHeight(height); // Función auxiliar
					
					HexCell newCell = new HexCell(coords, terrain);
					hexGrid.RegisterCell(newCell);
				}
			}
		}
	}
	
	private TerrainType DetermineTerrainFromHeight(float height)
	{
		if (height < -0.5f) return TerrainType.Water;
		if (height < 0.0f) return TerrainType.Plains;
		if (height < 0.5f) return TerrainType.Forest;
		return TerrainType.Mountain;
	}
}
