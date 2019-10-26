using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PingBongWall : MonoBehaviour
{
    private int wallPlayerID;
    public int WallPlayerId
    {
        get => wallPlayerID;
        set => wallPlayerID = value;
    }

    private int wallPosition;
    public int WallPosition
    {
        get => wallPosition;
        set => wallPosition = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            int ballPlayerID = other.GetComponent<PingBongBall>().LastPlayerId;
            if (wallPlayerID != -1)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber != wallPlayerID)
                {
                    return;
                }

                if (wallPlayerID != ballPlayerID)
                {
                    if (wallPlayerID == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        PingBongGameManager.Instance.AddScore(ballPlayerID, wallPlayerID);
                    }
                }
                else
                {
                    other.GetComponent<PingBongBall>().Hit(transform.forward.normalized, 1, wallPlayerID);
                }

            }
            else
            {
                other.GetComponent<PingBongBall>().Hit(transform.forward.normalized, 1, ballPlayerID);
            }
        }
    }
}

