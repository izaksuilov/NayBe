using Photon.Pun;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetKey(KeyCode.LeftArrow)) transform.Translate(-Time.deltaTime * 7, 0, 0);
        if (Input.GetKey(KeyCode.RightArrow)) transform.Translate(Time.deltaTime * 7, 0, 0);
    }
}
