using Godot;
using System;

public partial class CameraRig : Node3D
{
    // ... (Variables de Movimiento y Límites igual que antes) ...
    [ExportGroup("Movimiento")]
    [Export] public float MoveSpeed = 10.0f;
    [Export] public float SmoothSpeed = 10.0f;
    [Export] public Rect2 LimitRect = new Rect2(-50, -50, 100, 100);

    [ExportGroup("Rotación")]
    [Export] public float RotationStep = 15.0f;
    [Export] public float RotationSmoothing = 5.0f;

    // NUEVO: Tiempo mínimo entre giros para evitar "spam" de la rueda
    [Export] public float RotationCooldown = 0.15f;

    private Vector3 _targetPosition;
    private float _targetRotationY;

    // NUEVO: Variable para controlar el cooldown
    private double _lastRotationTime = 0.0f;

    public override void _Ready()
    {
        _targetPosition = GlobalPosition;
        _targetRotationY = RotationDegrees.Y;
    }

    public override void _Process(double delta)
    {
        float fDelta = (float)delta;

        HandleMovement(fDelta);

        // 1. Movimiento (Igual que antes)
        GlobalPosition = GlobalPosition.Lerp(_targetPosition, SmoothSpeed * fDelta);

        // 2. Rotación: Aquí está el truco para evitar el efecto zootropo
        // Si la cámara ya está muy cerca del objetivo, reseteamos el objetivo a la posición actual
        // Esto evita que se acumulen vueltas si dejas de girar la rueda
        if (Mathf.Abs(RotationDegrees.Y - _targetRotationY) < 0.1f)
        {
             _targetRotationY = RotationDegrees.Y;
        }

        Vector3 currentRot = RotationDegrees;
        currentRot.Y = Mathf.Lerp(currentRot.Y, _targetRotationY, RotationSmoothing * fDelta);
        RotationDegrees = currentRot;
    }

    // ... (HandleMovement igual que antes) ...
    private void HandleMovement(float delta)
    {
        Vector2 inputDir = Input.GetVector("cam_left", "cam_right", "cam_forward", "cam_back");
        if (inputDir != Vector2.Zero)
        {
            Vector3 direction = (Transform.Basis.Z * inputDir.Y) + (Transform.Basis.X * inputDir.X);
            direction.Y = 0;
            direction = direction.Normalized();
            _targetPosition += direction * MoveSpeed * delta;
            _targetPosition.X = Mathf.Clamp(_targetPosition.X, LimitRect.Position.X, LimitRect.End.X);
            _targetPosition.Z = Mathf.Clamp(_targetPosition.Z, LimitRect.Position.Y, LimitRect.End.Y);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Usamos el tiempo actual del sistema para el cooldown
        double timeNow = Time.GetTicksMsec() / 1000.0;

        if (@event.IsActionPressed("cam_rotate_left"))
        {
            // Solo permitimos girar si ha pasado el tiempo de cooldown
            if (timeNow - _lastRotationTime > RotationCooldown)
            {
                // TRUCO ANTI-ZOOTROPO:
                // En lugar de sumar siempre, forzamos que el objetivo sea relativo a donde
                // estamos visualmente AHORA, no donde "deberíamos estar".
                // Esto corta la cola de rotaciones pendientes.
                _targetRotationY = RotationDegrees.Y + RotationStep;
                _lastRotationTime = timeNow;
            }
        }
        else if (@event.IsActionPressed("cam_rotate_right"))
        {
            if (timeNow - _lastRotationTime > RotationCooldown)
            {
                _targetRotationY = RotationDegrees.Y - RotationStep;
                _lastRotationTime = timeNow;
            }
        }
    }
}