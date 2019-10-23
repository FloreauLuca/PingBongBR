using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
        timeSinceLast += Time.deltaTime;
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
        Bounce(newForce, power);
    }

    [PunRPC]
    public void AddForce(Vector3 currentPosition, Vector3 newForce, int playerID)
    {
        lastPlayerID = playerID;
        lastVelocity = newForce;
        lastPosition = currentPosition;
        timeSinceLast = 0;
    }
    
    private void Bounce(Vector3 normalVector, float power)
    {
        Vector3 normal = normalVector.normalized;
        Vector3 currentVelocity = (lastVelocity + gravity * timeSinceLast);
        Vector3 newVelocity = currentVelocity - 2f * Vector3.Dot(currentVelocity, normal) * normal;
        newVelocity = ((bounceCoef * 1000 * (newVelocity)) + (1 * newVelocity)) / (1 + 1000);
        newVelocity *= power;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("AddForce", RpcTarget.All, transform.position, newVelocity, lastPlayerID);
        }
    }


    public void Respawn(int losePlayer)
    {
        float radius = 10 / Mathf.Tan(Mathf.PI / PhotonNetwork.CurrentRoom.PlayerCount);
        Quaternion rotation = PingBongGameManager.CalculateCircleRotation(losePlayer-1);
        Vector3 position = PingBongGameManager.CalculateCirclePosition(radius - 3, losePlayer-1);
        position += Vector3.up;
        photonView.RPC("AddForce", RpcTarget.All, position, rotation * startForce, losePlayer);
        Debug.Log(position);
    }
}
