using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour, IPunObservable
{
    RawImage image;
    Image bg;
    Texture texture;
    public int unAss;
    public bool isPlayerTurn =  false;
    public bool isInApp = true;
    void Start()
    {
        bg = transform.GetChild(0).GetComponent<Image>();
        image = transform.GetChild(1).GetComponent<RawImage>();
        transform.GetChild(2).GetChild(0).GetComponent<Text>().text = GetComponent<PhotonView>().Owner.NickName;
        texture = GameObject.Find("Avatar(Clone)").GetComponent<RawImage>().texture;
        image.texture = texture;
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
        bg.color = isPlayerTurn ? new Color(0.329f, 0.878f, 0.22f) : Color.white;
    }
}
