using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HubManager : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
	string sqlLobbyFilter = "";
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
	[SerializeField] private GameObject SearchPlayers;
	[SerializeField] private GameObject SearchCards;
	[SerializeField] private Text SearchBetFrom;
	[SerializeField] private Text SearchBetTo;
	string currentSelection;
	void Update()
	{
		if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))//выход из приложения свайпом вверх
			Application.Quit();
		//if (SearchGameWindow.activeSelf)
		//	PhotonNetwork.GetCustomRoomList(new TypedLobby("myLobby", LobbyType.SqlLobby), sqlLobbyFilter);
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
		int cards = 0;
		for (int i = 0; i < Cards.transform.childCount-1; i++)//получаем выбранное количество карт
			if (Cards.transform.GetChild(i).GetComponent<Toggle>().isOn)
				cards = int.Parse(Cards.transform.GetChild(i).GetChild(1).GetComponent<Text>().text);

		RoomOptions roomOptions = new RoomOptions();//создаём комнату: C0 -- Вид игры, С1 -- Ставка, С2 -- Количетсво карт
		roomOptions.CustomRoomProperties = new Hashtable() { { "C0", currentSelection }, { "C1", int.Parse(Bet.text) }, { "C2", cards } };
		roomOptions.CustomRoomPropertiesForLobby = new string[] { "C0", "C1", "C2" };
		roomOptions.MaxPlayers = byte.Parse(MaxP.text);
		PhotonNetwork.CreateRoom(RoomName.text.Equals("") ? NickName.text : RoomName.text,
								 roomOptions,
								 new TypedLobby("myLobby", LobbyType.SqlLobby));
	}
	public void Join()
	{
		sqlLobbyFilter = "(C0 = \"Raz\" OR C0 = \"Durak\") AND C1 = 100";
		PhotonNetwork.GetCustomRoomList(new TypedLobby("myLobby", LobbyType.SqlLobby), sqlLobbyFilter);
		Debug.Log(sqlLobbyFilter);
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
		Debug.Log(roomList[0]);
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
		string selectedGames = "", selectedPlayers = "", selectedCards = ""; 
		for (int i = 0; i < SearchGames.transform.childCount; i++)
		{
			if (SearchGames.transform.GetChild(i).GetComponent<Toggle>().isOn)
			{
				selectedGames += $"C0 = \"{SearchGames.transform.GetChild(i).name}\" OR ";
			}
		}
		sqlLobbyFilter = selectedGames;
		//sqlLobbyFilter = $"C0 = \"{selectedGame}\" AND C1 = 100";
	}

}
