﻿using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public struct Card
{
    public GameObject obj;
    public int value;
    public string suit;
    public Card(GameObject _obj, int _value, string _suit)
    {
        obj = _obj;
        value = _value;
        suit = _suit;
    }
}
public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] Sprite[] cardsImage;//лицевые стороны карт
    [SerializeField] GameObject cardPrefab;//префаб карты
    [SerializeField] GameObject[] positions;//позиции, на которых распложены игроки и карты
    List<Card> cards = new List<Card>();
    List<PlayerControl> players = new List<PlayerControl>();
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
            FindChildrenWithTag(positions[players.Count == 1 ? 0 : players.Count - 2], "PlayerPosition");
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.SetParent(PlayerPositions[players.Count - 1 - i].transform);
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
            GameObject obj = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = cardsImage[idx[i]];
            string[] name = cardsImage[idx[i]].name.Split('_');
            cards.Add(new Card(obj, int.Parse(name[0]), name[1]));
        }
        //положить каждому игроку соответствующее количество UnAss
        int k = 0;
        List<GameObject> UnAssPositions =
            FindChildrenWithTag(positions[players.Count == 1 ? 0 : players.Count - 2], "UnAssPosition");
        foreach (var player in players)
        {
            for (int i = 0; i < player.unAss; i++)
            {
                var card = cards[i].obj;
                card.transform.SetParent(UnAssPositions[k].transform);
                card.transform.localPosition = new Vector3(0, 0, 0);
                card.transform.rotation = Quaternion.Euler(0, 0, 90);
                card.transform.localScale = new Vector3(1, 1, 1);
            }k++;
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
        int countOfCards = int.Parse((string)PhotonNetwork.CurrentRoom.CustomProperties["C1"]);
        List<int> list = new List<int>();
        //создаем псевдорандомный массив неповторяющихся чисел
        int[] idx = new int[countOfCards];
        for (int i = 51; i >= 51 - countOfCards; i--)
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