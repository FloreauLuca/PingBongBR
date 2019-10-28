using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PongCase : MonoBehaviour
{
    public enum CaseType
    {
        FREEZE,
        BIGGER,
        SMALLER,
        NONE
    }

    [SerializeField] private CaseType type;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == other.GetComponent<PongBall>().LastPlayerId)
            {
                PongGameManager.Instance.OpenCase(type);
                Destroy(gameObject);
            }
        }
    }
}
