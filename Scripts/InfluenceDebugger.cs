using Godot;
using Game;
using Game.AI;
using System.Collections.Generic;

public partial class InfluenceDebugger : Node3D
{
    [Export] public InfluenceMapManager MapManager;
    [Export] public HexGrid3D Grid { get; set; }
    [Export] public bool ShowInterest = true;
    [Export] public bool ShowThreat = true;

    private readonly Dictionary<Vector2I, Label3D> _interestLabels = new();
    private readonly Dictionary<Vector2I, Label3D> _threatLabels = new();

    public override void _Process(double delta)
    {
        if (MapManager == null || Grid == null) return;
        UpdateVisuals();
    }

    private void DrawDebug(InfluenceMap mapToVisualize, Color color, Dictionary<Vector2I, Label3D> labels, float yOffset = 0f)
    {
        foreach (var label in labels.Values)
        {
            label.Visible = false;
        }

        foreach (var kvp in mapToVisualize.GetAllInfluences())
        {
            Vector2I coords = kvp.Key;
            float value = kvp.Value;

            if (value <= 0.01f) continue;

            if (!labels.ContainsKey(coords))
            {
                var label = new Label3D
                {
                    Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
                    FontSize = 32,
                    GlobalPosition = Grid.HexToWorld(coords) + (Vector3.Up * 2.0f) + (Vector3.Up * yOffset)
                };
                AddChild(label);
                labels[coords] = label;
            }

            var activeLabel = labels[coords];
            activeLabel.Visible = true;
            activeLabel.Text = value.ToString("0.0");
            activeLabel.Modulate = color;
            
            float scale = Mathf.Clamp(value * 0.5f, 0.5f, 2.0f);
            activeLabel.Scale = new Vector3(scale, scale, scale);
        }
    }
    
    public void UpdateVisuals()
    {
        if (ShowThreat)
        {
            DrawDebug(MapManager.ThreatMap, Colors.Red, _threatLabels, 0.5f);
        }
        else
        {
            foreach(var label in _threatLabels.Values) label.Visible = false;
        }

        if (ShowInterest)
        {
            DrawDebug(MapManager.InterestMap, Colors.Green, _interestLabels);
        }
        else
        {
            foreach(var label in _interestLabels.Values) label.Visible = false;
        }
    }
}