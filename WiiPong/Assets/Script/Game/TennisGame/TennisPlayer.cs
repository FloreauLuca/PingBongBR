using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class TennisPlayer : MonoBehaviour
{
    private Rigidbody rigidbody;
    private Vector3 objectif;
    private bool running = false;

    [SerializeField] private float speed = 10;
    


    private PhotonView photonView;

    private int playerID;
    public int PlayerId => playerID;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = GlobalGameManager.GetColor(photonView.Owner.GetPlayerNumber());
        }

        playerID = photonView.Owner.ActorNumber;
        foreach (Racket racket in GetComponentsInChildren<Racket>())
        {
            racket.PlayerId = PlayerId;
        }


        running = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(objectif, transform.position) < 1)
        {
            rigidbody.velocity = Vector3.zero;
        }
    }

    public void Goto(Vector3 point)
    {
        rigidbody.AddForce((point-transform.position).normalized * speed);
        objectif = point;
        objectif.Set(objectif.x, transform.position.y, objectif.z);
    }

    private void OnDrawGizmos()
    {
        if (!running) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(objectif, 0.75f);
    }

}
