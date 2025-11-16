using Godot;

namespace Game;

public partial class TroopSpawner : Node
{
    [Export] public PackedScene TroopScene { get; set; }
    [Export] public HexGrid HexGrid { get; set; }

    public int Mana { get; private set; } = 0;

    public bool TrySpawnTroop(TroopData data, Vector2I coords)
    {
        if (Mana < data.Cost) // TODO User.Mana
        {
            GD.Print("No hay mana suficiente");
            return false;
        }

        var cell = HexGrid.GetCell(coords);
        if (cell == null || cell.IsOccupied)
        {
            GD.Print("Casilla ocupada o no válida");
            return false;
        }

        Mana -= data.Cost; // TODO User.Mana

        Troop troop = new Troop(data, coords, HexGrid.Entities.Player); // TODO User.id

        Vector3 worldPos = new Vector3(0,0,0); // TODO
        troop.GlobalPosition = worldPos;

        AddChild(troop);

        cell.IsOccupied = true;

        return true;
    }

    public void TrySpawnScout(Vector2I coords)  => TrySpawnTroop(Troops.Scout, coords);
    public void TrySpawnWarrior(Vector2I coords)=> TrySpawnTroop(Troops.Warrior, coords);
    public void TrySpawnArcher(Vector2I coords) => TrySpawnTroop(Troops.Archer, coords);
}
