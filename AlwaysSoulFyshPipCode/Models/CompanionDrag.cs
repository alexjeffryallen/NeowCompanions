using Godot;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace NeowCompanions.NeowCompanionsCode.Models;

internal static class CompanionDrag
{
    public static NCreatureVisuals MakeDraggable(NCreatureVisuals visuals)
    {
        MakeNodeDraggable(visuals);
        return visuals;
    }

    public static T MakeNodeDraggable<T>(T visuals)
        where T : Node2D
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
    private const float ScaleStep = 1.1f;
    private const float MinScaleMultiplier = 0.4f;
    private const float MaxScaleMultiplier = 2.5f;
    private static readonly Vector2 MinPosition = new(35.0f, 85.0f);
    private static readonly Vector2 MaxPosition = new(1875.0f, 850.0f);

    private static readonly Dictionary<string, Vector2> SavedNormalizedPositions = new();
    private static readonly Dictionary<string, float> SavedScaleMultipliers = new();
    private static CompanionDragController? activeDragController;

    private bool isDragging;
    private bool hasInitializedPosition;
    private Vector2 dragOffset;
    private string? companionKey;
    private float scaleMultiplier = 1f;

    public override void _Process(double delta)
    {
        if (GetParent() is not Node2D visuals)
        {
            return;
        }

        InitializePosition(visuals);

        bool rightMouseDown = Input.IsMouseButtonPressed(MouseButton.Right);
        Vector2 mousePosition = visuals.GetGlobalMousePosition();

        if (!rightMouseDown)
        {
            if (activeDragController == this)
            {
                activeDragController = null;
            }

            isDragging = false;
            return;
        }

        if (!isDragging)
        {
            if (activeDragController != null
                && GodotObject.IsInstanceValid(activeDragController)
                && activeDragController != this)
            {
                return;
            }

            if (visuals.GlobalPosition.DistanceTo(mousePosition) > HitRadius)
            {
                return;
            }

            activeDragController = this;
            isDragging = true;
            dragOffset = visuals.GlobalPosition - mousePosition;
        }

        visuals.GlobalPosition = ClampPosition(mousePosition + dragOffset);
        SavePosition(visuals);
    }

    public override void _ExitTree()
    {
        if (activeDragController == this)
        {
            activeDragController = null;
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (!isDragging
            || activeDragController != this
            || inputEvent is not InputEventMouseButton mouseButton
            || !mouseButton.Pressed
            || (mouseButton.ButtonIndex != MouseButton.WheelUp
                && mouseButton.ButtonIndex != MouseButton.WheelDown)
            || GetParent() is not Node2D visuals)
        {
            return;
        }

        float step = mouseButton.ButtonIndex == MouseButton.WheelUp
            ? ScaleStep
            : 1f / ScaleStep;
        float newMultiplier = Mathf.Clamp(
            scaleMultiplier * step,
            MinScaleMultiplier,
            MaxScaleMultiplier);
        float relativeChange = newMultiplier / scaleMultiplier;

        visuals.Scale *= relativeChange;
        scaleMultiplier = newMultiplier;
        if (companionKey != null)
        {
            SavedScaleMultipliers[companionKey] = scaleMultiplier;
        }

        GetViewport().SetInputAsHandled();
    }

    private void InitializePosition(Node2D visuals)
    {
        if (hasInitializedPosition)
        {
            return;
        }

        NCreature? creatureNode = FindCreatureNode(visuals);
        if (creatureNode?.Entity == null)
        {
            return;
        }

        hasInitializedPosition = true;
        companionKey = creatureNode.Entity.ModelId.ToString();
        if (SavedScaleMultipliers.TryGetValue(companionKey, out float savedScaleMultiplier))
        {
            scaleMultiplier = savedScaleMultiplier;
            visuals.Scale *= scaleMultiplier;
        }

        Vector2 viewportSize = visuals.GetViewportRect().Size;
        if (viewportSize.X <= 0f || viewportSize.Y <= 0f
            || !SavedNormalizedPositions.TryGetValue(companionKey, out Vector2 normalizedPosition))
        {
            return;
        }

        visuals.GlobalPosition = ClampPosition(new Vector2(
            normalizedPosition.X * viewportSize.X,
            normalizedPosition.Y * viewportSize.Y));
    }

    private void SavePosition(Node2D visuals)
    {
        if (companionKey == null)
        {
            return;
        }

        Vector2 viewportSize = visuals.GetViewportRect().Size;
        if (viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            return;
        }

        SavedNormalizedPositions[companionKey] = new Vector2(
            visuals.GlobalPosition.X / viewportSize.X,
            visuals.GlobalPosition.Y / viewportSize.Y);
    }

    private static NCreature? FindCreatureNode(Node node)
    {
        Node? current = node.GetParent();
        while (current != null)
        {
            if (current is NCreature creature)
            {
                return creature;
            }

            current = current.GetParent();
        }

        return null;
    }

    private static Vector2 ClampPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.X, MinPosition.X, MaxPosition.X),
            Mathf.Clamp(position.Y, MinPosition.Y, MaxPosition.Y));
    }
}
