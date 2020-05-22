using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] Sprite[] cardsImage;//лицевые стороны карт
    [SerializeField] GameObject cardPrefab;//префаб карты
    [SerializeField] GameObject field;//поле
    [SerializeField] GameObject[] positions;//позиции, на которых распложены игроки и карты
    List<PlayerControl> players = new List<PlayerControl>();
    public static List<Card> allCards;
    private void Awake()
    {
        allCards = new List<Card>();
    }
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
        players.Sort(new PlayerComparer());//сортируем игроков, в обратном порядке очереди
                                           //(так игроки будут ходить по часовой стрелке)
                                           //и ставим нашего игрока первым в массиве
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
                }for (int k = 0; k < players.Count; k++) players[k].localOffset = k;
                goto outer;
            }
        }
    outer:
        List<GameObject> PlayerPositions = 
            FindChildrenWithTag(positions[players.Count == 1 ? 0 : players.Count - 2], "PlayerPosition");
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.SetParent(PlayerPositions[i].transform);
            players[i].transform.localPosition = new Vector3(0, 0, 0);
        }
    }
    /// <summary>
    /// Начать игру
    /// </summary>
    /// <param name="idx">Массив индексов для карт</param>
    private void StartGame(int[] idx)
    {
        //создать нужное количество карт
        for (int i = 0; i < idx.Length; i++)
        {
            GameObject obj = Instantiate(cardPrefab);
            obj.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = cardsImage[idx[i]];
            string[] name = cardsImage[idx[i]].name.Split('_');
            obj.GetComponent<CardScript>().CreateCard(new Card(obj, int.Parse(name[0]), name[1]));
        }
        //положить каждому игроку соответствующее количество UnAss и положить одну карту
        int k = players[0].GetComponent<PhotonView>().OwnerActorNr-1, cardIndex = idx.Length-1;
        List<GameObject> 
            UnAssPositions = FindChildrenWithTag(positions[players.Count == 1 ? 0 : players.Count - 2], "UnAssPosition"),
            HandPositions = FindChildrenWithTag(positions[players.Count == 1 ? 0 : players.Count - 2], "HandPosition");
        int maxCard = -1, maxCardIndex = -1;
        for (int a = 0; a < players.Count; a++, k++, cardIndex--)
        {
            for (int i = 0; i < players[a].unAss; i++, cardIndex--) //unass
                AttachCard(allCards[cardIndex].Obj, UnAssPositions[k % UnAssPositions.Count].transform, rotation: Quaternion.Euler(0, 0, 90));
            AttachCard(allCards[cardIndex].Obj, HandPositions[k % HandPositions.Count].transform);//карта
            //находим самую большую карту, игрок с этой картой начнёт ходить
            if (allCards[cardIndex].Value > maxCard)
            {
                maxCard = allCards[cardIndex].Value;
                maxCardIndex = a;
            }
        }
        //оставшиеся карты положить в центр
        while (cardIndex >= 0)
        {
            AttachCard(allCards[cardIndex].Obj, field.transform);
            cardIndex--;
        }
        players[maxCardIndex].isPlayerTurn = true;
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
        int countOfCards = int.Parse((string)PhotonNetwork.CurrentRoom.CustomProperties["C1"]);
        List<int> list = new List<int>();
        //создаем псевдорандомный массив неповторяющихся чисел
        int[] idx = new int[countOfCards];
        for (int i = 51; i > 51 - countOfCards; i--)
            list.Add(i);
        for (int i = 0; i < countOfCards; i++)
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
    public static List<GameObject> FindChildrenWithTag(GameObject parent, string tag)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.tag == tag)
                children.Add(child.gameObject);
        }
        children.Reverse();
        return children;
    }
    private void AttachCard(GameObject card, Transform parent, Vector3 localPosition = default, Quaternion rotation = default)
    {
        card.transform.SetParent(parent);
        card.transform.localPosition = localPosition;
        card.transform.rotation = rotation;
        card.transform.localScale = new Vector3(1,1,1);
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
/// <summary>
/// Сортировка игроков
/// </summary>
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