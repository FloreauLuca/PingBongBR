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
    [SerializeField] private Vector3 biggerScale;
    [SerializeField] private Vector3 smallerScale;
    private Vector3 standardScale;

    private PhotonView photonView;

    private int playerID;
    public int PlayerId => playerID;

    private Vector3 startOrientation;
    private Vector3 startPosition;

    private bool freezed = false;
    private bool big = false;
    private bool small = false;


    [SerializeField] private Renderer colorRenderer;

    public void SetColor(Color color)
    {
        colorRenderer.material.color = color;
    }


    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerID = photonView.Owner.ActorNumber;
        SetColor(GlobalGameManager.GetColor(playerID));
        startOrientation = transform.right;
        startPosition = transform.position;
        standardScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && !freezed)
        {
            transform.position = transform.position + (startOrientation * speed * Input.GetAxis("Horizontal") * Time.deltaTime);
            if (Vector3.Dot(startOrientation, transform.position - startPosition) > 10)
            {
                transform.position = startPosition + startOrientation * 10;
            }
            if (Vector3.Dot(startOrientation, transform.position - startPosition) < -10)
            {
                transform.position = startPosition + startOrientation * -10;
            }
            transform.Rotate(Vector3.up, rotationSpeed * Input.GetAxis("Vertical") * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            other.GetComponent<PongBall>().Hit(transform.forward.normalized, power, playerID);
        }
    }

    public IEnumerator Freezed()
    {
        freezed = true;
        SetColor(GlobalGameManager.GetColor(playerID) + Color.gray);
        yield return new WaitForSeconds(5.0f);
        SetColor(GlobalGameManager.GetColor(playerID));
        freezed = false;
    }


    public IEnumerator Bigger()
    {
        big = true;
        small = false;
        transform.localScale = biggerScale;
        yield return new WaitForSeconds(5.0f);
        if (big)
        {
            transform.localScale = standardScale;
            big = false;
        }
    }

    public IEnumerator Smaller()
    {
        small = true;
        big = false;
        transform.localScale = smallerScale;
        yield return new WaitForSeconds(5.0f);
        if (small)
        {
            transform.localScale = standardScale;
        }
    }

}
