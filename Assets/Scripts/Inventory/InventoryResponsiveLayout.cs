using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Keeps the placeholder inventory UI inside the visible canvas across different aspect ratios.
public class InventoryResponsiveLayout : MonoBehaviour
{
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField, Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;
    [SerializeField] private float safePadding = 24f;
    [SerializeField] private float minimumPanelScale = 0.62f;

    private readonly List<RectTransform> managedPanels = new List<RectTransform>();
    private readonly Dictionary<RectTransform, PanelLayoutSnapshot> panelSnapshots = new Dictionary<RectTransform, PanelLayoutSnapshot>();

    private RectTransform canvasRect;
    private CanvasScaler canvasScaler;
    private Vector2 lastCanvasSize;
    private bool initialized;

    // Receives the panels that should be kept on-screen and captures their authored prefab positions.
    public void Initialize(Transform canvasTransform, IEnumerable<Transform> inventoryPanels, Transform hudPanel)
    {
        canvasRect = canvasTransform as RectTransform;
        if (canvasRect == null)
            return;

        canvasScaler = canvasTransform.GetComponent<CanvasScaler>();
        ConfigureCanvasScaler();

        managedPanels.Clear();
        panelSnapshots.Clear();

        AddManagedPanel(hudPanel);

        if (inventoryPanels != null)
        {
            foreach (Transform panel in inventoryPanels)
                AddManagedPanel(panel);
        }

        initialized = true;
        ApplyLayout();
    }

    // Reapplies layout when the canvas size changes, such as when resizing the game view.
    private void Update()
    {
        if (!initialized || canvasRect == null)
            return;

        Vector2 canvasSize = canvasRect.rect.size;
        if (canvasSize == lastCanvasSize)
            return;

        ApplyLayout();
    }

    // Sets the Canvas Scaler to a balanced mode so very wide screens do not over-scale by width alone.
    private void ConfigureCanvasScaler()
    {
        if (canvasScaler == null)
            return;

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
    }

    // Adds one top-level UI panel to the safe-area pass.
    private void AddManagedPanel(Transform panel)
    {
        RectTransform rectTransform = panel as RectTransform;
        if (rectTransform == null || managedPanels.Contains(rectTransform))
            return;

        managedPanels.Add(rectTransform);
        panelSnapshots[rectTransform] = new PanelLayoutSnapshot(
            rectTransform.anchoredPosition,
            rectTransform.localScale);
    }

    // Restores authored positions, scales the whole layout when space is tight, then clamps as a final guard.
    private void ApplyLayout()
    {
        if (canvasRect == null)
            return;

        lastCanvasSize = canvasRect.rect.size;
        Rect safeRect = GetSafeRect();
        float groupScale = GetGroupScale(safeRect);

        foreach (RectTransform panel in managedPanels)
        {
            if (panel == null || !panelSnapshots.TryGetValue(panel, out PanelLayoutSnapshot snapshot))
                continue;

            panel.anchoredPosition = snapshot.anchoredPosition * groupScale;
            panel.localScale = snapshot.localScale * groupScale;

            ApplyScaleToFit(panel, snapshot, safeRect, groupScale);
            ClampInsideSafeRect(panel, safeRect);
        }
    }

    // Builds a padded canvas rectangle in local UI units.
    private Rect GetSafeRect()
    {
        Rect rect = canvasRect.rect;
        float horizontalPadding = Mathf.Min(safePadding, rect.width * 0.1f);
        float verticalPadding = Mathf.Min(safePadding, rect.height * 0.1f);

        return Rect.MinMaxRect(
            rect.xMin + horizontalPadding,
            rect.yMin + verticalPadding,
            rect.xMax - horizontalPadding,
            rect.yMax - verticalPadding);
    }

    // Calculates how much the authored whole-screen composition must shrink to fit the safe area.
    private float GetGroupScale(Rect safeRect)
    {
        Rect authoredBounds = GetAuthoredLayoutBounds();
        if (authoredBounds.width <= 0f || authoredBounds.height <= 0f)
            return 1f;

        float widthScale = safeRect.width / authoredBounds.width;
        float heightScale = safeRect.height / authoredBounds.height;
        float scale = Mathf.Min(1f, widthScale, heightScale);

        return Mathf.Max(minimumPanelScale, scale);
    }

    // Calculates the prefab-authored bounds of all managed panels before responsive scaling.
    private Rect GetAuthoredLayoutBounds()
    {
        bool hasBounds = false;
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;

        foreach (RectTransform panel in managedPanels)
        {
            if (panel == null || !panelSnapshots.TryGetValue(panel, out PanelLayoutSnapshot snapshot))
                continue;

            Vector2 size = Vector2.Scale(panel.rect.size, Abs(snapshot.localScale));
            Vector2 panelMin = snapshot.anchoredPosition - Vector2.Scale(size, panel.pivot);
            Vector2 panelMax = panelMin + size;

            if (!hasBounds)
            {
                min = panelMin;
                max = panelMax;
                hasBounds = true;
                continue;
            }

            min = Vector2.Min(min, panelMin);
            max = Vector2.Max(max, panelMax);
        }

        return hasBounds
            ? Rect.MinMaxRect(min.x, min.y, max.x, max.y)
            : Rect.zero;
    }

    // Shrinks oversized panels as a fallback for extreme aspect ratios.
    private void ApplyScaleToFit(RectTransform panel, PanelLayoutSnapshot snapshot, Rect safeRect, float groupScale)
    {
        Vector2 panelSize = panel.rect.size;
        if (panelSize.x <= 0f || panelSize.y <= 0f)
            return;

        float widthScale = safeRect.width / panelSize.x;
        float heightScale = safeRect.height / panelSize.y;
        float scale = Mathf.Min(groupScale, widthScale, heightScale);
        scale = Mathf.Max(minimumPanelScale, scale);

        panel.localScale = snapshot.localScale * scale;
    }

    // Uses positive scale values when estimating authored panel bounds.
    private Vector2 Abs(Vector3 value)
    {
        return new Vector2(Mathf.Abs(value.x), Mathf.Abs(value.y));
    }

    // Moves a panel just enough to keep its visible bounds inside the safe rectangle.
    private void ClampInsideSafeRect(RectTransform panel, Rect safeRect)
    {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, panel);
        Vector2 offset = Vector2.zero;

        if (bounds.min.x < safeRect.xMin)
            offset.x = safeRect.xMin - bounds.min.x;
        else if (bounds.max.x > safeRect.xMax)
            offset.x = safeRect.xMax - bounds.max.x;

        if (bounds.min.y < safeRect.yMin)
            offset.y = safeRect.yMin - bounds.min.y;
        else if (bounds.max.y > safeRect.yMax)
            offset.y = safeRect.yMax - bounds.max.y;

        panel.anchoredPosition += offset;
    }

    private readonly struct PanelLayoutSnapshot
    {
        public readonly Vector2 anchoredPosition;
        public readonly Vector3 localScale;

        public PanelLayoutSnapshot(Vector2 anchoredPosition, Vector3 localScale)
        {
            this.anchoredPosition = anchoredPosition;
            this.localScale = localScale;
        }
    }
}
