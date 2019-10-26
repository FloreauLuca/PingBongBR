using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PingBongGameManager : MonoBehaviourPunCallbacks
{
    public static PingBongGameManager Instance = null;
    private PhotonView photonView;

    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject camera;

    [Header("Countdown time in seconds")] public float Countdown = 5.0f;
    private float startTime;
    private bool isTimerRunning;

    private bool gameStarted = false;

    [SerializeField] private GameObject wallPrefab;

    private PingBongPlayer myPlayer;
    private List<PingBongWall> myWall;
    private PingBongBall ball = null;

    private List<Player> leftPlayers;
    private List<Player> newPlayers;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        photonView = GetComponent<PhotonView>();
        leftPlayers = new List<Player>();
        newPlayers = new List<Player>();
        infoText.text = "Waiting for other players...";

        Hashtable props = new Hashtable
        {
            {"PlayerLoadedLevel", true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnect : " + cause);
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }


    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (changedProps.ContainsKey("PlayerLoadedLevel"))
        {
            if (CheckAllPlayerLoadedLevel())
            {
                Hashtable props = new Hashtable
                {
                    {CountdownTimer.CountdownStartTime, (float) PhotonNetwork.Time}
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //newPlayers.Add(newPlayer);
    }

    private void NewPlayer(Player newPlayer)
    {
        if (gameStarted)
        {
            if (PhotonNetwork.LocalPlayer != newPlayer)
            {
                PingBongWall wall = Instantiate(wallPrefab).GetComponent<PingBongWall>();
                wall.WallPosition = newPlayer.GetPlayerNumber();
                wall.WallPlayerId = newPlayer.ActorNumber;
                myWall.Add(wall);
                ReplaceObject();
                uiManager.NewPlayer(newPlayer);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        RespawnBall(0);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //leftPlayers.Add(otherPlayer);
    }

    private void LeftPlayer(Player otherPlayer)
    {
        if (gameStarted)
        {
            List<PingBongWall> removeWall = new List<PingBongWall>();
            foreach (PingBongWall wall in myWall)
            {
                if (wall.WallPlayerId == otherPlayer.ActorNumber)
                {
                    removeWall.Add(wall);
                }
            }

            foreach (PingBongWall wall in removeWall)
            {
                Destroy(wall.gameObject);
                myWall.Remove(wall);
            }

            uiManager.LeftPlayer(otherPlayer);

            ReplaceObject();
        }
    }

    private void ReplaceObject()
    {
        int totalPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        float radius = 12.5f / Mathf.Tan(Mathf.PI / totalPlayer);
        if (totalPlayer <= 2)
        {
            radius = 12.5f;
        }

        int totalWall = totalPlayer > 2 ? totalPlayer : 4;
        for (int i = 0; i < totalWall; i++)
        {
            if (myWall.Count <= i)
            {
                PingBongWall wall = Instantiate(wallPrefab).GetComponent<PingBongWall>();
                myWall.Add(wall);
            }
            Vector3 position = CalculateCirclePosition(radius, i, totalWall);
            Quaternion rotation = CalculateCircleRotation(i, totalWall);
            myWall[i].WallPosition = i;
            if (i < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                myWall[i].WallPlayerId = PhotonNetwork.PlayerList[i].ActorNumber;
            }
            else
            {
                myWall[i].WallPlayerId = -1;
            }
            myWall[i].transform.position = position;
            myWall[i].transform.rotation = rotation;

            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[i])
            {
                position = CalculateCirclePosition(radius - 2, i, totalWall);
                rotation = CalculateCircleRotation(i, totalWall);
                myPlayer.ReplacePlayer(position, rotation);

                position = CalculateCirclePosition(radius, i, totalWall);
                rotation = CalculateCircleRotation(i, totalWall);
                camera.transform.rotation = rotation;
                camera.transform.Rotate(Vector3.right, 45);
                position += Vector3.up * 10;
                camera.transform.position = position;
            }

        }

    }

    private void StartGame()
    {
        int totalPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        float radius = 12.5f / Mathf.Tan(Mathf.PI / totalPlayer);
        if (totalPlayer <= 2)
        {
            radius = 12.5f;
        }

        int totalWall = totalPlayer > 2 ? totalPlayer : 4;
        for(int i = 0; i < totalWall; i++)
        {

            myWall = new List<PingBongWall>();
            Vector3 position = CalculateCirclePosition(radius, i, totalWall);
            Quaternion rotation = CalculateCircleRotation(i, totalWall);
            PingBongWall wall = Instantiate(wallPrefab, position, rotation).GetComponent<PingBongWall>();
            wall.WallPosition = i;
            if (i < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                wall.WallPlayerId = PhotonNetwork.PlayerList[i].ActorNumber;
            }
            else
            {
                wall.WallPlayerId = -1;
            }

            myWall.Add(wall);
            if (i < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[i])
                {
                    Debug.LogError("Player Order : " + i);
                    position = CalculateCirclePosition(radius - 2, i, totalWall);
                    rotation = CalculateCircleRotation(i, totalWall);
                    myPlayer = PhotonNetwork.Instantiate("PingBongPlayer", position, rotation, 0).GetComponent<PingBongPlayer>();

                    position = CalculateCirclePosition(radius, i, totalWall);
                    rotation = CalculateCircleRotation(i, totalWall);
                    camera.transform.rotation = rotation;
                    camera.transform.Rotate(Vector3.right, 45);
                    position += Vector3.up * 10;
                    camera.transform.position = position;
                }
            }

        }

        gameStarted = true;
    }

    public static Vector3 CalculateCirclePosition(float radius, int selectSegment, int maxSegment)
    {
        int totalPlayer = maxSegment;
        float angularStart = (360.0f / totalPlayer) * selectSegment;
        float x = radius * Mathf.Sin(angularStart * Mathf.Deg2Rad);
        float z = radius * Mathf.Cos(angularStart * Mathf.Deg2Rad);
        Vector3 position = new Vector3(x, 0.0f, z);
        return position;
    }

    public static Quaternion CalculateCircleRotation(int selectSegment, int maxSegment)
    {
        int totalPlayer = maxSegment;
        float angularStart = (360.0f / totalPlayer) * selectSegment;
        Quaternion rotation = Quaternion.Euler(0.0f, angularStart + 180, 0.0f);
        return rotation;
    }

    private bool CheckAllPlayerLoadedLevel()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object playerLoadedLevel;

            if (player.CustomProperties.TryGetValue("PlayerLoadedLevel", out playerLoadedLevel))
            {
                if ((bool) playerLoadedLevel)
                {
                    continue;
                }
            }

            return false;
        }

        return true;
    }

    private void OnCountdownTimerIsExpired()
    {
        if (!gameStarted)
        {
            StartGame();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            ball = PhotonNetwork.Instantiate("PingBongBall", Vector3.up, Quaternion.identity, 0).GetComponent<PingBongBall>();
        }
    }


    private void Update()
    {
        if (isTimerRunning)
        {
            float timer = (float) PhotonNetwork.Time - startTime;
            float countdown = Countdown - timer;

            infoText.text = string.Format("Game starts in {0} seconds", countdown.ToString("n2"));

            if (countdown > 0.0f)
            {
                return;
            }

            isTimerRunning = false;

            infoText.text = string.Empty;

            OnCountdownTimerIsExpired();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object startTimeFromProps;

        if (propertiesThatChanged.TryGetValue("StartTime", out startTimeFromProps))
        {
            isTimerRunning = true;
            startTime = (float) startTimeFromProps;
        }
    }

    public void AddScore(int playerID, int losePlayerID)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == playerID)
            {
                player.AddScore(1);
            }
        }
        photonView.RPC("RespawnBall", RpcTarget.All, losePlayerID);
    }

    [PunRPC]
    public void RespawnBall(int losePlayerID)
    {
        foreach (Player player in newPlayers)
        {
            NewPlayer(player);
        }

        foreach (Player player in leftPlayers)
        {
            LeftPlayer(player);
        }

        newPlayers = new List<Player>();
        leftPlayers = new List<Player>();

        if (PhotonNetwork.IsMasterClient)
        {
            if (ball == null)
            {
                ball = PhotonNetwork.Instantiate("PingBongBall", Vector3.up, Quaternion.identity, 0).GetComponent<PingBongBall>();
            }
            else
            {
                ball.Respawn(losePlayerID);
            }
        }
    }
}