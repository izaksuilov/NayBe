using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class HubManager : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    #region Переменные
    public string sqlLobbyFilter = "(C0 = \"Raz\" OR C0 = \"Durak\" OR C0 = \"NayBe\") AND (C1 = \"24\" OR C1 = \"36\" OR C1 = \"52\") AND (C2 >= 100 AND C2 <= 1000000)";
	[SerializeField] GameObject SettingsWindow;
	[SerializeField] GameObject SearchGameWindow;
	[SerializeField] GameObject CreateGameWindow;
	[SerializeField] GameObject RazSettings;
	[SerializeField] GameObject DurakSettings;
	[SerializeField] Text Bet;
	[SerializeField] GameObject Cards;
	[SerializeField] Text UnAss;
	[SerializeField] Text UnAff;
	[SerializeField] Text RoomName;
	[SerializeField] Button CreateButton;
	[SerializeField] Text MaxP;
	[SerializeField] InputField NickName;
	[SerializeField] GameObject SearchGames;
	[SerializeField] GameObject SearchCards;
	[SerializeField] Text SearchBetFrom;
	[SerializeField] Text SearchBetTo;
	[SerializeField] Transform ListOfRooms;
	[SerializeField] GameObject RoomPrefab;
	string currentSelection;
    #endregion
    void Update()
	{
		if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))//выход из приложения свайпом вверх
			Application.Quit();
	}
	void Awake()
	{
		CreateButton.interactable = false;
		Settings.Load();
		#region Network
		PhotonNetwork.NickName = NickName.text;
		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.ConnectUsingSettings();
		#endregion
		SelectWindow("Create");
		SelectGame("Raz");
	}
    public override void OnConnectedToMaster()
	{
		CreateButton.interactable = true;
		StartCoroutine(RefreshRoomList());
	}
	/// <summary>
	/// Создать комнату с выбранными параметрами
	/// </summary>
	public void Create()
	{
		if (NickName.text.Equals(""))//проверяем, что пользователь ввёл ник
		{
			CreateButton.gameObject.transform.GetChild(0).GetComponent<Text>().text = "Введите Никнейм";
			CreateButton.interactable = false;
			return;
		}
		string cards = "";
		for (int i = 0; i < Cards.transform.childCount-1; i++)//получаем выбранное количество карт
			if (Cards.transform.GetChild(i).GetComponent<Toggle>().isOn)
				cards = Cards.transform.GetChild(i).GetChild(1).GetComponent<Text>().text;

		RoomOptions roomOptions = new RoomOptions();//создаём комнату: C0 -- Вид игры, С1 -- Количетсво карт, С2 -- Ставка 
		roomOptions.CustomRoomProperties = new Hashtable() { { "C0", currentSelection }, { "C1", cards }, { "C2", int.Parse(Bet.text) } };
		if (RazSettings.activeSelf)
			roomOptions.CustomRoomProperties.Add("C3", int.Parse(UnAss.text));
		else if (DurakSettings.activeSelf)
			roomOptions.CustomRoomProperties.Add("C3", int.Parse(UnAff.text));
		roomOptions.CustomRoomPropertiesForLobby = new string[] { "C0", "C1", "C2" };
		roomOptions.MaxPlayers = byte.Parse(MaxP.text);
		PhotonNetwork.CreateRoom(RoomName.text.Equals("") ? NickName.text : RoomName.text,
								 roomOptions,
								 new TypedLobby("myLobby", LobbyType.SqlLobby));
	}
	public override void OnJoinedRoom()
	{
		SceneManager.LoadScene(currentSelection);
	}
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		
	}
	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach (Transform child in ListOfRooms.transform)
			Destroy(child.gameObject);
		if (roomList.Count > 0)
		{
			for (int i = 0; i < roomList.Count; i++)
			{
				Instantiate(RoomPrefab, ListOfRooms);
				RoomPrefab.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = roomList[i].Name;
				RoomPrefab.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text 
					= roomList[i].CustomProperties["C0"].ToString();
				RoomPrefab.transform.GetChild(1).GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text
					= roomList[i].CustomProperties["C1"].ToString();
				RoomPrefab.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text
					= roomList[i].CustomProperties["C2"].ToString();
				RoomPrefab.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text
					= $"{roomList[i].PlayerCount} / {roomList[i].MaxPlayers}";
			}
		}
	}
	public override void OnDisconnected(DisconnectCause cause)
	{

	}
	/// <summary>
	/// Открыть нужное окно 
	/// </summary>
	/// <param name="window">Название окна</param>
	public void SelectWindow(string window)
	{
		SettingsWindow.SetActive(false); SearchGameWindow.SetActive(false); CreateGameWindow.SetActive(false);
		if (window.Equals("Settings"))
			SettingsWindow.SetActive(true);
		else if (window.Equals("Search"))
			SearchGameWindow.SetActive(true);
		else if (window.Equals("Create"))
			CreateGameWindow.SetActive(true);
	}
	/// <summary>
	/// Открыть нужную игру 
	/// </summary>
	/// <param name="window">Название игры</param>
	public void SelectGame(string game)
	{
		currentSelection = game;

		DurakSettings.SetActive(false); RazSettings.SetActive(false);
		if (currentSelection.Equals("Raz"))
			RazSettings.SetActive(true);
		else if (currentSelection.Equals("Durak"))
			DurakSettings.SetActive(true);
	}
	/// <summary>
	/// Применить фильтр для поиска комнат
	/// </summary>
	public void ApplyFilter()
	{
		string selectedGames = "(", selectedCards = "(";
		CreateSqlFilter(SearchGames, "C0", ref selectedGames);
		CreateSqlFilter(SearchCards, "C1", ref selectedCards);
		sqlLobbyFilter = $"{selectedGames} AND {selectedCards} " +
			$"AND (C2 >= {int.Parse(SearchBetFrom.text)} AND C2 <= {int.Parse(SearchBetTo.text)})";
		Debug.Log(sqlLobbyFilter);
	}
	void CreateSqlFilter(GameObject obj, string parameter, ref string s)
	{
		for (int i = 0; i < obj.transform.childCount; i++)
			if (obj.transform.GetChild(i).GetComponent<Toggle>().isOn)
				s += $"{parameter} = \"{obj.transform.GetChild(i).name}\" OR ";
		s = s.Remove(s.Length - 4) + ")";
	}
	IEnumerator RefreshRoomList()
	{
		if (SearchGameWindow.activeSelf)
			PhotonNetwork.GetCustomRoomList(new TypedLobby("myLobby", LobbyType.SqlLobby), sqlLobbyFilter);
		yield return new WaitForSeconds(2);
		StartCoroutine(RefreshRoomList());
	}
}