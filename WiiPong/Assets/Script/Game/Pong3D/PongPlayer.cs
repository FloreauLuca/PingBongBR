using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;

public class PongPlayer : MonoBehaviour
{
    [SerializeField] private float power = 1;
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
        playerID = photonView.Owner.ActorNumber;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = GlobalGameManager.GetColor(playerID);
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
            other.GetComponent<PongBall>().Hit(transform.forward.normalized, power, playerID);
        }
    }

}
