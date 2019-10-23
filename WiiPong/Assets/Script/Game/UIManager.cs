using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject scoreTëxtPrefab;

    private Dictionary<int, TextMeshProUGUI> playerUIs;

    public void Awake()
    {
        playerUIs = new Dictionary<int, TextMeshProUGUI>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(scoreTëxtPrefab);
            entry.transform.SetParent(gameObject.transform);
            entry.transform.localScale = Vector3.one;
            TextMeshProUGUI entryText = entry.GetComponent<TextMeshProUGUI>();
            entryText.color = GlobalGameManager.GetColor(player.GetPlayerNumber());
            entryText.text = "Player " + player.ActorNumber + " : 0";
            playerUIs.Add(player.ActorNumber, entryText);

        }
    }

    public void UpdateScore()
    {
        foreach (KeyValuePair<int, int> score in PingBongGameManager.Instance.Score)
        {
            playerUIs[score.Key].text = "Player " + score.Key + " : " + score.Value;
        }
    }
}
