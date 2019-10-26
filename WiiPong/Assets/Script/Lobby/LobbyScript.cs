using System.Collections;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyScript : MonoBehaviourPunCallbacks
{

    #region TopPanel

    private readonly string connectionStatusMessage = "    Connection Status: ";

    [Header("UI References")]
    public TextMeshProUGUI ConnectionStatusText;
    
    public void Update()
    {
        ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState + " " + PhotonNetwork.CountOfPlayers;
    }
    #endregion

    [Header("Login Panel")]
    [SerializeField] private GameObject loginPanel;

    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Selection Panel")]
    [SerializeField] private GameObject selectionPanel;

    [Header("Create Room Panel")]
    [SerializeField] private GameObject createRoomPanel;

    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private TMP_InputField maxPlayersInputField;

    [Header("Join Random Room Panel")]
    [SerializeField] private GameObject joinRandomRoomPanel;


    [Header("Room List Panel")]
    [SerializeField] private GameObject roomListPanel;

    [SerializeField] private GameObject roomListContent;
    [SerializeField] private GameObject roomListEntryPrefab;

    [Header("Inside Room Panel")]
    [SerializeField] private GameObject insideRoomPanel;

    [SerializeField] private GameObject playerListContent;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject playerListEntryPrefab;

    [SerializeField] private string levelName;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();
        playerNameInput.text = "Player " + Random.Range(100, 1000); //MYTODO start player name
    }


    public override void OnConnectedToMaster()
    {
        SetActivePanel(selectionPanel.name);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();

        ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed : " + message);
        SetActivePanel(selectionPanel.name);
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed : " + message);
        SetActivePanel(selectionPanel.name);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed : " + message);
        string roomName = "Room " + Random.Range(1000, 10000); //MyToDo name create random room

        RoomOptions options = new RoomOptions { MaxPlayers = 8}; //MYTODO maxplayer create random room

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        SetActivePanel(insideRoomPanel.name);

        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerListEntryPrefab);
            entry.transform.SetParent(playerListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerListEntry>().Initialize(player.ActorNumber, player.NickName);

            object isPlayerReady;
            if (player.CustomProperties.TryGetValue("IsPlayerReady", out isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }

            playerListEntries.Add(player.ActorNumber, entry);
        }

        startGameButton.gameObject.SetActive(CheckPlayersReady());

        Hashtable props = new Hashtable
        {
            {"PlayerLoadedLevel", false}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
    
    public override void OnLeftRoom()
    {
        SetActivePanel(selectionPanel.name);

        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(playerListEntryPrefab);
        entry.transform.SetParent(playerListContent.transform);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);

        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);

        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            startGameButton.gameObject.SetActive(CheckPlayersReady());
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            object isPlayerReady;
            if (changedProps.TryGetValue("IsPlayerReady", out isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }
        }

        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        SetActivePanel(selectionPanel.name);
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;
        if (roomName.Equals(""))
        {
            roomName = "Room : " + Random.Range(0, 10);
        }
        int maxPlayers = -1;
        maxPlayers = int.Parse(maxPlayersInputField.text);
        if (maxPlayers < 0)
        {
            maxPlayers = 8;
            return;
        }
        
        RoomOptions options = new RoomOptions { MaxPlayers = (byte)maxPlayers};

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        SetActivePanel(joinRandomRoomPanel.name);

        PhotonNetwork.JoinRandomRoom();
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;

        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }
    
    public void OnRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        SetActivePanel(roomListPanel.name);
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("PongScene2");
        }
        else
        {
            PhotonNetwork.LoadLevel("PongSceneBR");
        }
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(roomListEntryPrefab);
            entry.transform.SetParent(roomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListEntry>().Initialize(info.Name, info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }


    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        roomListEntries.Clear();
    }


    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            return false;
        }
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (player.CustomProperties.TryGetValue("IsPlayerReady", out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public void SetActivePanel(string activePanel)
    {
        loginPanel.SetActive(activePanel.Equals(loginPanel.name));
        selectionPanel.SetActive(activePanel.Equals(selectionPanel.name));
        createRoomPanel.SetActive(activePanel.Equals(createRoomPanel.name));
        joinRandomRoomPanel.SetActive(activePanel.Equals(joinRandomRoomPanel.name));
        roomListPanel.SetActive(activePanel.Equals(roomListPanel.name));    // UI should call OnRoomListButtonClicked() to activate this
        insideRoomPanel.SetActive(activePanel.Equals(insideRoomPanel.name));
    }

    public void LocalPlayerPropertiesUpdated()
    {
        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }
}
