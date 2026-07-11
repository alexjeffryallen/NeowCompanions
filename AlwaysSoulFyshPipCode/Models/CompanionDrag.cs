using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace NeowCompanions.NeowCompanionsCode.Models;

internal static class CompanionDrag
{
    public static NCreatureVisuals MakeDraggable(NCreatureVisuals visuals)
    {
        if (visuals.GetNodeOrNull<CompanionDragController>(nameof(CompanionDragController)) == null)
        {
            CompanionDragController controller = new()
            {
                Name = nameof(CompanionDragController),
            };
            visuals.AddChild(controller);
        }

        return visuals;
    }
}

internal sealed partial class CompanionDragController : Node
{
    private const float HitRadius = 130.0f;
    private static readonly Vector2 MinPosition = new(35.0f, 85.0f);
    private static readonly Vector2 MaxPosition = new(1875.0f, 850.0f);

    private bool isDragging;
    private Vector2 dragOffset;

    public override void _Process(double delta)
    {
        if (GetParent() is not Node2D visuals)
        {
            return;
        }

        bool rightMouseDown = Input.IsMouseButtonPressed(MouseButton.Right);
        Vector2 mousePosition = visuals.GetGlobalMousePosition();

        if (!rightMouseDown)
        {
            isDragging = false;
            return;
        }

        if (!isDragging)
        {
            if (visuals.GlobalPosition.DistanceTo(mousePosition) > HitRadius)
            {
                return;
            }

            isDragging = true;
            dragOffset = visuals.GlobalPosition - mousePosition;
        }

        visuals.GlobalPosition = ClampPosition(mousePosition + dragOffset);
    }

    private static Vector2 ClampPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.X, MinPosition.X, MaxPosition.X),
            Mathf.Clamp(position.Y, MinPosition.Y, MaxPosition.Y));
    }
}
