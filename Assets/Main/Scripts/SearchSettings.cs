using UnityEngine;

public class SearchSettings : MonoBehaviour
{
    public void ToggleSettings()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
