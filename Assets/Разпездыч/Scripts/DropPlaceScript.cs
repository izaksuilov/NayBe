using UnityEngine;
using UnityEngine.EventSystems;

public enum FieldType
{
    MY_HAND,
    FIELD,
    ENEMY_HAND,
    UNASS
}
public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    public FieldType Type;
    public void OnDrop(PointerEventData eventData)
    {
        if (!RazManager.isBeginningPhase && (Type == FieldType.ENEMY_HAND || Type == FieldType.UNASS)) return;
        //if (FindObjectOfType<RazManager>().isFirstPhase)
        CardScript card = eventData.pointerDrag.GetComponent<CardScript>();
        if (card)
            card.DefaultParent = transform;
    }
}
