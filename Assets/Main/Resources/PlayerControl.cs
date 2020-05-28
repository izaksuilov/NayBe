using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour, IPunObservable
{
    RawImage image;
    Image bg;
    public int unAss;
    public bool isPlayerTurn = false;
    void Start()
    {
        bg = transform.GetChild(0).GetComponent<Image>();
        image = transform.GetChild(1).GetComponent<RawImage>();
        transform.GetChild(2).GetChild(0).GetComponent<Text>().text = GetComponent<PhotonView>().Owner.NickName;
        string directoryPath = Application.persistentDataPath + "/avatar.jpg";
        try { image.texture = NativeGallery.LoadImageAtPath(directoryPath); }
        catch {image.texture = Resources.Load<Texture2D>("icons/Avatar");}
        unAss = (int)PhotonNetwork.CurrentRoom.CustomProperties["C3"];
        FindObjectOfType<MapController>().AddPlayer(this);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.Serialize(ref isPlayerTurn);
        stream.Serialize(ref unAss);
    }
    private void Update()
    {
        try
        {
            if (isPlayerTurn && GetComponent<PhotonView>().IsMine)
                transform.parent.parent.parent.GetChild(3).gameObject.SetActive(true);
            else transform.parent.parent.parent.GetChild(3).gameObject.SetActive(false);
        }
        catch{ }
        bg.color = isPlayerTurn ? new Color(0.329f, 0.878f, 0.22f) : Color.white;
    }
}
