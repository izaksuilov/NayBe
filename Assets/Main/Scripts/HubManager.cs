using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HubManager : MonoBehaviourPunCallbacks
{
	[SerializeField] private Button CreateButton;
	[SerializeField] private GameObject RazSettings;
	[SerializeField] private GameObject DurakSettings;
	[SerializeField] private Text MaxP;
	[SerializeField] private InputField NickName;
	string currentSelection;
	void Update()
	{
		if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
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
		SelectGame("Raz");
	}
	public override void OnConnectedToMaster()
	{
		CreateButton.interactable = true;
	}
	public void Create()
	{
		if (NickName.text.Equals(""))
		{
			CreateButton.gameObject.transform.GetChild(0).GetComponent<Text>().text = "Введите Никнейм";
			CreateButton.interactable = false;
			return;
		}
		PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = Convert.ToByte(MaxP.text)});
	}
	public override void OnJoinedRoom()
	{
		SceneManager.LoadScene(currentSelection);
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

		DurakSettings.SetActive(false); RazSettings.SetActive(false);
		if (currentSelection.Equals("Raz"))
			RazSettings.SetActive(true);
		else if (currentSelection.Equals("Durak"))
			DurakSettings.SetActive(true);
	}
}
