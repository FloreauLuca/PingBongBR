using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Racket : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float power = 1;

    private int playerID;
    public int PlayerId
    {
        get => playerID;
        set => playerID = value;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Hit());
        }
    }

    IEnumerator Hit()
    {
        Quaternion startRotation = transform.rotation;
        for (int i = 0; i < speed; i++)
        {
            transform.Rotate(Vector3.up, 90/speed);
            yield return null;
        }

        transform.rotation = startRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            other.GetComponent<TennisBall>().Hit(transform.forward.normalized, power, playerID);
            Debug.Log(transform.forward + "," + (transform.forward.normalized * power + Vector3.up * 5));
        }
    }
}
