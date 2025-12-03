#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

	HexTileVisualizer GetOrSpawnVisualizer(Vector2I coord)
	{
		if (visualizers.TryGetValue(coord, out HexTileVisualizer? visualizer)){
			return visualizer;
		}

		HexTileVisualizer newViz = hexTileVisualizerScene.InstantiateUnderAs<HexTileVisualizer>(this);
		newViz.HexCoord = coord; // Add this line

		Vector3 position = grid.HexToWorld(coord);
		newViz.GlobalPosition = position;
		visualizers.Add(coord, newViz);
		return newViz;
	}

	public bool TryGetVisualizer(Vector2I coord, [NotNullWhen(true)] out HexTileVisualizer? visualizer)
	{
		if (visualizers.TryGetValue(coord, out HexTileVisualizer? vis)){
			visualizer = vis;
			return true;
		}

		visualizer = null;
		return false;

	}

	async Task SpawnCell((Vector2I, TerrainState.TerrainType) args)
	{
		(Vector2I coord, TerrainState.TerrainType terrainType) = args;
		HexTileVisualizer viz = GetOrSpawnVisualizer(coord);
		await viz.SpawnTerrain(terrainType,grid.InnerRadius*2);
	}
}
