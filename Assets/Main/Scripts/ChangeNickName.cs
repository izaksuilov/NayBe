using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNickName : MonoBehaviour
{
    [SerializeField] private Button CreateButton;
    void Start()
    {
        GetComponent<InputField>().text = Settings.NickName;
    }
    public void ChangeNick()
    {
        if (GetComponent<InputField>().text.Length < 1) return;
        Settings.NickName = GetComponent<InputField>().text;
        if (!CreateButton.interactable)
        {
            CreateButton.gameObject.transform.GetChild(0).GetComponent<Text>().text = "Создать комнату";
            CreateButton.interactable = true;
        }
        PhotonNetwork.NickName = Settings.NickName;
    }
}
