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
    [SerializeField] Text money, lvl;
    bool isUsualMode;
    int bet, prize, iterator = 0;
    string ace = "";
    void Start()
    {
        Input.multiTouchEnabled = false;
        if (Application.platform == RuntimePlatform.Android)
            GameObject.Find("Leave").SetActive(false);

        playerPref.GetComponent<PlayerControl>().enabled = false;
        playerPref.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Settings.NickName;

        RawImage image = playerPref.transform.GetChild(1).GetComponent<RawImage>();
        string directoryPath = Application.persistentDataPath + "/avatar.jpg";
        try { image.texture = NativeGallery.LoadImageAtPath(directoryPath); }
        catch { image.texture = Resources.Load<Texture2D>("icons/Avatar1"); }

        Instantiate(playerPref, GameObject.Find("PlayerPosition").transform);
        UpdateText();
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
        prize = isUsualMode ? bet * 2 : bet * 3;
        Settings.Money -= bet;
        UpdateText(true);
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
    private void UpdateText(bool showBet = false, string text = "")
    {
        if (text.Length > 0)
        {
            money.text = text;
            return;
        }
        lvl.text = $"Lvl {Settings.Lvl} ({Settings.Progress}/{Settings.NextLvlExp})";
        money.text = $"У вас {Settings.Money} рублей";
        if (showBet) money.text += $"\nСтавка: {bet} рублей";
    }
    IEnumerator RemoveCard()
    {
        if (iterator == 0)
        {
            UpdateText(text: "Старт через 3...");
            yield return new WaitForSeconds(0.5f);
            UpdateText(text: "Старт через 2...");
            yield return new WaitForSeconds(0.5f);
            UpdateText(text: "Старт через 1...");
            yield return new WaitForSeconds(0.5f);
            UpdateText(text: "Поехали!");
            yield return new WaitForSeconds(0.5f);
            UpdateText(true);
            iterator++;
        }
        if (cardsField.childCount == 0)
        {
            StopCoroutine(RemoveCard());
            PlayerLost();
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
        StopAllCoroutines();
        cardsField.GetChild(cardsField.childCount - 1)
            .GetChild(0).GetChild(1).gameObject.SetActive(true);

        Transform card = cardsField.GetChild(cardsField.childCount - 1);
        card.position = new Vector3(0, 0, 0);
        card.rotation = Quaternion.Euler(0, 0, 0);
        cardsField.GetComponent<VerticalLayoutGroup>().enabled = false;
        cardsField.GetComponent<VerticalLayoutGroup>().enabled = true;
        Card selectedCard = card.GetComponent<BankCardScript>().thisCard,
             bankCard = bankField.GetChild(0).GetComponent<BankCardScript>().thisCard;
        if ((((bankCard.Suit.Equals("pik") ^ selectedCard.Suit.Equals("pik")) &&
                !ace.Equals("pik")) || // если одна из карт -- пики и пики -- не козырь
                (!bankCard.Suit.Equals(selectedCard.Suit) && // если карты разных не козырных мастей
                !bankCard.Suit.Equals(ace) &&
                !selectedCard.Suit.Equals(ace))) && isUsualMode) 
        { Draw(); return; }

        else if (bankCard.Suit.Equals("pik") && !selectedCard.Suit.Equals("pik"))
        { PlayerLost(); return; }

        else if (selectedCard.Value > bankCard.Value)
        {
            if (!selectedCard.Suit.Equals(bankCard.Suit) && !selectedCard.Suit.Equals(ace))
            { PlayerLost(); return; }
        }
        else
        {
            if (bankCard.Suit.Equals(ace) ||
              (!bankCard.Suit.Equals(ace) && !selectedCard.Suit.Equals(ace)))
            { PlayerLost(); return; }
        }
        PlayerWin();
    }
    private void PlayerWin()
    {
        Settings.Money += prize;
        Settings.Progress += isUsualMode ? Settings.Step : Settings.Step * 2;
        UpdateText(text: $"Победа!\nВы выиграли {prize} рублей!");
        StartCoroutine(RepeatGame());
    }

    private void PlayerLost()
    {
        UpdateText(text: $"Поражение!\nВы проиграли {bet} рублей!");
        StartCoroutine(RepeatGame());
    }
    private void Draw()
    {
        Settings.Money += bet/2;
        Settings.Progress += Settings.Step / 2;
        UpdateText(text: $"Ничья!\nВы проиграли {bet/2} рублей!");
        StartCoroutine(RepeatGame());
    }
    private IEnumerator RepeatGame()
    {
        yield return new WaitForSeconds(2);
        foreach (var card in MapController.FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject, "Card"))
            Destroy(card);
        Settings.Load();
        UpdateText();
        field.SetActive(false);
        options.SetActive(true);
        FindObjectOfType<SliderStep>().ChangeNumber();
        iterator = 0;
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
