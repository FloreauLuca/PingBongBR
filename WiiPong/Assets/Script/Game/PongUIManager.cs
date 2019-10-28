using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PongUIManager : MonoBehaviour
{
    public GameObject scoreTëxtPrefab;

    private Dictionary<int, TextMeshProUGUI> playerUIs;

    public void Start()
    {
        playerUIs = new Dictionary<int, TextMeshProUGUI>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(scoreTëxtPrefab);
            entry.transform.SetParent(gameObject.transform);
            entry.transform.localScale = Vector3.one;
            TextMeshProUGUI entryText = entry.GetComponent<TextMeshProUGUI>();
            entryText.color = GlobalGameManager.GetColor(player.ActorNumber);
            entryText.text = "Player " + player.NickName + " : 0";
            playerUIs.Add(player.ActorNumber, entryText);

        }
    }

    public void UpdateScore()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerUIs[player.ActorNumber].text = player.NickName + " : " + player.GetScore();
        }
    }

    public void NewPlayer(Player newPlayer)
    {
        GameObject entry = Instantiate(scoreTëxtPrefab);
        entry.transform.SetParent(gameObject.transform);
        entry.transform.localScale = Vector3.one;
        TextMeshProUGUI entryText = entry.GetComponent<TextMeshProUGUI>();
        entryText.color = GlobalGameManager.GetColor(newPlayer.ActorNumber);
        entryText.text = "Player " + newPlayer.ActorNumber + " : 0";
        playerUIs.Add(newPlayer.ActorNumber, entryText);
    }

    public void LeftPlayer(Player oldPlayer)
    {
        Destroy(playerUIs[oldPlayer.ActorNumber]);
        playerUIs.Remove(oldPlayer.ActorNumber);
    }
}
