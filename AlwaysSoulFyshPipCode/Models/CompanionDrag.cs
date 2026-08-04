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

    public static T MakeLinkedDraggable<T>(T visuals, string linkKey, Vector2 linkOffset)
        where T : Node2D
    {
        MakeNodeDraggable(visuals);
        CompanionDragController controller = visuals.GetNode<CompanionDragController>(nameof(CompanionDragController));
        controller.LinkKey = linkKey;
        controller.LinkOffset = linkOffset;
        return visuals;
    }
}

internal sealed partial class CompanionDragController : Node2D
{
    private const float BoundsPadding = 10.0f;
    private const float ScaleStep = 1.1f;
    private const float MinScaleMultiplier = 0.4f;
    private const float MaxScaleMultiplier = 2.5f;
    private static readonly Vector2 MinPosition = new(35.0f, 85.0f);
    private static readonly Vector2 MaxPosition = new(1875.0f, 850.0f);

    private static readonly Dictionary<string, Vector2> SavedNormalizedPositions = new();
    private static readonly Dictionary<string, float> SavedScaleMultipliers = new();
    private static readonly Dictionary<string, List<CompanionDragController>> LinkedControllers = new();
    private static readonly Dictionary<string, Vector2> LinkedAnchors = new();
    private static readonly List<CompanionDragController> AllControllers = [];
    private static CompanionDragController? activeDragController;

    public string? LinkKey { get; set; }
    public Vector2 LinkOffset { get; set; }

    private bool isDragging;
    private bool hasInitializedPosition;
    private Vector2 dragOffset;
    private string? companionKey;
    private float scaleMultiplier = 1f;

    public override void _Ready()
    {
        AllControllers.Add(this);

        if (LinkKey == null)
            return;

        if (!LinkedControllers.TryGetValue(LinkKey, out List<CompanionDragController>? controllers))
        {
            controllers = [];
            LinkedControllers[LinkKey] = controllers;
        }

        controllers.Add(this);
    }

    public override void _Process(double delta)
    {
        if (GetParent() is not Node2D visuals)
        {
            return;
        }

        InitializePosition(visuals);

        bool rightMouseDown = Input.IsMouseButtonPressed(MouseButton.Right);
        bool leftMouseDown = Input.IsMouseButtonPressed(MouseButton.Left);
        Vector2 mousePosition = visuals.GetGlobalMousePosition();
        QueueRedraw();

        if (rightMouseDown && leftMouseDown)
        {
            if (activeDragController == this)
                activeDragController = null;
            isDragging = false;
            return;
        }

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

            if (FindClosestController(mousePosition) != this
                || !GetGlobalTargetRect(visuals).HasPoint(mousePosition))
            {
                return;
            }

            activeDragController = this;
            isDragging = true;
            Vector2 dragOrigin = LinkKey != null && LinkedAnchors.TryGetValue(LinkKey, out Vector2 anchor)
                ? anchor
                : visuals.GlobalPosition;
            dragOffset = dragOrigin - mousePosition;
        }

