using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI roomPlayersText;

    private string roomName;

    public void Initialize(string name, int currentPlayers, int maxPlayers)
    {
        roomName = name;

        roomNameText.text = name;
        roomPlayersText.text = currentPlayers + " / " + maxPlayers;
    }

    public void OnJoinRoomButtonClick()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(roomName);
    }
}
