using Godot;

public partial class MapGenerator : Node3D
{
    [Export]
    int MapRadius { get; set; } = 10;

    [Export]
    public PackedScene HexTileScene { get; set; }

    // El tamaño (radio) de tu malla hexagonal 3D.
    // Ajústalo para que coincida con el tamaño de tu modelo.
    private float hexSize = 1.0f;
    private FastNoiseLite noise;

    public override void _Ready()
    {
        // Inicializamos el objeto de ruido
        noise = new FastNoiseLite();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        noise.Frequency = 0.05f; // Puedes experimentar con estos valores

        GenerateMap();
    }

    public void GenerateMap()
    {
        // Primero, comprobamos si la escena de la tesela ha sido asignada en el editor
        if (HexTileScene == null)
        {
            GD.PrintErr("La escena 'HexTileScene' no está asignada en el MapGenerator.");
            return;
        }

        // Doble bucle para recorrer la rejilla hexagonal en un área cuadrada
        for (int q = -MapRadius; q <= MapRadius; q++)
        {
            for (int r = -MapRadius; r <= MapRadius; r++)
            {
                // Calculamos la tercera coordenada cúbica 's'
                int s = -q - r;

                // Esta condición asegura que estamos generando dentro de un radio hexagonal,
                // no en un cuadrado, lo que evita que se generen teselas en las esquinas.
                if (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s) <= MapRadius * 2)
                {
                    // 1. Obtener la altura del ruido
                    // Multiplicamos por un valor (ej. 5.0f) para amplificar el efecto.
                    float height = noise.GetNoise2D(q, r) * 5.0f;

                    // 2. Convertir coordenadas del hexágono a posición 3D en el mundo
                    // Esta es la fórmula estándar para convertir coordenadas axiales (q, r) a cartesianas (x, z)
                    float xPos = hexSize * 3.0f / 2.0f * q;
                    float zPos = hexSize * Mathf.Sqrt(3.0f) * (r + q / 2.0f);

                    Vector3 hexPos = new Vector3(xPos, height, zPos);

                    // 3. Instanciar y posicionar la tesela
                    Node3D newHex = HexTileScene.Instantiate<Node3D>();
                    newHex.Position = hexPos;
                    AddChild(newHex);
                }
            }
        }
    }
}