using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class PingBongPlayer : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float rotationSpeed = 10;

    private PhotonView photonView;

    private int playerID;
    public int PlayerId => playerID;

    private Vector3 startOrientation;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = GlobalGameManager.GetColor(photonView.Owner.GetPlayerNumber());
        }

        playerID = photonView.Owner.ActorNumber;
        foreach (Racket racket in GetComponentsInChildren<Racket>())
        {
            racket.PlayerId = PlayerId;
        }
        startOrientation = transform.right;
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            transform.position = transform.position + (startOrientation * speed * Input.GetAxis("Horizontal"));
            if (Vector3.Dot(startOrientation, transform.position - startPosition) > 10)
            {
                transform.position = startPosition + startOrientation * 10;
            }
            if (Vector3.Dot(startOrientation, transform.position - startPosition) < -10)
            {
                transform.position = startPosition + startOrientation * -10;
            }
            transform.Rotate(Vector3.up, rotationSpeed * Input.GetAxis("Vertical"));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            other.GetComponent<PingBongBall>().Hit(transform.forward.normalized, 1, playerID);
        }
    }
}
