using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PingBongBall : MonoBehaviour
{
    private Vector3 lastPosition;
    private Vector3 lastVelocity;
    private float timeSinceLast;

    [SerializeField] private Vector3 gravity;
    [SerializeField] private float bounceCoef = 1f;

    [SerializeField] private Vector3 startForce;

    private PhotonView photonView;

    private int lastPlayerID;
    public int LastPlayerId => lastPlayerID;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        lastPosition = transform.position;
        lastVelocity = startForce;
        timeSinceLast = 0;
    }

    private void FixedUpdate()
    {
        timeSinceLast += Time.fixedDeltaTime;
        transform.position = lastPosition + lastVelocity * timeSinceLast + (1f / 2) * gravity * timeSinceLast * timeSinceLast;
        GetComponent<Renderer>().material.color = GlobalGameManager.GetColor(lastPlayerID - 1);
        if (transform.position.y < 0.5)
        {
            Bounce(Vector3.up, bounceCoef);
        }
    }

    public void Hit(Vector3 newForce, float power, int playerID)
    {
        lastPlayerID = playerID;
        Vector3 normal = newForce.normalized;
        Vector3 currentVelocity = (lastVelocity + gravity * timeSinceLast);
        Vector3 newVelocity = currentVelocity - 2f * Vector3.Dot(currentVelocity, normal) * normal;
        newVelocity = ((bounceCoef * 1000 * (newVelocity)) + (1 * newVelocity)) / (1 + 1000);
        newVelocity *= power;
        AddForce(transform.position, newVelocity, lastPlayerID, true);
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
        {
            photonView.RPC("AddForce", RpcTarget.Others, transform.position, newVelocity, lastPlayerID, false);
        }
    }


    [PunRPC]
    public void AddForce(Vector3 currentPosition, Vector3 newForce, int playerID, bool spawn)
    {
        lastPlayerID = playerID;
        lastVelocity = newForce;
        lastPosition = currentPosition;
        if (spawn)
        {
            timeSinceLast = 0;
        }
        else
        {
            timeSinceLast = PhotonNetwork.GetPing()/1000f;
        }
    }

    private void Bounce(Vector3 normalVector, float power)
    {
        Vector3 normal = normalVector.normalized;
        Vector3 currentVelocity = (lastVelocity + gravity * timeSinceLast);
        Vector3 newVelocity = currentVelocity - 2f * Vector3.Dot(currentVelocity, normal) * normal;
        newVelocity = ((bounceCoef * 1000 * (newVelocity)) + (1 * newVelocity)) / (1 + 1000);
        newVelocity *= power;
        AddForce(transform.position, newVelocity, lastPlayerID, true);
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("AddForce", RpcTarget.Others, transform.position, newVelocity, lastPlayerID, false);
        }
    }


    public void Respawn(int losePlayer)
    {
        float radius = 10 / Mathf.Tan(Mathf.PI / PhotonNetwork.CurrentRoom.PlayerCount);
        Quaternion rotation = PingBongGameManager.CalculateCircleRotation(losePlayer-1);
        Vector3 position = PingBongGameManager.CalculateCirclePosition(radius - 3, losePlayer-1);
        position += Vector3.up;
        photonView.RPC("AddForce", RpcTarget.All, position, rotation * startForce, losePlayer, true);
    }
}
