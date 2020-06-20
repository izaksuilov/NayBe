﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
public class RazManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject playerPref;
    public static bool isBeginningPhase;
    public static string ace;
    void Awake()
    {
        Input.multiTouchEnabled = false;
        isBeginningPhase = true;
        ace = "";
        FindObjectOfType<MapController>().MakePlaceForPlayers();
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 100000;
        PhotonNetwork.Instantiate(playerPref.name, new Vector3(0,0,0), Quaternion.identity);
    }
    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
            Leave();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    public void Leave()
    {
        PhotonNetwork.RaiseEvent((byte)Events.PlayerLeftRoom, null, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, new SendOptions() { Reliability = true });
        PhotonNetwork.LeaveRoom();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

}
