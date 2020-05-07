using UnityEngine;
using UnityEngine.EventSystems;

public enum FieldType
{
    MY_HAND,
    FIELD,
    OTHER
}
public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    public FieldType Type;
    public void OnDrop(PointerEventData eventData)
    {
        if (Type == FieldType.OTHER) return;
        CardScript card = eventData.pointerDrag.GetComponent<CardScript>();
        if (card)
            card.DefaultParent = transform;
    }
}
