using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System;
using System.Collections;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] Sprite[] cardsImage;//лицевые стороны карт
    [SerializeField] GameObject cardPrefab, field, positions, otherPlayer, looserNick;
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
    private void TryStartGame()
    {
        ArrangePlayers();
        PhotonNetwork.RaiseEvent((byte)Events.ArrangePlayers, null, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)//если комната полная
        {
            int[] data = CreateRandomIds();
            PhotonNetwork.RaiseEvent((byte)Events.StartFirstPhase, data, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
            StartFirstPhase(data);
        }
    }
    /// <summary>
    /// Добавить игрока в массив игроков
    /// </summary>
    /// <param name="player">Текущий Игрок</param>
    public void AddPlayer(PlayerControl player)
    {
        players.Add(player);//когда добавляется игрок, нужно переместить всех игроков в зависимости от их количества
        TryStartGame();
    }
    /// <summary>
    /// Расставить игроков по местам
    /// </summary>
    private void ArrangePlayers()
    {
        players.Sort(new PlayerComparer());//сортируем игроков, в обратном порядке очереди
                                           //(так игроки будут ходить по часовой стрелке)
                                           //и ставим нашего игрока первым в массиве
        players =  PlayerComparer.PutMyPlayerFirst(players);
        List<GameObject> PlayerPositions =
            FindChildrenWithTag(positions, "PlayerPosition");
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.SetParent(PlayerPositions[i].transform);
            players[i].transform.localPosition = new Vector3(0, 0, 0);
            players[i].transform.localScale = new Vector3(1, 1, 1);
        }
    }
    /// <summary>
    /// Начать игру
    /// </summary>
    /// <param name="idx">Массив индексов для карт</param>
    private void StartFirstPhase(int[] idx)
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
        players.Sort(new PlayerComparer());
        int cardIndex = idx.Length - 1, maxCard = -1, beginnerPlayer = -1;
        for (int a = 0; a < players.Count; a++, cardIndex--)
        {
            GameObject playerParent = players[a].transform.parent.parent.parent.gameObject;
            for (int i = 0; i < players[a].unAss; i++, cardIndex--) //unass
                AttachCard(allCards[cardIndex].Obj, FindChildrenWithTag(playerParent, "UnAssPosition")[0].transform, rotation: Quaternion.Euler(0, 0, 90));
            AttachCard(allCards[cardIndex].Obj, FindChildrenWithTag(playerParent, "HandPosition")[0].transform);//карта
            if (allCards[cardIndex].Value > maxCard)//находим самую большую карту
            {
                maxCard = allCards[cardIndex].Value;
                beginnerPlayer = players[a].GetComponent<PhotonView>().OwnerActorNr;
            }
        }
        //оставшиеся карты положить в центр
        while (cardIndex >= 0)
        {
            AttachCard(allCards[cardIndex].Obj, field.transform);
            cardIndex--;
        }// игрок с наибольшей картой начнёт ходить
        players = PlayerComparer.PutMyPlayerFirst(players);
        if (players[0].GetComponent<PhotonView>().OwnerActorNr == beginnerPlayer) players[0].isPlayerTurn = true;
    }
    public static void StartSecondPhase(string ace)
    {
        RazManager.isBeginningPhase = false;
        RazManager.ace = ace;
        GameObject aceImage = GameObject.Find("AceImage");
        aceImage.GetComponent<RawImage>().texture = Resources.Load<Texture2D>($"icons/{RazManager.ace}");
        aceImage.GetComponent<RawImage>().color = new Color(1,1,1,1);
        players[0].isPlayerTurn = true;
        PhotonNetwork.RaiseEvent((byte)Events.StartSecondPhase, ace,
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    }
    /// <summary>
    /// Убрать игрока из массива игроков
    /// </summary>
    public static void PlayerWin()
    {
        PhotonNetwork.RaiseEvent((byte)Events.PlayerWin, null, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, new SendOptions() { Reliability = true });
    }
    /// <summary>
    /// Закончить игру
    /// </summary>
    private IEnumerator EndGame(string nickname = "")
    {
        
        yield return new WaitForSeconds(0.005f);
        var cards = FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject, "Card");
        foreach (GameObject item in cards)
        {
            if (item.transform.position.x < -100) { Destroy(item); continue; }
            int randN = item.GetComponent<CardScript>().thisCard.Value > 10 ? UnityEngine.Random.Range(0, 4) : UnityEngine.Random.Range(-4, 0);
            item.transform.position = new Vector3(item.transform.position.x - 5, item.transform.position.y - randN, 1);
            item.transform.rotation = Quaternion.Euler(0, 0, item.transform.rotation.eulerAngles.z - UnityEngine.Random.Range(3.5f, 7.5f));
        }
        if (cards.Count != 0)
            StartCoroutine(EndGame(nickname));
        else
        {
            GameObject.Find("AceImage").GetComponent<RawImage>().color = new Color(1, 1, 1, 0);
            ToggleLayotActive(true);
            foreach (PlayerControl player in players)
                player.isPlayerTurn = false;
            RazManager.isBeginningPhase = true;
            RazManager.ace = "";
            allCards.Clear();
            if (nickname.Length != 0)
            {
                looserNick.SetActive(true);
                looserNick.GetComponent<Text>().text = $"ТЫ проебал, {nickname}";
                yield return new WaitForSeconds(3);
                looserNick.SetActive(false);
            }
            TryStartGame();
        }

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
        int actorN = -1;
        for (int i = players.Count-1; i >= 0; i--)
        {
            if (FindChildrenWithTag(players[i].transform.parent.parent.parent.gameObject, "HandPosition")[0].transform.childCount == 0) continue;
            actorN = players[i].GetComponent<PhotonView>().OwnerActorNr;
            break;
        }
        PhotonNetwork.RaiseEvent((byte)Events.SwitchPlayerTurn, actorN, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    }
    public static void MoveCard(int value, string suit, int actorN = -1)
    {
        object[] data = actorN == -1 ? new object[] {value, suit} : new object[] {value, suit, actorN}; 
        PhotonNetwork.RaiseEvent((byte)Events.MoveCard, data,
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    }
    public static IEnumerator CallClearField()
    {
        yield return new WaitForSeconds(0.4f);
        PhotonNetwork.RaiseEvent((byte)Events.ClearField, null,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All }, new SendOptions() { Reliability = true });
    }
    private IEnumerator ClearField()
    {
        yield return new WaitForSeconds(0.004f);
        foreach (GameObject item in FindChildrenWithTag(field, "Card"))
        {
            if (item.transform.position.x < -470) continue;
            int randN = item.GetComponent<CardScript>().thisCard.Value > 10 ? UnityEngine.Random.Range(0, 4) : UnityEngine.Random.Range(-4, 0);
            item.transform.position = new Vector3(item.transform.position.x - 17, item.transform.position.y - randN);
            item.transform.rotation = Quaternion.Euler(0, 0, item.transform.rotation.eulerAngles.z - UnityEngine.Random.Range(3.5f, 7));
        }
        if (FindChildrenWithTag(field, "Card").Count > 0 && FindChildrenWithTag(field, "Card")[0].transform.position.x > -470)////////////////////////////!!
            StartCoroutine(ClearField());
        else
            foreach (GameObject item in FindChildrenWithTag(field, "Card"))
            {
                //item.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
                //item.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
                //item.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                //item.transform.position = new Vector3(-45, item.transform.position.y);
                //Debug.Log(item.transform.position)
                //item.GetComponent<CardScript>().enabled = false;
                //item.transform.SetParent(FindObjectOfType<Canvas>().transform);
                Destroy(item);
            }
    }

    #region События 
    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)Events.StartFirstPhase:
            {
                int[] data = (int[])photonEvent.CustomData;
                StartFirstPhase(data); break;
            }
            case (byte)Events.StartSecondPhase:
            {
                players[0].isPlayerTurn = false;
                RazManager.isBeginningPhase = false;
                RazManager.ace = (string)photonEvent.CustomData;
                GameObject aceImage = GameObject.Find("AceImage");
                aceImage.GetComponent<RawImage>().texture = Resources.Load<Texture2D>($"icons/{RazManager.ace}");
                aceImage.GetComponent<RawImage>().color = new Color(1, 1, 1, 1);
                break;
            }
            case (byte)Events.ArrangePlayers:
            {
                ArrangePlayers(); break;
            }
            case (byte)Events.PlayerLeftRoom:
            {
                ToggleLayotActive(false);
                StartCoroutine(EndGame());
                break;
            }
            case (byte)Events.PlayerWin:
            {
                int activePlayers = 0, actorN = -1, i;
                for (int k = players.Count - 1; k >= 0; k--)
                {
                    if (FindChildrenWithTag(players[k].transform.parent.parent.parent.gameObject, "HandPosition")[0].transform.childCount != 0)
                    {
                        activePlayers++;
                        actorN = players[k].GetComponent<PhotonView>().OwnerActorNr;
                    }
                }
                    
                if (activePlayers == 1)
                {
                    var array = FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject, "Player");
                    for (i = 0; i < array.Count; i++)
                    {
                        if (array[i].GetComponent<PhotonView>().OwnerActorNr == actorN)
                        {
                            array[i].GetComponent<PlayerControl>().unAss++;
                            ToggleLayotActive(false);
                            StartCoroutine(EndGame(array[i].GetComponent<PhotonView>().Owner.NickName));
                            break;
                        }
                    }

                }
                break;
            }
            case (byte)Events.SwitchPlayerTurn:
            {
                int data = (int)photonEvent.CustomData;
                for (int i = 0; i < players.Count; i++)
                {
                    if(players[i].GetComponent<PhotonView>().OwnerActorNr == data) players[i].isPlayerTurn = true;
                    return;
                }break;
            }
            case (byte)Events.MoveCard:
            {
                object[] data = (object[])photonEvent.CustomData;
                int value = (int)data[0], actorN = -1;
                string suit = (string)data[1];
                if (data.Length == 3) actorN = (int)data[2];
                foreach (GameObject item in FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject, "Card"))
                {
                    if (item.GetComponent<CardScript>().thisCard.Value == value &&
                        item.GetComponent<CardScript>().thisCard.Suit.Equals(suit))
                    {
                        item.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);
                        item.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        if (actorN != -1)
                        {
                            foreach (GameObject player in FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject, "Player"))
                                if (player.GetComponent<PhotonView>().OwnerActorNr == actorN)
                                {
                                    AttachCard(item, FindChildrenWithTag(player.transform.parent.parent.parent.gameObject, "HandPosition")[0].transform);
                                    return;
                                }
                        }
                        else
                        {
                            AttachCard(item, field.transform);
                            return;
                        }
                    }
                }
                break;
            }
            case (byte)Events.ClearField:
            {
                StartCoroutine(ClearField());
                break;
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
    public static void ToggleLayotActive(bool active)
    {
        GameObject.Find("Inner Field").GetComponent<LayoutGroup>().enabled = active;
        foreach (var item in FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject, "UnAssPosition"))
            item.GetComponent<LayoutGroup>().enabled = active;
        foreach (var item in FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject, "HandPosition"))
            item.GetComponent<LayoutGroup>().enabled = active;
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
    public static List<PlayerControl> PutMyPlayerFirst(List<PlayerControl> players)
    {
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
                return players;
            }
        }
        return players;
    }
}
