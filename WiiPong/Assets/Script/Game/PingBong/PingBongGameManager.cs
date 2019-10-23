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

    [Header("Countdown time in seconds")]
    public float Countdown = 5.0f;
    private float startTime;
    private bool isTimerRunning;

    private bool gameStarted = false;

    private Dictionary<int, int> score;
    public Dictionary<int, int> Score => score;

    [SerializeField] private GameObject wallPrefab;


    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        score = new Dictionary<int, int>();
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


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
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


    private void StartGame()
    {
        if (!gameStarted)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!score.ContainsKey(player.ActorNumber))
                {
                    score.Add(player.ActorNumber, 0);
                }
            }

            int totalPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
            float radius = 10 / Mathf.Tan(Mathf.PI / totalPlayer);
            if (totalPlayer <= 2)
            {
                radius = 10;
            }
            Debug.Log(radius);
            Vector3 position = CalculateCirclePosition(radius-2, PhotonNetwork.LocalPlayer.GetPlayerNumber());
            Quaternion rotation = CalculateCircleRotation(PhotonNetwork.LocalPlayer.GetPlayerNumber());
            Debug.Log(totalPlayer + " " + PhotonNetwork.LocalPlayer.GetPlayerNumber());

            PhotonNetwork.Instantiate("PingBongPlayer", position, rotation, 0);

            position = CalculateCirclePosition(radius, PhotonNetwork.LocalPlayer.GetPlayerNumber());
            rotation = CalculateCircleRotation(PhotonNetwork.LocalPlayer.GetPlayerNumber());
            camera.transform.rotation = rotation;
            camera.transform.Rotate(Vector3.right, 45);
            position += Vector3.up * 10;
            camera.transform.position = position;

            for (int i = 0; i < totalPlayer; i++)
            {
                position = CalculateCirclePosition(radius, i);
                rotation = CalculateCircleRotation(i);
                GameObject wall = Instantiate(wallPrefab, position, rotation);
                wall.GetComponent<PingBongWall>().WallPlayerId = i + 1;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate("PingBongBall", Vector3.up, Quaternion.identity, 0);
            }
        }
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
                if ((bool)playerLoadedLevel)
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
        StartGame();
    }


    private void Update()
    {
        if (isTimerRunning)
        {
            float timer = (float)PhotonNetwork.Time - startTime;
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

    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object startTimeFromProps;

        if (propertiesThatChanged.TryGetValue("StartTime", out startTimeFromProps))
        {
            isTimerRunning = true;
            startTime = (float)startTimeFromProps;
        }
    }

    
    public void AddScore(int playerID)
    {
        photonView.RPC("UpdateScore", RpcTarget.All, playerID);
    }

    [PunRPC]
    private void UpdateScore(int playerID)
    {
        if (score.ContainsKey(playerID))
        {
            score[playerID]++;
            uiManager.UpdateScore();
        }
        else
        {
            Debug.LogError("Score don't contain : " + playerID);
        }
    }
}
