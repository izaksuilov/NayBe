﻿using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Collections;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] Sprite[] cardsImage;//лицевые стороны карт
    [SerializeField] GameObject cardPrefab, field, positions, otherPlayer;
    static GameObject canvas;
    static PlayerControl myPlayer;
    static bool isPlaying = false;
    public static List<PlayerControl> players { get; private set; }
    public static List<Card> allCards;
    public static int minCard { get; private set; } = 0;
    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>().gameObject;
        players = new List<PlayerControl>();
        allCards = new List<Card>();
    }
    private void Update()
    {
        // "синхронизация игрового процесса" 
        foreach (var player in players)
            if (!player.isFieldClear) goto skipEnablefield;
        PhotonNetwork.RaiseEvent((byte)Events.EnableField, null,
           new RaiseEventOptions() { Receivers = ReceiverGroup.All }, new SendOptions() { Reliability = true });
    skipEnablefield:

        if (players.Count != PhotonNetwork.CurrentRoom.MaxPlayers) return;

        if (players.Count != PhotonNetwork.PlayerList.Length && isPlaying)
        {
            Settings.Money += (int)PhotonNetwork.CurrentRoom.CustomProperties["C2"];
            SetLayoutActive(false);
            isPlaying = false;
            StartCoroutine(EndGame());
        }
        foreach (var player in players)
            if (!player.isReadyToStart)
                return;

        ArrangePlayers();
        if (players.Count == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)//если комната полная
        {
            int[] data = CreateRandomIds();
            PhotonNetwork.RaiseEvent((byte)Events.StartFirstPhase, data, 
                new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
            StartFirstPhase(data);
            myPlayer.isReadyToStart = false;    
        }
    }

    /// <summary>
    /// Начать первую фазу игры (раздача карт)
    /// </summary>
    private void StartFirstPhase(int[] idx)
    {
        isPlaying = true;
        //вычесть у всех деньги, чтобы, даже если игрок выйдет, у него снялись деньги
        Settings.Money -= (int)PhotonNetwork.CurrentRoom.CustomProperties["C2"];
        myPlayer.isReadyToStart = false;
        //создать нужное количество карт
        for (int i = 0; i < idx.Length; i++)
        {
            GameObject obj = Instantiate(cardPrefab);
            obj.GetComponent<CardScript>().enabled = true;
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
        if (myPlayer.GetComponent<PhotonView>().OwnerActorNr == beginnerPlayer) myPlayer.isPlayerTurn = true;
    }

    /// <summary>
    /// Начать вторую фазу игры (непосредтсвенно игра)
    /// </summary>
    public static void StartSecondPhase(string ace)
    {
        RazManager.isBeginningPhase = false;
        RazManager.ace = ace;
        GameObject aceImage = GameObject.Find("AceImage");
        aceImage.GetComponent<RawImage>().texture = Resources.Load<Texture2D>($"icons/{RazManager.ace}");
        aceImage.GetComponent<RawImage>().color = new Color(1, 1, 1, 1);
        myPlayer.isPlayerTurn = true;
        PhotonNetwork.RaiseEvent((byte)Events.StartSecondPhase, ace,
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    }

    /// <summary>
    /// Закончить игру
    /// </summary>
    private IEnumerator EndGame(string nickname = "")
    {
        isPlaying = false;
        yield return new WaitForSeconds(0.004f);
        var cards = FindChildrenWithTag(canvas, "Card");
        foreach (GameObject card in cards)
        {
            if (card.transform.position.x < -470) { Destroy(card); continue; }
            int randN = card.GetComponent<CardScript>().thisCard.Value > 10 ? Random.Range(0, 4) : Random.Range(-4, 0);
            card.transform.position = new Vector3(card.transform.position.x - 5, card.transform.position.y - randN, 1);
            card.transform.rotation = Quaternion.Euler(0, 0, card.transform.rotation.eulerAngles.z - Random.Range(0, 10));
        }
        if (cards.Count != 0)
            StartCoroutine(EndGame(nickname));
        else
        {
            GameObject.Find("AceImage").GetComponent<RawImage>().color = new Color(1, 1, 1, 0);
            SetLayoutActive(true);
            RazManager.isBeginningPhase = true;
            RazManager.ace = "";
            allCards.Clear();
            players.Clear();
            if (nickname.Length != 0)
                Notification.Show($"Проиграл {nickname}", 3f);
            foreach (var player in FindChildrenWithTag(canvas, "Player"))
                players.Add(player.GetComponent<PlayerControl>());
            myPlayer.isPlayerTurn = false;
            myPlayer.isReadyToStart = true;
        }
    }

    /// <summary>
    /// Создать нужное количество мест для игроков
    /// </summary>
    public void MakePlaceForPlayers()
    {
        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers - 2; i++)
            Instantiate(otherPlayer, otherPlayer.transform.parent);
    }

    /// <summary>
    /// Расставить игроков по местам
    /// </summary>
    public void ArrangePlayers()
    {
        players.Sort(new PlayerComparer());//сортируем игроков, в обратном порядке очереди
                                           //(так игроки будут ходить по часовой стрелке)
                                           //и ставим нашего игрока первым в массиве
        players.Reverse();  
        players = PlayerComparer.PutMyPlayerFirst(players);
        List<GameObject> PlayerPositions =
            FindChildrenWithTag(positions, "PlayerPosition");
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.SetParent(PlayerPositions[i].transform);
            players[i].transform.localPosition = new Vector3(0, 0, 0);
            players[i].transform.localScale = new Vector3(1, 1, 1);
        }
        players.Sort(new PlayerComparer());
        players = PlayerComparer.PutMyPlayerFirst(players);
        myPlayer = players[0];
    }

    /// <summary>
    /// Убрать выигравшего игрока из массива игроков
    /// </summary>
    public static void PlayerWin()
    {
        PhotonNetwork.RaiseEvent((byte)Events.PlayerWin, myPlayer.GetComponent<PhotonView>().OwnerActorNr,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All }, new SendOptions() { Reliability = true });
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

    /// <summary>
    /// Передать ход следующему игроку
    /// </summary>
    public static void SwitchPlayerTurn()
    {
        myPlayer.isPlayerTurn = false;
        PhotonNetwork.RaiseEvent((byte)Events.SwitchPlayerTurn, players[1].GetComponent<PhotonView>().OwnerActorNr,
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    }

    /// <summary>
    /// Переместить карту
    /// </summary>
    public static void MoveCard(int value, string suit, int actorN = -1)
    {
        object[] data = actorN == -1 ? new object[] {value, suit} : new object[] {value, suit, actorN}; 
        PhotonNetwork.RaiseEvent((byte)Events.MoveCard, data,
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions() { Reliability = true });
    }

    /// <summary>
    /// Начать убирать карты в бито
    /// </summary>
    public static IEnumerator CallClearField()
    {
        myPlayer.isFieldClear = false;
        GameObject.Find("Inner Field").GetComponent<DropPlaceScript>().enabled = false;
        yield return new WaitForSeconds(0.4f);
        PhotonNetwork.RaiseEvent((byte)Events.ClearField, null,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All }, new SendOptions() { Reliability = true });
    }

    /// <summary>
    /// Убрать карты в бито
    /// </summary>
    private IEnumerator ClearField()
    {
        yield return new WaitForSeconds(0.004f);
        foreach (GameObject card in FindChildrenWithTag(field, "Card"))
        {
            if (card.transform.position.x < -470) continue;
            int randN = card.GetComponent<CardScript>().thisCard.Value > 10 ? Random.Range(0, 4) : Random.Range(-4, 0);
            card.transform.position = new Vector3(card.transform.position.x - 10, card.transform.position.y - randN);
            card.transform.rotation = Quaternion.Euler(0, 0, card.transform.rotation.eulerAngles.z - Random.Range(3.5f, 7));
        }
        if (FindChildrenWithTag(field, "Card").Count > 0 && FindChildrenWithTag(field, "Card")[0].transform.position.x > -470)////////////////////////////!!
            StartCoroutine(ClearField());
        else
        {
            foreach (GameObject card in FindChildrenWithTag(field, "Card"))
            {
                //card.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
                //card.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
                //card.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                //card.transform.position = new Vector3(-45, card.transform.position.y);
                //Debug.Log(card.transform.position)
                //card.GetComponent<CardScript>().enabled = false;
                //card.transform.SetParent(FindObjectOfType<Canvas>().transform);
                Destroy(card);
            }
            myPlayer.isFieldClear = true;
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
                RazManager.isBeginningPhase = false;
                RazManager.ace = (string)photonEvent.CustomData;
                myPlayer.isPlayerTurn = false;
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
                Settings.Money += (int)PhotonNetwork.CurrentRoom.CustomProperties["C2"];
                SetLayoutActive(false);
                StartCoroutine(EndGame());
                break;
            }
            case (byte)Events.PlayerWin:
            {
                int acotrN = (int)photonEvent.CustomData;
                for (int i = 0; i < players.Count; i++)
                    if (players[i].GetComponent<PhotonView>().OwnerActorNr == acotrN)
                    {
                        players[i].isReadyToStart = false;
                        if (players[i].GetComponent<PhotonView>().IsMine)
                        {
                            int maxP = PhotonNetwork.CurrentRoom.MaxPlayers,
                                bet = (int)PhotonNetwork.CurrentRoom.CustomProperties["C2"];
                            if (players.Count == maxP)
                                Settings.Money += maxP % 2 != 0 ? bet / maxP * (maxP / 2 + 1) : bet / 2;
                            else
                            {
                                bet -= bet / (maxP * (maxP / 2 + 1));
                                Settings.Money += bet / (2 * (maxP - players.Count));
                            }
                            Settings.Progress += Settings.Step * players.Count / maxP;
                        }
                        players.RemoveAt(i);
                        break;
                    }
                if (players.Count == 1)
                {
                    players[0].unAss++;
                    SetLayoutActive(false);
                    StartCoroutine(EndGame(players[0].GetComponent<PhotonView>().Owner.NickName));
                    if (players[0].GetComponent<PhotonView>().IsMine &&
                        Settings.Money < (int)PhotonNetwork.CurrentRoom.CustomProperties["C2"])
                        PhotonNetwork.LeaveRoom();
                }
                break;
            }
            case (byte)Events.SwitchPlayerTurn:
            {
                int actorN = (int)photonEvent.CustomData;
                if (myPlayer.GetComponent<PhotonView>().OwnerActorNr == actorN)
                    myPlayer.isPlayerTurn = true;
                break;
            }
            case (byte)Events.MoveCard:
            {
                object[] data = (object[])photonEvent.CustomData;
                int value = (int)data[0], actorN = -1;
                string suit = (string)data[1];
                if (data.Length == 3) actorN = (int)data[2];
                foreach (GameObject card in FindChildrenWithTag(canvas, "Card"))
                {
                    if (card.GetComponent<CardScript>().thisCard.Value == value &&
                        card.GetComponent<CardScript>().thisCard.Suit.Equals(suit))
                    {
                        card.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);
                        card.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        if (actorN != -1)
                        {
                            foreach (var player in players)
                                if (player.GetComponent<PhotonView>().OwnerActorNr == actorN)
                                {
                                    AttachCard(card, 
                                        FindChildrenWithTag(player.transform.parent.parent.parent.gameObject,
                                            "HandPosition")[0].transform);
                                    return;
                                }
                        }
                        else
                        {
                            AttachCard(card, field.transform);
                            return;
                        }
                    }
                }
                break;
            }
            case (byte)Events.ClearField:
            {
                field.GetComponent<DropPlaceScript>().enabled = false;
                Color color = field.GetComponent<Image>().color;
                field.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.3f);
                foreach (var player in players)
                    player.isFieldClear = false;
                StartCoroutine(ClearField());
                break;
            }
            case (byte)Events.EnableField:
            {
                field.GetComponent<DropPlaceScript>().enabled = true;
                Color color = field.GetComponent<Image>().color;
                field.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1);
                myPlayer.isFieldClear = false;
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
    public static List<GameObject> FindChildrenWithTag(GameObject parent, string tag = "")
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (tag.Equals(""))
                children.Add(child.gameObject);
            else if (child.tag == tag)
                children.Add(child.gameObject);
        }
        children.Reverse();
        return children;
    }

    /// <summary>
    /// Создать псевдорандомные индексы для карт (перемешать колоду)
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
    /// Включить/Выключить Layout для анимации
    /// </summary>
    public static void SetLayoutActive(bool active)
    {
        GameObject.Find("Inner Field").GetComponent<LayoutGroup>().enabled = active;
        foreach (var item in FindChildrenWithTag(canvas, "UnAssPosition"))
            item.GetComponent<LayoutGroup>().enabled = active;
        foreach (var item in FindChildrenWithTag(canvas, "HandPosition"))
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
