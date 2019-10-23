using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    private PhotonView photonView;

    public void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            GameManager.Instance.AddScore(collision.gameObject.GetComponent<BallScript>().lastPlayerID);
        }
    }

}
