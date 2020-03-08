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
	[SerializeField] private Button JoinButton;
	[SerializeField] private Text TitleText;
	[SerializeField] private GameObject TopPanel;
	[SerializeField] private Text DescriptionText;
	[SerializeField] private GameObject CreateRoomButton;
	[SerializeField] private GameObject JoinRoomButton;
	[SerializeField] private GameObject DurakPanel;
	[SerializeField] private GameObject RazPanel;
	[SerializeField] private Text MaxP;
	struct Data
	{
		public string Title;
		public string Description;
		public string Scene;
	}

	Dictionary<string, Data> _data = new Dictionary<string, Data>();

	string currentSelection;

	// Use this for initialization
	void Awake()
	{
        #region Network
        PhotonNetwork.NickName = "Player " + Random.Range(1, 100);
		PhotonNetwork.AutomaticallySyncScene = true;
		if (PhotonNetwork.IsConnected)
			PhotonNetwork.JoinRandomRoom();
		else
		{
			PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.GameVersion = gameVersion;
		}
		#endregion
		ButtonsMenuActive(false);
		// Setup data
		_data.Add(
			"Durak",
			new Data()
			{
				Title = "Дурак",
				Description = "Это типо описание какое-нибудь я хз чо ту  можно написать и нужно ли оно вообще",
				Scene = "Durak"
			}
		);
		_data.Add(
			"Naybe",
			new Data()
			{
				Title = "Наеби",
				Description = "Это игра объеби соседство",
				Scene = "Naybe"
			}
		);
		_data.Add(
			"Raz",
			new Data()
			{
				Title = "Разпездыч",
				Description = "Это разпездыч",
				Scene = "Raz"
			}
		);
		Select("Durak");
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
	public void Join()
	{
		PhotonNetwork.JoinRandomRoom();
	}
	public override void OnJoinedRoom()
	{
		if (!string.IsNullOrEmpty(currentSelection))
			SceneManager.LoadScene(_data[currentSelection].Scene);
	}
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		
	}
	public override void OnDisconnected(DisconnectCause cause)
	{

	}
	public void Select(string Reference)
	{
		currentSelection = Reference;

		TitleText.text = _data[currentSelection].Title;
		DescriptionText.text = _data[currentSelection].Description;

		TopPanel.SetActive(!string.IsNullOrEmpty(_data[currentSelection].Scene));
		CreateRoomButton.SetActive(!string.IsNullOrEmpty(_data[currentSelection].Scene));
		JoinRoomButton.SetActive(!string.IsNullOrEmpty(_data[currentSelection].Scene));

		DurakPanel.SetActive(false); RazPanel.SetActive(false);
		if (currentSelection.Equals("Durak"))
			DurakPanel.SetActive(!string.IsNullOrEmpty(_data[currentSelection].Scene));
		else if (currentSelection.Equals("Raz"))
			RazPanel.SetActive(!string.IsNullOrEmpty(_data[currentSelection].Scene));
	}
	public void ButtonsMenuActive(bool interactable)
	{
		CreateButton.interactable = interactable;
		JoinButton.interactable = interactable;
	}
}
