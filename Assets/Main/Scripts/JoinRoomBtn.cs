using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomBtn : MonoBehaviour
{
    // Start is called before the first frame update
    public void Join()
    {
        PhotonNetwork.JoinOrCreateRoom(GetComponent<Text>().text, new RoomOptions(), TypedLobby.Default);
    }
}
