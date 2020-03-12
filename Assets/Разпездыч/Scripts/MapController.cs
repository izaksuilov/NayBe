using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private Sprite[] cardsImage;
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject[] playersPositions;
    [SerializeField] private List<GameObject> cards;
    private List<PlayerControl> players = new List<PlayerControl>(); 
    public void AddPlayer(PlayerControl player)
    {
        players.Add(player);
        ArrangePlayers();
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)
        {
            int[] data = CreateRandomIds();
            PhotonNetwork.RaiseEvent((byte)Events.CreateCards, data, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
            CreateCards(data);
        }
    }
    /// <summary>
    /// Расстовляем игроков по местам
    /// </summary>
    private void ArrangePlayers()
    {
        players.Sort(new NameComparer());
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<PhotonView>().IsMine)
            {
                for (int j = i; j > 0; j--)
                {
                    var first = players[0];
                    int k;
                    for (k = 0; k < players.Count - 1; k++)
                        players[k] = players[k + 1];
                    players[k] = first;
                }
                goto outer;
            }
        }
    outer:
        List<GameObject> PlayerPositions = FindChildrenWithTag(playersPositions[players.Count == 1 ? 0 : players.Count - 2], "PlayerPosition");
        for (int i = 0; i <players.Count ; i++)
        {
            players[i].transform.parent = PlayerPositions[players.Count-1-i].transform;
            players[i].transform.localPosition = new Vector3(0, 0, 0);
        }
        if (players.Count == 1)
        {
            players[0].transform.parent = PlayerPositions[1].transform;
            players[0].transform.localPosition = new Vector3(0, 0, 0);
        }
            
    }
    /// <summary>
    /// Начать игру
    /// </summary>
    /// <param name="idx"></param>
    private void CreateCards(int[] idx)
    {
        //создаются рандомные индексы для карт
        List<int> nums = new List<int>();
        for (int i = 0; i < cardsImage.Length; i++)
            nums.Add(i);
        for (int i = 0; i < cardsImage.Length; i++)
        {
            GameObject card = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            card.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = cardsImage[nums[idx[i]]];
            cards.Add(card);
            nums.RemoveAt(idx[i]);
        }
        int k = 0;
        List<GameObject> UnAssPositions = FindChildrenWithTag(playersPositions[players.Count == 1 ? 0 : players.Count - 2], "UnAssPosition");
        foreach (var player in players)
        {
            for (int i = 0; i < player.unAss; i++, k++)
            {
                var card = cards[k];
                card.transform.parent = UnAssPositions[k].transform;
                card.transform.localPosition = new Vector3(0, 0, 0);
                //card.transform.rotation = Quaternion.AngleAxis(90, card.transform.localPosition);
            }
        }

    }
    public void RemovePlayer()
    {
        players.RemoveAt(players.Count - 1);
        EndGame();
        if (players.Count > 0)
            ArrangePlayers();
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
    private List<GameObject> FindChildrenWithTag(GameObject parent, string tag)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.tag == tag)
                children.Add(child.gameObject);
        }
        return children;
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
class NameComparer : IComparer<PlayerControl>
{
    public int Compare(PlayerControl p1, PlayerControl p2)
    {
        if (p1.GetComponent<PhotonView>().OwnerActorNr > p2.GetComponent<PhotonView>().OwnerActorNr)
            return 1;
        else if (p1.GetComponent<PhotonView>().OwnerActorNr < p2.GetComponent<PhotonView>().OwnerActorNr)
            return -1;
        return 0;
    }
}