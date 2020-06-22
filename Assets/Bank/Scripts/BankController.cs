using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BankController : MonoBehaviour
{
    [SerializeField] GameObject playerPref, options, usualGame, field, cardPrefab;
    [SerializeField] Transform cardsField, bankField, aceField;
    [SerializeField] Sprite[] cardsImage;
    [SerializeField] Text money;
    bool isUsualMode;
    int bet, maxPrize;
    string ace = "";
    void Start()
    {
        Input.multiTouchEnabled = false;
        if (Application.platform == RuntimePlatform.Android)
            GameObject.Find("Leave").SetActive(false);

        playerPref.GetComponent<PlayerControl>().enabled = false;
        playerPref.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Settings.nickName;

        RawImage image = playerPref.transform.GetChild(1).GetComponent<RawImage>();
        string directoryPath = Application.persistentDataPath + "/avatar.jpg";
        try { image.texture = NativeGallery.LoadImageAtPath(directoryPath); }
        catch { image.texture = Resources.Load<Texture2D>("icons/Avatar1"); }

        Instantiate(playerPref, GameObject.Find("PlayerPosition").transform);
        Debug.LogError(1);
        Settings.Load();
        UpdateMoney();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
            Leave();
    }
    public void Leave()
    {
        SceneManager.LoadScene(0);
    }
    public void StartGame()
    {
        isUsualMode = usualGame.GetComponent<Toggle>().isOn;
        bet = int.Parse(GameObject.Find("Bet Number").GetComponent<Text>().text);
        options.SetActive(false);
        maxPrize = isUsualMode ? bet * 2 : bet * 5;
        Settings.AddMoney(-bet);
        UpdateMoney(true);

        field.SetActive(true);
        int[] idx = CreateRandomIds();
        for (int i = 0; i < idx.Length; i++)
        {
            Transform parent = cardsField;
            if (i == 0)
                parent = aceField;
            else if (i == 1)
                parent = bankField;
            GameObject obj = Instantiate(cardPrefab, parent);
            obj.GetComponent<BankCardScript>().enabled = true;
            obj.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = cardsImage[idx[i]];
            string[] name = cardsImage[idx[i]].name.Split('_');
            if (i == 0) ace = name[1];
            obj.GetComponent<BankCardScript>().CreateCard(new Card(obj, int.Parse(name[0]), name[1]));
        }
        StartCoroutine(RemoveCard());

    }
    private void UpdateMoney(bool showBet = false)
    {
        money.text = $"У вас {Settings.money} рублей";
        if (showBet) money.text += $"\nСтавка: {bet} рублей";
    }
    IEnumerator RemoveCard()
    {
        if (cardsField.childCount == 0)
        {
            StopCoroutine(RemoveCard());
            goto end;
        }
        Transform card = cardsField.GetChild(cardsField.childCount - 1);
        int randN = card.GetComponent<CardScript>().thisCard.Value > 10 ? UnityEngine.Random.Range(0, 4) : UnityEngine.Random.Range(-4, 0);
        card.transform.position = new Vector3(card.transform.position.x - 25, card.transform.position.y - randN);
        card.transform.rotation = Quaternion.Euler(0, 0, card.transform.rotation.eulerAngles.z - UnityEngine.Random.Range(3.5f, 7));
        if (card.transform.position.x < -250) Destroy(card.gameObject);

        yield return new WaitForSeconds(0.01f);
        StartCoroutine(RemoveCard());
    end:;
    }
    public void ChooseCard()
    {
        StopCoroutine(RemoveCard());
        cardsField.GetChild(cardsField.childCount - 1)
            .GetChild(0).GetChild(1).gameObject.SetActive(true);
        Card selectedCard = cardsField.GetChild(cardsField.childCount - 1).GetComponent<BankCardScript>().thisCard,
             aceCard = field.transform.GetChild(1).GetChild(0).GetComponent<BankCardScript>().thisCard;

        if (aceCard.Suit.Equals(ace))
        {
            if (selectedCard.Suit.Equals(ace) && selectedCard.Value > aceCard.Value)
                PlayerWin();
            else PlayerLost();
        }
        else
        {
            if (selectedCard.Suit.Equals(ace))
                PlayerWin();
            else if (selectedCard.Suit.Equals(aceCard.Suit))
            {
                if (selectedCard.Value > aceCard.Value)
                    PlayerWin();
                else PlayerLost();
            }
            else if (isUsualMode)
                Draw();
            else 
                PlayerLost();
        }
    }
    private void PlayerWin()
    {
        Debug.Log("Win");
    }

    private void PlayerLost()
    {
        Debug.Log("Lost");
    }
    private void Draw()
    {
        Debug.Log("Draw");
    }


    private int[] CreateRandomIds()
    {
        int countOfCards = 52;
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
}
