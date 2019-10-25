using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
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


    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        photonView = GetComponent<PhotonView>();
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
        NewPlayer(newPlayer);
    }

    private void NewPlayer(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && ball != null)
        {
            Destroy(ball.gameObject);
        }

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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        LeftPlayer(otherPlayer);
    }

    private void LeftPlayer(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient && ball != null)
        {
            Destroy(ball.gameObject);
        }

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

        Vector3 position = CalculateCirclePosition(radius - 2, PhotonNetwork.LocalPlayer.GetPlayerNumber());
        Quaternion rotation = CalculateCircleRotation(PhotonNetwork.LocalPlayer.GetPlayerNumber());
        myPlayer.ReplacePlayer(position, rotation);

        foreach (PingBongWall wall in myWall)
        {
            position = CalculateCirclePosition(radius, wall.WallPosition);
            rotation = CalculateCircleRotation(wall.WallPosition);
            wall.transform.position = position;
            wall.transform.rotation = rotation;
        }

        position = CalculateCirclePosition(radius, PhotonNetwork.LocalPlayer.GetPlayerNumber());
        rotation = CalculateCircleRotation(PhotonNetwork.LocalPlayer.GetPlayerNumber());
        camera.transform.rotation = rotation;
        camera.transform.Rotate(Vector3.right, 45);
        position += Vector3.up * 10;
        camera.transform.position = position;
    }

    private void StartGame()
    {
        int totalPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        float radius = 12.5f / Mathf.Tan(Mathf.PI / totalPlayer);
        if (totalPlayer <= 2)
        {
            radius = 12.5f;
        }

        Debug.LogError(PhotonNetwork.LocalPlayer.GetPlayerNumber() + " " + (PhotonNetwork.LocalPlayer == null).ToString() + " " + (!PhotonNetwork.IsConnectedAndReady).ToString() + " " + (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(PlayerNumbering.RoomPlayerIndexedProp)).ToString());
        Vector3 position = CalculateCirclePosition(radius - 2, PhotonNetwork.LocalPlayer.GetPlayerNumber());
        Quaternion rotation = CalculateCircleRotation(PhotonNetwork.LocalPlayer.GetPlayerNumber());
        myPlayer = PhotonNetwork.Instantiate("PingBongPlayer", position, rotation, 0).GetComponent<PingBongPlayer>();

        position = CalculateCirclePosition(radius, PhotonNetwork.LocalPlayer.GetPlayerNumber());
        rotation = CalculateCircleRotation(PhotonNetwork.LocalPlayer.GetPlayerNumber());
        camera.transform.rotation = rotation;
        camera.transform.Rotate(Vector3.right, 45);
        position += Vector3.up * 10;
        camera.transform.position = position;

        myWall = new List<PingBongWall>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            position = CalculateCirclePosition(radius, player.GetPlayerNumber());
            rotation = CalculateCircleRotation(player.GetPlayerNumber());
            PingBongWall wall = Instantiate(wallPrefab, position, rotation).GetComponent<PingBongWall>();
            wall.WallPosition = player.GetPlayerNumber();
            wall.WallPlayerId = player.ActorNumber;
            myWall.Add(wall);
        }

        gameStarted = true;
    }

    public static Vector3 CalculateCirclePosition(float radius, int selectSegment)
    {
        int totalPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        float angularStart = (360.0f / totalPlayer) * selectSegment;
        float x = radius * Mathf.Sin(angularStart * Mathf.Deg2Rad);
        float z = radius * Mathf.Cos(angularStart * Mathf.Deg2Rad);
        Vector3 position = new Vector3(x, 0.0f, z);
        return position;
    }

    public static Quaternion CalculateCircleRotation(int selectSegment)
    {
        int totalPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
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

    public void AddScore(int playerID)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == playerID)
            {
                player.AddScore(1);
                uiManager.UpdateScore();
            }
        }
    }
}