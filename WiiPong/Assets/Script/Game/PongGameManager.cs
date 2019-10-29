using System;
using System.Collections;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class PongGameManager : MonoBehaviourPunCallbacks
{
    public static PongGameManager Instance = null;
    private PhotonView photonView;

    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private PongUIManager uiManager;
    [SerializeField] private GameObject camera;

    [Header("Countdown time in seconds")] public float Countdown = 5.0f;
    private float startTime;
    private bool isTimerRunning;

    private bool gameStarted = false;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject wallEmptyPrefab;

    private PongBall ball = null;
    public PongBall Ball => ball;
    private PongPlayer myPlayer = null;

    private bool endOfGame = false;

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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerDisconnectedMessage(otherPlayer.ActorNumber, otherPlayer.NickName);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartCoroutine(SpawnCase());
        }
    }

    private void StartGame()
    {
        int totalPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        float radius = 15f / Mathf.Tan(Mathf.PI / totalPlayer);
        if (totalPlayer <= 2)
        {
            radius = 12.5f;
        }

        for (int i = 0; i < totalPlayer; i++)
        {
            Vector3 position = CalculateCirclePosition(0, radius, i, totalPlayer);
            Quaternion rotation = CalculateCircleRotation(0, i, totalPlayer);
            PongWall wall = Instantiate(wallPrefab, position, rotation).GetComponent<PongWall>();
            wall.WallPlayerId = PhotonNetwork.PlayerList[i].ActorNumber;
            wall.SetColor(GlobalGameManager.GetColor(wall.WallPlayerId) - Color.gray);


            if (totalPlayer % 2 != 0)
            {
                position = CalculateCirclePosition(180, radius + 3, i, totalPlayer);
                rotation = CalculateCircleRotation(180, i, totalPlayer);
                PongWall wallCorner = Instantiate(wallEmptyPrefab, position, rotation).GetComponent<PongWall>();
                wallCorner.WallPlayerId = -1;
            }


            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[i])
            {
                position = CalculateCirclePosition(0, radius - 2, i, totalPlayer);
                rotation = CalculateCircleRotation(0, i, totalPlayer);
                myPlayer = PhotonNetwork.Instantiate("PongPlayer", position, rotation, 0).GetComponent<PongPlayer>();

                position = CalculateCirclePosition(0, radius, i, totalPlayer);
                rotation = CalculateCircleRotation(0, i, totalPlayer);
                camera.transform.rotation = rotation;
                camera.transform.Rotate(Vector3.right, 45);
                position += Vector3.up * 10;
                camera.transform.position = position;
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnCase());
        }
        gameStarted = true;
    }

    public static Vector3 CalculateCirclePosition(float startRotation, float radius, int selectSegment, int maxSegment)
    {
        int totalPlayer = maxSegment;
        float angularStart = (360.0f / totalPlayer) * selectSegment + startRotation;
        float x = radius * Mathf.Sin(angularStart * Mathf.Deg2Rad);
        float z = radius * Mathf.Cos(angularStart * Mathf.Deg2Rad);
        Vector3 position = new Vector3(x, 0.0f, z);
        return position;
    }

    public static Quaternion CalculateCircleRotation(float startRotation, int selectSegment, int maxSegment)
    {
        int totalPlayer = maxSegment;
        float angularStart = (360.0f / totalPlayer) * selectSegment + startRotation;
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
            ball = PhotonNetwork.Instantiate("PongBall", Vector3.up, Quaternion.identity, 0).GetComponent<PongBall>();
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
        string nickName = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == playerID)
            {
                player.AddScore(1);
                nickName = player.NickName;
            }
        }

        photonView.RPC("RespawnRequest", RpcTarget.All, (nickName + " score a goal"), losePlayerID, playerID);
    }

    public void BallLostMessage()
    {
        photonView.RPC("RespawnRequest", RpcTarget.All, ("Sorry for the inconvenience the ball is going to respawn."), -1, -1);
    }

    public void PlayerDisconnectedMessage(int playerID, string nickName)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            photonView.RPC("RespawnRequest", RpcTarget.All, (nickName + " has been disconnected"), -1, -1);
        }
        else
        {
            StartCoroutine(EndOfGame(PhotonNetwork.LocalPlayer));
        }
    }

    [PunRPC]
    public void RespawnRequest(string message, int losePlayerID, int winPlayerID)
    {
        if (PhotonNetwork.IsMasterClient && !endOfGame && ball != null)
        {
            ball.Stop();
        }

        CheckEndOfGame();
        uiManager.UpdateScore();
        StartCoroutine(RespawnMessage(message, losePlayerID, winPlayerID));
    }

    public void RespawnBall(int losePlayerID)
    {
        if (PhotonNetwork.IsMasterClient && !endOfGame)
        {
            if (ball == null)
            {
                ball = PhotonNetwork.Instantiate("PongBall", Vector3.up, Quaternion.identity, 0).GetComponent<PongBall>();
            }
            else
            {
                ball.Respawn(losePlayerID);
            }
        }
    }

    private void CheckEndOfGame()
    {
        Player winner = null;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetScore() >= 10)
            {
                winner = player;
            }
        }

        if (winner != null)
        {
            endOfGame = true;
            if (PhotonNetwork.IsMasterClient)
            {
                StopAllCoroutines();
            }

            StartCoroutine(EndOfGame(winner));
        }
    }


    private IEnumerator EndOfGame(Player winner)
    {
        float timer = 5.0f;

        while (timer > 0.0f)
        {
            infoText.text = string.Format("{0} won with {1} points.\n\n\nReturning to login screen in {2} seconds.", winner.NickName, winner.GetScore(), timer.ToString("n2"));
            infoText.color = GlobalGameManager.GetColor(winner.ActorNumber);
            yield return new WaitForEndOfFrame();

            timer -= Time.deltaTime;
        }

        PhotonNetwork.LeaveRoom();
    }


    private IEnumerator RespawnMessage(string message, int losePlayerID, int winPlayerID)
    {
        float timer = 1.0f;

        while (timer > 0.0f)
        {
            infoText.text = message + "\n\n\nBall will respawn in " + timer.ToString("n2");
            infoText.color = GlobalGameManager.GetColor(winPlayerID);
            yield return new WaitForEndOfFrame();

            timer -= Time.deltaTime;
        }
        infoText.text = "";
        RespawnBall(losePlayerID);
    }

    public void OpenCase(PongCase.CaseType type)
    {
        switch (type)
        {
            case PongCase.CaseType.FREEZE:
                photonView.RPC("Freeze", RpcTarget.Others);
                break;
            case PongCase.CaseType.BIGGER:
                StartCoroutine(myPlayer.Bigger());
                break;
            case PongCase.CaseType.SMALLER:
                StartCoroutine(myPlayer.Smaller());
                break;
        }
    }

    [PunRPC]
    public void Freeze()
    {
        StartCoroutine(myPlayer.Freezed());
    }


    private IEnumerator SpawnCase()
    {
        int totalPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        float radius = 15f / Mathf.Tan(Mathf.PI / totalPlayer);
        if (totalPlayer <= 2)
        {
            radius = 12.5f;
        }

        while (!endOfGame)
        {
            yield return new WaitForSeconds(Random.Range(5, 25));
            Vector3 position = new Vector3(Random.Range(-radius+3, radius-3), 1, Random.Range(-radius + 3, radius - 3));
            int random = Random.Range(0,3);
            switch (random)
            {
                case 0:
                    PhotonNetwork.InstantiateSceneObject("FreezeCase", position, Quaternion.Euler(0, Random.value * 360.0f, 0));
                    break;
                case 1:
                    PhotonNetwork.InstantiateSceneObject("SmallCase", position, Quaternion.Euler(0, Random.value * 360.0f, 0));
                    break;
                case 2:
                    PhotonNetwork.InstantiateSceneObject("BigCase", position, Quaternion.Euler(0, Random.value * 360.0f, 0));
                    break;
            }
        }
    }


}