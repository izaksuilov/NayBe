using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HandScript : MonoBehaviour
{
    //скрипт, который раскладывает карты в руке
    void Update()
    {
        int childCount = GetComponent<HorizontalLayoutGroup>().transform.childCount;
        GetComponent<HorizontalLayoutGroup>().spacing = -GetComponent<RectTransform>().sizeDelta.x;

        float rotation = childCount == 2 ? 25 : (float)Math.Pow(228 * childCount, 1 / 2f);

        for (int i = 0; i < childCount; i++)
            GetComponent<HorizontalLayoutGroup>().transform.GetChild(i).GetChild(0).transform.rotation = 
                Quaternion.Euler(0, 0, childCount == 1 ? 0 : rotation - rotation * 2f / (childCount - 1) * i);

        if (RazManager.isBeginningPhase || transform.childCount == 1 || transform.GetComponent<DropPlaceScript>().Type != FieldType.MY_HAND) return;

        List<Card> hand = new List<Card>(); 
        foreach (Transform card in transform)
            hand.Add(card.GetComponent<CardScript>().thisCard);
        var sortedHand = hand.GroupBy(x => x.Suit)
                             .Select(x => new
                             {
                                 Cards = x.OrderBy(c => c.Value),
                             })
                             .SelectMany(x => x.Cards)
                             .OrderBy(c => c.Suit.Equals(RazManager.ace));

        int j = 0;
        foreach (var card in sortedHand)
        {
            for (int k = 0; k < transform.childCount; k++)
            {
                var thisCard = transform.GetChild(k).GetComponent<CardScript>().thisCard;
                if (thisCard.Value == card.Value && thisCard.Suit.Equals(card.Suit))
                {
                    transform.GetChild(k).SetSiblingIndex(j);
                    break;
                }
            }
            j++;
        }
    }
}
