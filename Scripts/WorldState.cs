using Godot;

namespace Game;

public partial class WorldState : Node
{
	[Export] GridState gridState;

	public static WorldState Instance { get; private set; }
	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
		}
	}
	public GridState GridState => gridState;
	[Export] Player[] players;
	public Player GetPlayer(int index) => players[index];
	public int PlayerCount => players.Length;
	public Vector2I[] GetVisibleCoords(int playerIndex)
	{
		throw new System.NotImplementedException();
	}

}