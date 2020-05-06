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
	string sqlLobbyFilter = "(C0 = \"Raz\" OR C0 = \"Durak\" OR C0 = \"NayBe\") AND (C1 = \"24\" OR C1 = \"36\" OR C1 = \"52\") AND (C2 >= 100 AND C2 <= 1000000)";
	[SerializeField] private GameObject SettingsWindow;
	[SerializeField] private GameObject SearchGameWindow;
	[SerializeField] private GameObject CreateGameWindow;
	[SerializeField] private GameObject RazSettings;
	[SerializeField] private GameObject DurakSettings;
	[SerializeField] private Text Bet;
	[SerializeField] private GameObject Cards;
	[SerializeField] private Text RoomName;
	[SerializeField] private Button CreateButton;
	[SerializeField] private Text MaxP;
	[SerializeField] private InputField NickName;
	[SerializeField] private GameObject SearchGames;
	[SerializeField] private GameObject SearchCards;
	[SerializeField] private Text SearchBetFrom;
	[SerializeField] private Text SearchBetTo;
	[SerializeField] private Transform ListOfRooms;
	[SerializeField] private GameObject RoomPrefab;
	string currentSelection;
	void Update()
	{
		if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))//выход из приложения свайпом вверх
			Application.Quit();
		if (SearchGameWindow.activeSelf)
			PhotonNetwork.GetCustomRoomList(new TypedLobby("myLobby", LobbyType.SqlLobby), sqlLobbyFilter);
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
	}
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
		roomOptions.CustomRoomPropertiesForLobby = new string[] { "C0", "C1", "C2" };
		roomOptions.MaxPlayers = byte.Parse(MaxP.text);
		PhotonNetwork.CreateRoom(RoomName.text.Equals("") ? NickName.text : RoomName.text,
								 roomOptions,
								 new TypedLobby("myLobby", LobbyType.SqlLobby));
	}
	public void Join()
	{
		//TypedLobby sqlLobby = new TypedLobby("myLobby", LobbyType.SqlLobby);    // same as above
		//string sqlLobbyFilter = "C0 = 0";   // find a game with mode 0
		//lbClient.OpJoinRandomRoom(null, expectedMaxPlayers, matchmakingMode, sqlLobby, sqlLobbyFilter);
		//PhotonNetwork.JoinRandomRoom();
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
}