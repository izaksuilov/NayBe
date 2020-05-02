using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HubManager : MonoBehaviourPunCallbacks
{
	string gameVersion = "1";
	[SerializeField] private Button CreateButton;
	[SerializeField] private Text TitleText;
	[SerializeField] private Text DescriptionText;
	[SerializeField] private GameObject RazSettings;
	[SerializeField] private GameObject DurakSettings;
	[SerializeField] private GameObject NayBeSettings;
	[SerializeField] private Text MaxP;
	struct Data
	{
		public string Title;
		public string Description;
		public string Scene;
	}
	Dictionary<string, Data> _data = new Dictionary<string, Data>();
	string currentSelection;
	void Update()
	{
		if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
	void Awake()
	{
        #region Network
        PhotonNetwork.NickName = "Player " + Random.Range(1, 100);
		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.ConnectUsingSettings();
		PhotonNetwork.GameVersion = gameVersion;
		#endregion
		ButtonsMenuActive(false);
		#region Создаем разделы
		_data.Add(
			"Raz",
			new Data()
			{
				Title = "Бездельник",
				Description = "Тут должно быть описание игры",
				Scene = "Raz"
			}
		);
		_data.Add(
			"Durak",
			new Data()
			{
				Title = "Дурак",
				Description = "Мне нужно будет поменять дизайн игры и сделать его ориентированным на телефоны",
				Scene = "Durak"
			}
		);
		_data.Add(
			"NayBe",
			new Data()
			{
				Title = "Подтетерь соседа",
				Description = "Но я пока еще не придумал, как лучше сделать",
				Scene = "NayBe"
			}
		);

		#endregion
		SelectGame("Raz");
	}
	private void OnLevelWasLoaded()
	{
		ButtonsMenuActive(false);
	}
	public override void OnConnectedToMaster()
	{
		ButtonsMenuActive(true);
	}
	public void Create()
	{
		PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = Convert.ToByte(MaxP.text)});
	}
	public override void OnJoinedRoom()
	{
		SceneManager.LoadScene(_data[currentSelection].Scene);
	}
	public void Join()
	{
		PhotonNetwork.JoinRandomRoom();
	}
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		
	}
	public override void OnDisconnected(DisconnectCause cause)
	{

	}
	public void SelectGame(string Game)
	{
		currentSelection = Game;

		TitleText.text = _data[currentSelection].Title;
		DescriptionText.text = _data[currentSelection].Description;

		DurakSettings.SetActive(false); RazSettings.SetActive(false); NayBeSettings.SetActive(false);
		if (currentSelection.Equals("Raz"))
			RazSettings.SetActive(true);
		else if (currentSelection.Equals("Durak"))
			DurakSettings.SetActive(true);
		else if (currentSelection.Equals("NayBe"))
			NayBeSettings.SetActive(true);
	}
	public void ButtonsMenuActive(bool interactable)
	{
		CreateButton.interactable = interactable;
	}
}
