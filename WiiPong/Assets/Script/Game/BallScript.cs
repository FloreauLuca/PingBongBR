using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public Vector3 startSpeed;

    public int lastPlayerID = -1;

    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Rigidbody>().AddForce(startSpeed);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(startSpeed);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            lastPlayerID = collision.gameObject.GetComponent<PlayerScript>().PlayerId;
            GetComponent<Renderer>().material.color = GlobalGameManager.GetColor(lastPlayerID-1);
        }
    }
}
