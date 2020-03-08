using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    private static ToolTip instance;
    [SerializeField] private Camera camera;
    private Text tooltipText;
    private RectTransform background;
    private void Awake()
    {
        instance = this;
        background = transform.Find("Background").GetComponent<RectTransform>();
        tooltipText = transform.Find("Text").GetComponent<Text>();
        ShowTooltip("123");
    }
    private void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, camera, out localPoint);
        localPoint.x += 55;
        localPoint.y += 10;
        transform.localPosition = localPoint;
    }
    private void ShowTooltip(string tooltipString)
    {
        gameObject.SetActive(true);
        tooltipText.text = tooltipString;
        Vector2 bgSize = new Vector2(tooltipText.preferredWidth + 8, tooltipText.preferredHeight + 8);
        background.sizeDelta = bgSize;
    }
    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }
    public static void Show_Static(string tooltipString)
    {
        instance.ShowTooltip(tooltipString);
    }
    public static void Hide_Static()
    {
        instance.HideTooltip();
    }
}
