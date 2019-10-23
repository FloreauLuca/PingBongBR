using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisWall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            GameManager.Instance.AddScore(other.GetComponent<TennisBall>().LastPlayerId);
            other.GetComponent<TennisBall>().Hit(transform.forward.normalized, 1, other.GetComponent<TennisBall>().LastPlayerId);
        }
    }
}
