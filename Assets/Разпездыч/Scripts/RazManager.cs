using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
//todo решить проблему, когда мастер закрывает игру
//todo решить проблему, когда выходит и добавляется игрок 
public class RazManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject playerPref;
    public MapController MapController;
    public static bool isBeginningPhase { get; set; } = false;
    void Start()
    {
        PhotonNetwork.Instantiate(playerPref.name, new Vector3(0,0,0), Quaternion.identity);
    }
    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
            Leave();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    public void Leave()
    {
        MapController.RemovePlayer();
        PhotonNetwork.LeaveRoom();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting)
        //{
        //    stream.SendNext();
        //}
        //else
        //{
        //     = ()stream.ReceiveNext();
        //}
    }
}
