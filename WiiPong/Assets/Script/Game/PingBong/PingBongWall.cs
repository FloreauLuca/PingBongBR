using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingBongWall : MonoBehaviour
{
    [SerializeField] private int wallPlayerID;
    public int WallPlayerId
    {
        get => wallPlayerID;
        set => wallPlayerID = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            int ballPlayerID = other.GetComponent<PingBongBall>().LastPlayerId;
            if (wallPlayerID != -1)
            {
                if (wallPlayerID != ballPlayerID)
                {
                    PingBongGameManager.Instance.AddScore(ballPlayerID);
                    other.GetComponent<PingBongBall>().Respawn(wallPlayerID);
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
