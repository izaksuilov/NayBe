using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour, IPunObservable
{
    PhotonView photonView;
    RawImage image;
    public int unAss;
    public bool isPlayerTurn =  false;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        image = transform.GetChild(0).GetComponent<RawImage>();
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
        image.color = isPlayerTurn ? new Color(0.3f, 1, 0.3f) : Color.white;
    }
}
