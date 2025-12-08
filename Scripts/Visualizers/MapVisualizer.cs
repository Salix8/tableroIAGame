#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game.Visualizers;

[GlobalClass]
public partial class MapVisualizer : Node3D
{
	[Export] PlayableMatch game;
	[Export] HexGrid3D grid;
	[Export] PackedScene hexTileVisualizerScene;
	public override void _EnterTree()
	{
		game.State.TerrainState.CellAdded.Subscribe(SpawnCell);
		game.State.ManaPoolClaimed.Subscribe(ClaimVisualizer);
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

	async Task ClaimVisualizer((PlayerId, Vector2I) args)
	{
		(PlayerId owner, Vector2I coord) = args;
		visualizers.TryGetValue(coord, out HexTileVisualizer? visualizer);
		Debug.Assert(visualizer != null, "No visualizer found for claimed mana pool");
		game.Players.TryGetValue(owner, out PlayerInfo? info);
		Debug.Assert(info != null, "Claimed visualizer does not belong to a valid player");
		await visualizer.SetBaseColor(info.TerrainColor);
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
