using System.Collections.Generic;
using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game.Visualizers;

[GlobalClass]
public partial class MapVisualizer : Node3D
{
	[Export] PlayableWorldState state;
	[Export] HexGrid3D grid;
	[Export] PackedScene hexTileVisualizerScene;
	public override void _EnterTree()
	{
		state.State.TerrainState.CellAdded.Subscribe(SpawnCell);
	}

	Dictionary<Vector2I, HexTileVisualizer> visualizers = new();

	HexTileVisualizer GetVisualizer(Vector2I coord)
	{
		if (visualizers.TryGetValue(coord, out HexTileVisualizer visualizer)){
			return visualizer;
		}

		HexTileVisualizer newViz = hexTileVisualizerScene.InstantiateUnderAs<HexTileVisualizer>(this);

		Vector3 position = grid.HexToWorld(coord);
		newViz.GlobalPosition = position;
		visualizers.Add(coord, newViz);
		return newViz;
	}

	async Task SpawnCell((Vector2I, TerrainState.TerrainType) args)
	{
		(Vector2I coord, TerrainState.TerrainType terrainType) = args;
		HexTileVisualizer viz = GetVisualizer(coord);
		await viz.SpawnTerrain(terrainType,grid.InnerRadius*2);
	}
}