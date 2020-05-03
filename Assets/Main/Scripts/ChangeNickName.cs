using UnityEngine;
using UnityEngine.UI;

public class ChangeNickName : MonoBehaviour
{
    [SerializeField] private Button CreateButton;
    void Start()
    {
        GetComponent<InputField>().text = Settings.nickName;
    }
    public void ChangeNick()
    {
        Settings.SaveNickName(GetComponent<InputField>().text);
        if (CreateButton.interactable == false)
        {
            CreateButton.gameObject.transform.GetChild(0).GetComponent<Text>().text = "Создать комнату";
            CreateButton.interactable = true;
        }
    }
}
