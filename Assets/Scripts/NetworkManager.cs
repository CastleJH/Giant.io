using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject lobbyPanel;
    public GameObject loadingPanel;
    public InputField nicknameInput;

    void Awake()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        
    }

    public void ButtonJoinRoom()
    {
        loadingPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 20 }, null);
    }

    public override void OnJoinedRoom()
    {
        loadingPanel.SetActive(false);
        bool isEmptyNameField = true;
        for (int i = 0; i < nicknameInput.text.Length; i++)
            if (nicknameInput.text[i] != ' ')
            {
                isEmptyNameField = false;
                break;
            }
        if (isEmptyNameField)
        {
            bool exist;
            string newNickName;
            do
            {
                exist = false;
                newNickName = "username" + Random.Range(1000, 9999).ToString(); ;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    if (PhotonNetwork.PlayerList[i].NickName == newNickName)
                    {
                        exist = true;
                        break;
                    }
            } while (exist);
            PhotonNetwork.LocalPlayer.NickName = newNickName;
        }
        else
        {
            bool exist;
            string newNickName;
            int numTag = 1;
            newNickName = nicknameInput.text;
            do
            {
                exist = false;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    if (PhotonNetwork.PlayerList[i].NickName == newNickName)
                    {
                        exist = true;
                        break;
                    }
                newNickName = nicknameInput.text + "_" + numTag.ToString();
                numTag++;
            } while (exist);
            PhotonNetwork.LocalPlayer.NickName = newNickName;
        }
        GameManager.instance.SpawnMyPlayer();
    }

    public void ButtonLeaveRoom()
    {
        loadingPanel.SetActive(true);
        GameManager.instance.deadPanel.SetActive(false);
        GameManager.instance.LeaveThisGame();
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        lobbyPanel.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ButtonLeaveRoom();
    }
}
