using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI PlayerNameText;

    public Image playerColorImage;
    public Button playerReadyButton;
    public Image playerReadyImage;

    private int ownerId;
    private bool isPlayerReady;

    #region UNITY

    public void OnEnable()
    {
        PlayerNumbering.OnPlayerNumberingChanged += OnPlayerNumberingChanged;
    }

    public void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
        {
            playerReadyButton.gameObject.SetActive(false);
        }
        else
        {
            Hashtable initialProps = new Hashtable() { { "IsPlayerReady", isPlayerReady }, {"PlayerLives",GlobalGameManager.PLAYER_MAX_LIVES } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
            PhotonNetwork.LocalPlayer.SetScore(0);

        }
    }

    public void OnDisable()
    {
        PlayerNumbering.OnPlayerNumberingChanged -= OnPlayerNumberingChanged;
    }

    #endregion

    public void Initialize(int playerId, string playerName)
    {
        ownerId = playerId;
        PlayerNameText.text = playerName;
    }

    private void OnPlayerNumberingChanged()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == ownerId)
            {
                playerColorImage.color = GlobalGameManager.GetColor(player.GetPlayerNumber());
            }
        }
    }

    public void SetPlayerReady(bool playerReady)
    {
        playerReadyButton.GetComponentInChildren<TextMeshProUGUI>().text = playerReady ? "Ready!" : "Ready?";
        playerReadyImage.enabled = playerReady;
    }

    public void OnReadyButtonClick()
    {
        isPlayerReady = !isPlayerReady;
        SetPlayerReady(isPlayerReady);

        Hashtable props = new Hashtable() { { "IsPlayerReady", isPlayerReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        if (PhotonNetwork.IsMasterClient)
        {
            FindObjectOfType<LobbyScript>().LocalPlayerPropertiesUpdated();
        }
    }
}
