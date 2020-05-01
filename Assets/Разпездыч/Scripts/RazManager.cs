using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
//todo решить проблему, когда мастер закрывает игру
//todo решить проблему, когда выходит и добавляется игрок 
public class RazManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject playerPref;
    private bool isAllInRoom = false;
    public MapController MapController; 
    void Start()
    {
        PhotonNetwork.Instantiate(playerPref.name, new Vector3(0,0,0), Quaternion.identity);
    }
    private void Update()
    {
        //В случае, если игрок закрыл приложение
        if (isAllInRoom && PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            MapController.RemovePlayer();
            isAllInRoom = false;
        }
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
            Leave();

    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            isAllInRoom = true;
}
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    public void Leave()
    {
        isAllInRoom = false;
        MapController.RemovePlayer();
        PhotonNetwork.LeaveRoom();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isAllInRoom);
        }
        else
        {
            isAllInRoom = (bool)stream.ReceiveNext();
        }
    }
}
