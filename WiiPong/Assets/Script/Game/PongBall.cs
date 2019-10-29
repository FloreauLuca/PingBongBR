using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PongBall : MonoBehaviour
{
    private Vector3 lastPosition;
    private Vector3 lastVelocity;
    private float timeSinceLast;

    [SerializeField] private Vector3 gravity;
    [SerializeField] private float bounceCoef = 1f;

    [SerializeField] private Vector3 startForce;
    [SerializeField] private float maxSpeed;

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
        GetComponent<Renderer>().material.color = GlobalGameManager.GetColor(lastPlayerID);
    }

    private void Update()
    {
        timeSinceLast += Time.deltaTime;
        transform.position = lastPosition + lastVelocity * timeSinceLast + (1f / 2) * gravity * timeSinceLast * timeSinceLast;
    }

    private void FixedUpdate()
    {
        if (transform.position.y < 0.5)
        {
            Bounce(Vector3.up, bounceCoef);
        }
        if (Vector3.Distance(transform.position, Vector3.zero) > 100)
        {
            PongGameManager.Instance.BallLostMessage();
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
        if (newVelocity.magnitude > maxSpeed)
        {
            newVelocity = newVelocity.normalized * maxSpeed;
        }
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
        {
            photonView.RPC("AddForce", RpcTarget.Others, transform.position, newVelocity, lastPlayerID, false);
        }
        AddForce(transform.position, newVelocity, lastPlayerID, true);
        Debug.Log("AddForce");
    }


    [PunRPC]
    public void AddForce(Vector3 currentPosition, Vector3 newForce, int playerID, bool spawn)
    {
        lastPlayerID = playerID;
        GetComponent<Renderer>().material.color = GlobalGameManager.GetColor(lastPlayerID);
        lastVelocity = newForce;
        lastPosition = new Vector3(currentPosition.x, 0.6f, currentPosition.z);
        if (spawn)
        {
            timeSinceLast = 0;
        }
        else
        {
            timeSinceLast = PhotonNetwork.GetPing()/1000f * 2;
            Debug.Log(timeSinceLast);
        }
    }

    private void Bounce(Vector3 normalVector, float power)
    {
        Vector3 normal = normalVector.normalized;
        Vector3 currentVelocity = (lastVelocity + gravity * timeSinceLast);
        Vector3 newVelocity = currentVelocity - 2f * Vector3.Dot(currentVelocity, normal) * normal;
        newVelocity = ((bounceCoef * 1000 * (newVelocity)) + (1 * newVelocity)) / (1 + 1000);
        newVelocity *= power;
        if (PhotonNetwork.IsMasterClient && PongGameManager.Instance.Ball != null)
        {
            photonView.RPC("AddForce", RpcTarget.Others, transform.position, newVelocity, lastPlayerID, false);
        }
        AddForce(transform.position, newVelocity, lastPlayerID, true);
    }


    public void Respawn(int losePlayer)
    {
        if (losePlayer <= 0)
        {
            lastPosition = transform.position;
            lastVelocity = startForce;
            timeSinceLast = 0;
            GetComponent<Renderer>().material.color = GlobalGameManager.GetColor(lastPlayerID);
            Quaternion rotation = PongGameManager.CalculateCircleRotation(0, 0, PhotonNetwork.CurrentRoom.PlayerCount);
            photonView.RPC("AddForce", RpcTarget.All, lastPosition, rotation * startForce, losePlayer, true);
            return;
        }

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == losePlayer)
            {
                float radius = 15f / Mathf.Tan(Mathf.PI / PhotonNetwork.CurrentRoom.PlayerCount);
                Quaternion rotation = PongGameManager.CalculateCircleRotation(0, i, PhotonNetwork.CurrentRoom.PlayerCount);
                Vector3 position = PongGameManager.CalculateCirclePosition(0, radius - 3, i, PhotonNetwork.CurrentRoom.PlayerCount);
                position += Vector3.up;
                photonView.RPC("AddForce", RpcTarget.All, position, rotation * startForce, losePlayer, true);
                break;
            }
        }
    }
    public void Stop()
    {
        lastVelocity = Vector3.zero;
        lastPosition = Vector3.up;
        lastPlayerID = -1;
        photonView.RPC("AddForce", RpcTarget.All, lastPosition, lastVelocity, lastPlayerID, false);
    }
}
