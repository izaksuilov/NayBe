using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] Sprite[] cardsImage;//лицевые стороны карт
    [SerializeField] GameObject cardPrefab, field, positions, otherPlayer;
    public static List<PlayerControl> players { get; private set; }
    public static List<Card> allCards;
    public static int minCard { get; private set; } = 0;
    private void Awake()
    {
        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers - 2; i++)
            Instantiate(otherPlayer, otherPlayer.transform.parent);//cоздать нужное количество мест для игроков
        players = new List<PlayerControl>();
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
            players[i].roomPosition = players.Count - 1 - i;
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
        List<GameObject> PlayerPositions =
            FindChildrenWithTag(positions, "PlayerPosition");
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
        switch (allCards.Count)
        {
            case 24: minCard = 9; break;
            case 36: minCard = 6; break;
            case 52: minCard = 2; break;
        }
        //положить каждому игроку соответствующее количество UnAss и положить одну карту
        int k = players[0].roomPosition, cardIndex = idx.Length - 1;
        List<GameObject>
            UnAssPositions = FindChildrenWithTag(positions, "UnAssPosition"),
            HandPositions = FindChildrenWithTag(positions, "HandPosition");
        int maxCard = -1, maxCardIndex = -1;
        for (int a = 0; a < players.Count; a++, k++, cardIndex--)
        {
            for (int i = 0; i < players[a].unAss; i++, cardIndex--) //unass
                AttachCard(allCards[cardIndex].Obj, UnAssPositions[k % UnAssPositions.Count].transform, rotation: Quaternion.Euler(0, 0, 90));
            AttachCard(allCards[cardIndex].Obj, HandPositions[k % HandPositions.Count].transform);//карта
            //находим самую большую карту
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
        }// игрок с наибольшей картой начнёт ходить
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<PhotonView>().OwnerActorNr == maxCardIndex+1)
            {
                players[i].isPlayerTurn = true;
                break;
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
        allCards.Clear();
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
    /// Присоединить карту к объекту-родителю
    /// </summary>
    private void AttachCard(GameObject card, Transform parent, Vector3 localPosition = default, Quaternion rotation = default)
    {
        card.transform.SetParent(parent);
        card.transform.localPosition = localPosition;
        card.transform.rotation = rotation;
        card.transform.localScale = new Vector3(1, 1, 1);
    }
    public static void SwitchPlayerTurn()
    {
        players[0].isPlayerTurn = false;
        PhotonNetwork.RaiseEvent((byte)Events.SwitchPlayerTurn, players[1].GetComponent<PhotonView>().OwnerActorNr,
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    }
    //public static void MoveCard(int value, int suit, int typeField)
    //{
    //    PhotonNetwork.RaiseEvent((byte)Events.SwitchPlayerTurn, ,
    //        new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    //}
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
            case (byte)Events.PlayerLeftRoom:
            {
                RemovePlayer(); break;
            }
            case (byte)Events.SwitchPlayerTurn:
            {
                int data = (int)photonEvent.CustomData;
                for (int i = 0; i < players.Count; i++)
                {
                    if(players[i].GetComponent<PhotonView>().OwnerActorNr == data) players[i].isPlayerTurn = true;
                    break;
                }break;
            }
            case (byte)Events.MoveCard:
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
