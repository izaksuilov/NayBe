using Photon.Pun;
using UnityEngine;

public class PLayerControlOffline : MonoBehaviour
{
    PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine) // пользовательский ввод
        {
            if (Input.GetKey(KeyCode.LeftArrow)) transform.Translate(-Time.deltaTime * 75, 0, 0);
            if (Input.GetKey(KeyCode.RightArrow)) transform.Translate(Time.deltaTime * 75, 0, 0);
        }
    }
}
