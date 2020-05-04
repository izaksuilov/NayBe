using System;
using UnityEngine;
using UnityEngine.UI;

public class HandScript : MonoBehaviour
{
    int prevChildCont = 0;
    void Update()
    {
        int currentChildCount = GetComponent<HorizontalLayoutGroup>().transform.childCount;
        GetComponent<HorizontalLayoutGroup>().spacing = -GetComponent<RectTransform>().sizeDelta.x;

        if (currentChildCount == prevChildCont) return;

        prevChildCont = currentChildCount;
        float rotation = currentChildCount == 2 ? 25 :(float)Math.Pow(500 * currentChildCount, 1 / 2f);

        for (int i = 0; i < currentChildCount; i++)
            GetComponent<HorizontalLayoutGroup>().transform.GetChild(i)
                .GetChild(0).transform.rotation = Quaternion.Euler(0, 0,
                currentChildCount == 1 ? 0 : rotation - rotation * 2f / (currentChildCount - 1) * i);
    }
}
