using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    private PhotonView photonView;

    private new Rigidbody rigidbody;
    private new Collider collider;
    private new Renderer renderer;

    private int playerID;
    public int PlayerId => playerID;

    public void Awake()
    {
        photonView = GetComponent<PhotonView>();

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
    }

    public void Start()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = GlobalGameManager.GetColor(photonView.Owner.GetPlayerNumber());
        }

        playerID = photonView.Owner.ActorNumber;
    }

    public void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal"));
        if (transform.position.x > 8)
        {
            transform.position = transform.position + Vector3.right * -1;
        }
        if (transform.position.x < -8)
        {
            transform.position = transform.position + Vector3.right * 1;
        }
    }
    
}
