using Photon.Pun;
using UnityEngine;

public class PlayerControl : MonoBehaviour, IPunObservable
{
    PhotonView photonView;
    SpriteRenderer spriteRenderer;
    public int unAss { get; private set; }
    public bool isPlayerTurn { get; set; } =  false;
    public string name1;

    bool isRed;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isRed);
            stream.SendNext(unAss);
        }
        else
        {
            isRed = (bool) stream.ReceiveNext();
            unAss = (int) stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //name1 = PhotonNetwork.PlayerList.Length;
        unAss = (int)PhotonNetwork.CurrentRoom.CustomProperties["C3"];
        FindObjectOfType<MapController>().AddPlayer(this);
        Debug.Log(unAss);
        Debug.Log(PhotonNetwork.PlayerList.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && PhotonNetwork.IsConnected == true) // пользовательский ввод
        {
            if (Input.GetKey(KeyCode.Space))
                isRed = true;
            else isRed = false;
        }
        if (isRed)
            spriteRenderer.color = Color.red;
        else
            spriteRenderer.color = Color.white;
    }
}
