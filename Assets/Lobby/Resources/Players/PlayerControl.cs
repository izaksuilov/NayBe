using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour, IPunObservable
{
    private PhotonView photonView;
    private SpriteRenderer spriteRenderer;
    public int unAss { get; private set; } =  1;
    public List<GameObject> cards { get; set; }
    public List<GameObject> unAssCards { get; set; }
    public bool isPlayerTurn { get; set; } =  false;

    private bool isRed;
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

        FindObjectOfType<MapController>().AddPlayer(this);
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
