using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeBehaviour : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public int PairId { get; private set; }
    public RectTransform Rect { get; private set; }
    public Color Color { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    private LogicPuzzle parent;
    private Image image;

    public void Initialize(LogicPuzzle parent, int pairId, Vector2 anchoredPosition, Vector2Int gridPosition, Color color)
    {
        this.parent = parent;
        this.PairId = pairId;
        this.GridPosition = gridPosition;
        this.Color = color;

        Rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (Rect != null)
            Rect.anchoredPosition = anchoredPosition;

        if (image != null)
            image.color = color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        parent?.OnNodePointerDown(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        parent?.OnNodeDrag(this, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        parent?.OnNodePointerUp(this, eventData);
    }
}