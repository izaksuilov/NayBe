using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private Sprite[] cardsImage;//лицевые стороны карт
    [SerializeField] private GameObject cardPrefab;//префаб карты
    [SerializeField] private GameObject[] playersPositions;//позиции, на которых распложены игроки и карты
    private List<GameObject> cards = new List<GameObject>();
    private List<PlayerControl> players = new List<PlayerControl>();
    /// <summary>
    /// Добавить игрока в массив игроков
    /// </summary>
    /// <param name="player">Текущий Игрок</param>
    public void AddPlayer(PlayerControl player)
    {
        players.Add(player);//когда добавляется игрок, нужно переместить всех игроков в зависимости от их количества
        ArrangePlayers();
        //если комната полная
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)
        {
            int[] data = CreateRandomIds();//генерируем рандомные числа, отвечающие за лицевые стороны карт
            PhotonNetwork.RaiseEvent((byte)Events.StartGame, data, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
            StartGame(data);//отправляем событие и начинаем игру
        }
    }
    /// <summary>
    /// Расставить игроков по местам
    /// </summary>
    private void ArrangePlayers()
    {
        players.Sort(new PlayerComparer());//сортируем игроков, в в обратном порядке очереди
                                           //(так игроки будут ходить по часовой стрелке)
                                           //и ставим нашего игрока первым в массиве
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<PhotonView>().IsMine)//
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
        List<GameObject> PlayerPositions = 
            FindChildrenWithTag(playersPositions[players.Count == 1 ? 0 : players.Count - 2], "PlayerPosition");
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.parent = PlayerPositions[players.Count - 1 - i].transform;
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
    /// <param name="idx">Массив индексов для карт</param>
    private void StartGame(int[] idx)
    {
        //создать нужное количество карт
        for (int i = 0; i < cardsImage.Length; i++)
        {
            GameObject card = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            card.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = cardsImage[idx[i]];
            cards.Add(card);
        }
        //положить каждому игроку соответствующее количество поджопников
        int k = 0;
        List<GameObject> UnAssPositions = 
            FindChildrenWithTag(playersPositions[players.Count == 1 ? 0 : players.Count - 2], "UnAssPosition");
        foreach (var player in players)
        {
            for (int i = 0; i < player.unAss; i++, k++)
            {
                var card = cards[k];
                card.transform.SetParent(UnAssPositions[k].transform);
                card.transform.localPosition = new Vector3(0, 0, 0);
                player.unAssCards.Add(card);
                //card.transform.rotation = Quaternion.AngleAxis(90, card.transform.localPosition);
            }
        }

    }
    /// <summary>
    /// Убрать игрока из массива игроков
    /// </summary>
    public void RemovePlayer()
    {
        players.RemoveAt(players.Count - 1);
        EndGame();
        if (players.Count > 0)
            ArrangePlayers();
    }
    /// <summary>
    /// Закончить игру
    /// </summary>
    private void EndGame()
    {
        //удаляем все карты
        foreach (GameObject item in GameObject.FindGameObjectsWithTag("Card"))
            Destroy(item);
    }
    /// <summary>
    /// Создать псевдорандомные индексы для карт
    /// </summary>
    private int[] CreateRandomIds()
    {
        List<int> list = new List<int>();
        //создаем псевдорандомный массив неповторяющихся чисел
        int[] idx = new int[cardsImage.Length];
        for (int i = 0; i < cardsImage.Length; i++)
            list.Add(i);
        for (int i = 0; i < cardsImage.Length; i++)
        {
            int randomId = UnityEngine.Random.Range(0, list.Count);
            idx[i] = list[randomId];
            list.RemoveAt(randomId);
        }
        return idx;
    }
    /// <summary>
    /// Находит объект-ребенок в объекте-родителе по тегу
    /// </summary>
    /// <param name="parent">Объект, в котором нужно искать</param>
    /// <param name="tag">Тег объекта-ребенка</param>
    /// <returns>Возвращает List объектов-детей</returns>
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
            case (byte)Events.StartGame:
            {
                int[] data = (int[])photonEvent.CustomData;
                StartGame(data); break;
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
class PlayerComparer : IComparer<PlayerControl>
{
    public int Compare(PlayerControl p1, PlayerControl p2)
    {
        if (p1.GetComponent<PhotonView>().OwnerActorNr > p2.GetComponent<PhotonView>().OwnerActorNr)
            return -1;
        else if (p1.GetComponent<PhotonView>().OwnerActorNr < p2.GetComponent<PhotonView>().OwnerActorNr)
            return 1;
        return 0;
    }
}