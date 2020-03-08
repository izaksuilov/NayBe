using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private Sprite[] cardsImage;
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject[] playersPlaces;
    private List<PlayerControl> players = new List<PlayerControl>();
    public void AddPlayer(PlayerControl player)
    {
        players.Add(player);
        ArrageObjects();
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)
        {
            int[] data = CreateRandomIds();
            PhotonNetwork.RaiseEvent((byte)Events.CreateCards, data, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
            CreateCards(data);
        }
    }

    private void ArrageObjects()
    {
        var selectedPlayers = (from t in players
                               orderby t.GetComponent<PhotonView>().OwnerActorNr
                               select t).ToArray();
        for (int i = 0; i < selectedPlayers.Length; i++)
        {
            if (selectedPlayers[i].GetComponent<PhotonView>().IsMine)
            {
                for (int j = i; j > 0; j--)
                {
                    var first = selectedPlayers[0];
                    int k;
                    for (k = 0; k < selectedPlayers.Length - 1; k++)
                        selectedPlayers[k] = selectedPlayers[k + 1];
                    selectedPlayers[k] = first;
                }
                goto outer;
            }
        }
      outer:
        for (int i = 0; i < players.Count; i++)
        {
            selectedPlayers[i].transform.parent = playersPlaces[players.Count == 1 ? 0 : players.Count - 2].transform.GetChild(i).transform;
            selectedPlayers[i].transform.localPosition = new Vector3(0, 0, 0);
        }
    }
    private void CreateCards(int[] idx)
    {
        List<int> nums = new List<int>();
        for (int i = 0; i < cardsImage.Length; i++)
            nums.Add(i);
        for (int i = 0; i < cardsImage.Length; i++)
        {
            GameObject card = Instantiate(prefab, new Vector3(i * 100, 0, 0), Quaternion.identity);
            card.GetComponent<SpriteRenderer>().sprite = cardsImage[nums[idx[i]]];
            nums.RemoveAt(idx[i]);
        }
    }
    private void RemovePlayer()
    {
        players.RemoveAt(players.Count - 1);
        PhotonNetwork.RaiseEvent((byte)Events.RemoveCards, null, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, new SendOptions() { Reliability = true });
    }
    private void EndGame()
    {
        foreach (GameObject item in GameObject.FindGameObjectsWithTag("Card"))
            Destroy(item);
    }
    private int[] CreateRandomIds()
    {
        int[] idx = new int[cardsImage.Length];
        for (int i = 0; i < cardsImage.Length; i++)
            idx[i] = UnityEngine.Random.Range(0, cardsImage.Length-i);
        return idx;
    }
    #region События 
    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)Events.CreateCards:
            {
                int[] data = (int[])photonEvent.CustomData;
                CreateCards(data); break;
            }

            case (byte)Events.RemoveCards:
                EndGame(); break;

            case (byte)Events.RemovePlayer:
                RemovePlayer(); break;
        }
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    #endregion
}