        Vector2 newPosition = ClampPosition(mousePosition + dragOffset);
        if (LinkKey != null)
        {
            SetLinkedAnchor(newPosition);
            SaveLinkedPosition(visuals, newPosition);
        }
        else
        {
            visuals.GlobalPosition = newPosition;
            SavePosition(visuals);
        }
    }

    public override void _ExitTree()
    {
        if (activeDragController == this)
        {
            activeDragController = null;
        }

        AllControllers.Remove(this);

        if (LinkKey != null && LinkedControllers.TryGetValue(LinkKey, out List<CompanionDragController>? controllers))
        {
            controllers.Remove(this);
            if (controllers.Count == 0)
            {
                LinkedControllers.Remove(LinkKey);
                LinkedAnchors.Remove(LinkKey);
            }
        }
    }

    public override void _Draw()
    {
        if (!Input.IsMouseButtonPressed(MouseButton.Right)
            || GetParent() is not Node2D visuals)
            return;

        bool showAllZones = Input.IsMouseButtonPressed(MouseButton.Left);
        if (!showAllZones && FindClosestController(visuals.GetGlobalMousePosition()) != this)
            return;

        Rect2 targetRect = GetGlobalTargetRect(visuals);
        Vector2[] points =
        [
            ToLocal(targetRect.Position),
            ToLocal(new Vector2(targetRect.End.X, targetRect.Position.Y)),
            ToLocal(targetRect.End),
            ToLocal(new Vector2(targetRect.Position.X, targetRect.End.Y))
        ];
        DrawColoredPolygon(points, new Color(0.15f, 0.85f, 1f, 0.10f));
        DrawPolyline([.. points, points[0]], new Color(0.2f, 0.9f, 1f, 0.95f), 3f, true);
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

        if (LinkKey != null)
        {
            ScaleLinked(relativeChange, newMultiplier);
        }
        else
        {
            visuals.Scale *= relativeChange;
        }
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

        Vector2 viewportSize = visuals.GetViewportRect().Size;
        if (viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            return;
        }

        hasInitializedPosition = true;
        companionKey = LinkKey ?? creatureNode.Entity.ModelId.ToString();
        if (SavedScaleMultipliers.TryGetValue(companionKey, out float savedScaleMultiplier))
        {
            scaleMultiplier = savedScaleMultiplier;
            if (LinkKey == null)
                visuals.Scale *= scaleMultiplier;
        }

        if (LinkKey != null)
        {
            InitializeLinkedPosition(visuals);
            return;
        }

        if (SavedNormalizedPositions.TryGetValue(companionKey, out Vector2 normalizedPosition))
        {
            visuals.GlobalPosition = ClampPosition(new Vector2(
                normalizedPosition.X * viewportSize.X,
                normalizedPosition.Y * viewportSize.Y));
        }
    }

    private void InitializeLinkedPosition(Node2D visuals)
    {
        if (LinkKey == null)
            return;

        Vector2 viewportSize = visuals.GetViewportRect().Size;
        Vector2 anchor;
        if (!LinkedAnchors.TryGetValue(LinkKey, out anchor))
        {
            anchor = visuals.GlobalPosition - LinkOffset * scaleMultiplier;
            if (viewportSize.X > 0f && viewportSize.Y > 0f
                && SavedNormalizedPositions.TryGetValue(LinkKey, out Vector2 normalizedPosition))
            {
                anchor = ClampPosition(new Vector2(
                    normalizedPosition.X * viewportSize.X,
                    normalizedPosition.Y * viewportSize.Y));
            }
            LinkedAnchors[LinkKey] = anchor;
        }

        if (SavedScaleMultipliers.ContainsKey(LinkKey))
            visuals.Scale *= scaleMultiplier;
        visuals.GlobalPosition = anchor + LinkOffset * scaleMultiplier;
    }

    private static CompanionDragController? FindClosestController(Vector2 mousePosition)
    {
        CompanionDragController? closest = null;
        float closestDistance = float.MaxValue;
        foreach (CompanionDragController controller in AllControllers)
        {
            if (!GodotObject.IsInstanceValid(controller)
                || controller.GetParent() is not Node2D visuals
                || !visuals.IsVisibleInTree())
                continue;

            Rect2 targetRect = GetGlobalTargetRect(visuals);
            Vector2 nearestPoint = new(
                Mathf.Clamp(mousePosition.X, targetRect.Position.X, targetRect.End.X),
                Mathf.Clamp(mousePosition.Y, targetRect.Position.Y, targetRect.End.Y));
            float distance = nearestPoint.DistanceSquaredTo(mousePosition);
            if (distance == closestDistance && closest?.GetParent() is Node2D previousVisuals)
            {
                float currentCenterDistance = targetRect.GetCenter().DistanceSquaredTo(mousePosition);
                float previousCenterDistance = GetGlobalTargetRect(previousVisuals).GetCenter().DistanceSquaredTo(mousePosition);
                if (currentCenterDistance >= previousCenterDistance)
                    continue;
            }
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = controller;
            }
        }
        return closest;
    }

    private static Rect2 GetGlobalTargetRect(Node2D visualNode)
    {
        if (visualNode is NCreatureVisuals visuals
            && visuals.SpineBody != null
            && visuals.SpineBody.BoundObject is Node2D spineNode)
        {
            Rect2 localBounds = visuals.SpineBody.GetSkeleton().GetBounds();
            Vector2[] globalCorners =
            [
                spineNode.ToGlobal(localBounds.Position),
                spineNode.ToGlobal(new Vector2(localBounds.End.X, localBounds.Position.Y)),
                spineNode.ToGlobal(localBounds.End),
                spineNode.ToGlobal(new Vector2(localBounds.Position.X, localBounds.End.Y))
            ];
            float minX = Mathf.Min(Mathf.Min(globalCorners[0].X, globalCorners[1].X), Mathf.Min(globalCorners[2].X, globalCorners[3].X));
            float minY = Mathf.Min(Mathf.Min(globalCorners[0].Y, globalCorners[1].Y), Mathf.Min(globalCorners[2].Y, globalCorners[3].Y));
            float maxX = Mathf.Max(Mathf.Max(globalCorners[0].X, globalCorners[1].X), Mathf.Max(globalCorners[2].X, globalCorners[3].X));
            float maxY = Mathf.Max(Mathf.Max(globalCorners[0].Y, globalCorners[1].Y), Mathf.Max(globalCorners[2].Y, globalCorners[3].Y));
            return new Rect2(minX, minY, maxX - minX, maxY - minY).Grow(BoundsPadding);
        }

        if (visualNode is NCreatureVisuals fallbackVisuals && fallbackVisuals.Bounds != null)
            return fallbackVisuals.Bounds.GetGlobalRect().Grow(BoundsPadding);

        return new Rect2(visualNode.GlobalPosition - new Vector2(45f, 45f), new Vector2(90f, 90f));
    }

    private void SetLinkedAnchor(Vector2 anchor)
    {
        if (LinkKey == null)
            return;

        LinkedAnchors[LinkKey] = anchor;
        if (!LinkedControllers.TryGetValue(LinkKey, out List<CompanionDragController>? controllers))
            return;

        foreach (CompanionDragController controller in controllers)
        {
            if (controller.GetParent() is Node2D member)
                member.GlobalPosition = anchor + controller.LinkOffset * scaleMultiplier;
        }
    }

    private void ScaleLinked(float relativeChange, float newMultiplier)
    {
        if (LinkKey == null || !LinkedControllers.TryGetValue(LinkKey, out List<CompanionDragController>? controllers))
            return;

        Vector2 anchor = LinkedAnchors.GetValueOrDefault(LinkKey);
        foreach (CompanionDragController controller in controllers)
        {
            if (controller.GetParent() is not Node2D member)
                continue;
            member.Scale *= relativeChange;
            member.GlobalPosition = anchor + controller.LinkOffset * newMultiplier;
            controller.scaleMultiplier = newMultiplier;
        }
    }

    private void SaveLinkedPosition(Node2D visuals, Vector2 anchor)
    {
        if (companionKey == null)
            return;
        Vector2 viewportSize = visuals.GetViewportRect().Size;
        if (viewportSize.X > 0f && viewportSize.Y > 0f)
            SavedNormalizedPositions[companionKey] = new Vector2(anchor.X / viewportSize.X, anchor.Y / viewportSize.Y);
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
