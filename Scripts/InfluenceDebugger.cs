using Godot;
using Game;
using Game.AI;
using System.Collections.Generic;

public partial class InfluenceDebugger : Node3D
{
    [Export] public InfluenceMapManager MapManager;
    [Export] public HexGrid3D Grid { get; set; }
    [Export] public bool ShowTerritory = true;
    // [Export] public bool ShowThreat = true;

    private readonly Dictionary<Vector2I, Label3D> territoryLabels = new();
    private readonly Dictionary<Vector2I, Label3D> _threatLabels = new();

    public override void _Process(double delta)
    {
        if (MapManager == null || Grid == null) return;
        UpdateVisuals();
    }

    private void DrawDebug(InfluenceMap mapToVisualize, Gradient grad, Dictionary<Vector2I, Label3D> labels, float yOffset = 0f)
    {

        var normalized = mapToVisualize.Clone();
        normalized.Normalize();
        foreach (var label in labels.Values)
        {
            label.Visible = false;
        }

        foreach (var kvp in mapToVisualize.Map)
        {
            Vector2I coords = kvp.Key;
            float value = kvp.Value;

            // if (Mathf.Abs(value) <= 0.0001f) continue;

            if (!labels.ContainsKey(coords))
            {
                var label = new Label3D();
                AddChild(label);

                label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
                label.FontSize = 64;
                label.GlobalPosition = Grid.HexToWorld(coords) + (Vector3.Up * 2.0f) + (Vector3.Up * yOffset);

                labels[coords] = label;
            }

            var activeLabel = labels[coords];
            activeLabel.Visible = true;
            activeLabel.Text = value.ToString("0.0");
            activeLabel.Modulate = grad.Sample(normalized.GetInfluence(coords));
            
            // float scale = Mathf.Clamp(Mathf.Abs(value), 0.3f, 1.0f);
            // activeLabel.Scale = new Vector3(scale, scale, scale);
        }
    }

    [Export] Gradient territoryGradient;
    public void UpdateVisuals()
    {
        // if (ShowThreat)
        // {
        //     DrawDebug(MapManager.ThreatMap, Colors.Red, _threatLabels, 0.5f);
        // }
        // else
        // {
        //     foreach(var label in _threatLabels.Values) label.Visible = false;
        // }

        if (ShowTerritory)
        {
            DrawDebug(MapManager.TerritoryMap, territoryGradient, territoryLabels, yOffset:1f);
        }
        else
        {
            foreach(var label in territoryLabels.Values) label.Visible = false;
        }
    }
}