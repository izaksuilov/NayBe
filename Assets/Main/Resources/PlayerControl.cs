using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour, IPunObservable
{
    RawImage image;
    Image bg;
    public int unAss, money;
    private int width = -1, height = -1;
    public bool isPlayerTurn = false, isFieldClear = false, isReadyToStart = true;
    string imgBytes;
    void Start()
    {
        bg = transform.GetChild(0).GetComponent<Image>();
        image = transform.GetChild(1).GetComponent<RawImage>();
        transform.GetChild(2).GetChild(0).GetComponent<Text>().text = GetComponent<PhotonView>().Owner.NickName;
        if (GetComponent<PhotonView>().IsMine)
        {
            string directoryPath = Application.persistentDataPath + "/avatar.jpg";
            try
            {
                image.texture = NativeGallery.LoadImageAtPath(directoryPath);
                //width = image.texture.width;
                //height = image.texture.height;
            }
            catch { image.texture = Resources.Load<Texture2D>("icons/Avatar1"); }
            money = Settings.Money;
        }
        else
            image.texture = Resources.Load<Texture2D>("icons/Avatar1");

        //imgBytes = Convert.ToBase64String((image.texture as Texture2D).GetRawTextureData());

        unAss = (int)PhotonNetwork.CurrentRoom.CustomProperties["C3"];
        MapController.players.Add(this);
        FindObjectOfType<MapController>().ArrangePlayers();
        PhotonNetwork.RaiseEvent((byte)Events.ArrangePlayers, null, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.Serialize(ref isPlayerTurn);
        stream.Serialize(ref isFieldClear);
        stream.Serialize(ref isReadyToStart);
        //stream.Serialize(ref imgBytes);
        stream.Serialize(ref unAss);
        stream.Serialize(ref money);
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
        money = Settings.Money;
        transform.GetChild(3).GetChild(0).GetComponent<Text>().text = $"{money} р";
        //if (width != -1)
        //{
        //    Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        //    tex.LoadRawTextureData(Convert.FromBase64String(imgBytes));
        //    tex.Apply();
        //    image.texture = tex;
        //}

    }
}
