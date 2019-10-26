﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PongWall : MonoBehaviour
{
    private int wallPlayerID = -1;
    public int WallPlayerId
    {
        get => wallPlayerID;
        set => wallPlayerID = value;
    }

    private int wallPosition = 0;
    public int WallPosition
    {
        get => wallPosition;
        set => wallPosition = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            int ballPlayerID = other.GetComponent<PongBall>().LastPlayerId;
            if (wallPlayerID != -1)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber != wallPlayerID)
                {
                    return;
                }

                if (wallPlayerID != ballPlayerID)
                {
                    PongGameManager.Instance.AddScore(ballPlayerID, wallPlayerID);
                }
                else
                {
                    other.GetComponent<PongBall>().Hit(transform.forward.normalized, 1, wallPlayerID);
                }

            }
            else
            {
                other.GetComponent<PongBall>().Hit(transform.forward.normalized, 1, ballPlayerID);
            }
        }
    }
}

