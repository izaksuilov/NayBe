using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BankCardScript : MonoBehaviour
{
    public Card thisCard { get; private set; }

    void Update()
    {
        
    }
    void Awake()
    {
        transform.GetChild(0).GetChild(1).gameObject
            .SetActive(transform.parent.name.Equals("Ace Field") ||
                       transform.parent.name.Equals("Bank Field"));
    }
    public void CreateCard(Card card)
    {
        thisCard = card;
    }
}
