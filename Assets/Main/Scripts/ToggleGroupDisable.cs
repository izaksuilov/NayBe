using UnityEngine;
using UnityEngine.UI;

public class ToggleGroupDisable : MonoBehaviour
{
    public void ForbidLastDisable()
    {
        int j = 0;
        for (int i = 0; i < transform.parent.childCount; i++)
            if (transform.parent.GetChild(i).GetComponent<Toggle>().isOn) j++;
        if (j < 1) GetComponent<Toggle>().isOn = true;
    }
}
