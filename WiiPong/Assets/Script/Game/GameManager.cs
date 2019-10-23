using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance = null;

    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Countdown time in seconds")]
    public float Countdown = 5.0f;
    private float startTime;
    private bool isTimerRunning;

    private bool gameStarted = false;

    private Dictionary<int, int> score;
    public Dictionary<int, int> Score => score;

    private TennisPlayer player1;
    private TennisPlayer player2;


    public void Awake()
    {
        Instance = this;
        score = new Dictionary<int, int>();
    }

    public void Start()
    {
        infoText.text = "Waiting for other players...";

        Hashtable props = new Hashtable
        {
            {"PlayerLoadedLevel", true}, {"Score", 0}
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


    private void StartGame()
    {
        if (!gameStarted)
        {
            float angularStart = (360.0f / PhotonNetwork.CurrentRoom.PlayerCount) * PhotonNetwork.LocalPlayer.GetPlayerNumber();
            float x = -8.0f * Mathf.Sin(angularStart * Mathf.Deg2Rad);
            float z = -8.0f * Mathf.Cos(angularStart * Mathf.Deg2Rad);
            Vector3 position = new Vector3(x, 0.0f, z);
            //Quaternion rotation = Quaternion.Euler(0.0f, angularStart, 0.0f);

            GameObject player = PhotonNetwork.Instantiate("TennisPlayer", position, Quaternion.identity, 0);
            
            /*
            angularStart = (360.0f / PhotonNetwork.CurrentRoom.PlayerCount) * PhotonNetwork.LocalPlayer.GetPlayerNumber();
            x = 10.0f * Mathf.Sin(angularStart * Mathf.Deg2Rad);
            z = 10.0f * Mathf.Cos(angularStart * Mathf.Deg2Rad);
            position = new Vector3(x, 0.0f, z);
            PhotonNetwork.Instantiate("Wall", position, Quaternion.identity, 0);
            gameStarted = true;
            */
        }
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
                    if (!score.ContainsKey(player.ActorNumber))
                    {
                        score.Add(player.ActorNumber, 0);
                    }

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
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        score[playerID]++;
        /*
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber != playerID)
            {
                continue;
            }
            object score;

            if (player.CustomProperties.TryGetValue("Score", out score))
            {
                player.SetCustomProperties(new Hashtable {{"Score", ((int) score + 1)}});
            }
        }
        */
    }
}
