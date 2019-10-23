using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisBall : MonoBehaviour
{
    private Vector3 lastPosition;
    private Vector3 lastVelocity;
    private float timeSinceLast;

    [SerializeField] private Vector3 gravity;
    [SerializeField] private float bounceCoef = 1f;

    [SerializeField] private Vector3 force;

    [SerializeField] private TennisPlayer player1;
    [SerializeField] private TennisPlayer player2;
    
    private Vector3[] ballPositions;

    private bool running = false;
    [SerializeField] private int iterations;
    [SerializeField] private int timeOfIterations;


    private int lastPlayerID;
    public int LastPlayerId => lastPlayerID;

    // Start is called before the first frame update
    void Start()
    {
        running = true;
        lastPosition = transform.position;
        //lastVelocity = force;
        ballPositions = new Vector3[iterations];
    }

    private void FixedUpdate()
    {
        timeSinceLast += Time.deltaTime;
        transform.position = lastPosition + lastVelocity * timeSinceLast + (1f / 2) * gravity * timeSinceLast * timeSinceLast;
        if (transform.position.y < 0.5)
        {
            Bounce(Vector3.up, bounceCoef);
        }
        
    }

    public void Hit(Vector3 newForce, float power, int playerID)
    {
        Bounce(newForce, power);
        CalculateTargetPosition();
        lastPlayerID = playerID;
        GetComponent<Renderer>().material.color = GlobalGameManager.GetColor(lastPlayerID - 1);
    }

    public void AddForce(Vector3 currentPosition, Vector3 newForce)
    {
        lastVelocity = newForce;
        lastPosition = currentPosition;
        timeSinceLast = 0;
    }
    
    private void Bounce(Vector3 normalVector, float power)
    {
        Debug.Log("Bong");
        //transform.position = new Vector3(transform.position.x, 0.5f, transform.position.y);
        Vector3 normal = normalVector.normalized;
        Vector3 currentVelocity = (lastVelocity + gravity * timeSinceLast);
        Vector3 newVelocity = currentVelocity - 2f * Vector3.Dot(currentVelocity, normal) * normal * power;
        newVelocity = ((bounceCoef * 1000 * (newVelocity)) + (1 * newVelocity)) / (1 + 1000);
        AddForce(transform.position, newVelocity);

    }

    private void CalculateTargetPosition()
    {
        Vector3 bouncePosition = lastPosition;
        Vector3 bounceVelocity = lastVelocity;
        float timer = 0;
        for (int time = 0; time < iterations; time++)
        {
            timer += 1f / iterations * timeOfIterations;
            Vector3 calculatePosition = bouncePosition + bounceVelocity * timer + (1f / 2) * gravity * timer * timer;
            if (calculatePosition.y < 0.5)
            {
                Vector3 currentVelocity = (bounceVelocity + gravity * timer);
                Vector3 newVelocity = currentVelocity - 2f * Vector3.Dot(currentVelocity, Vector3.up) * Vector3.up;
                float coef = 1f;
                newVelocity = ((coef * 1000 * (newVelocity)) + (1 * newVelocity)) / (1 + 1000);
                bouncePosition = calculatePosition;
                bounceVelocity = newVelocity;
                timer = 0;
            }

            ballPositions[time] = calculatePosition;
        }
    }
    public void CalculateObjectif()
    { 
    Vector3 player1Target = Vector3.positiveInfinity;
        Vector3 player2Target = Vector3.positiveInfinity;
        foreach (Vector3 ballPosition in ballPositions)
        {
            
            if (Mathf.Abs(player1Target.z - player1.transform.position.z) > Mathf.Abs(ballPosition.z - player1.transform.position.z ))
            {
                player1Target = ballPosition;
            }
            if (Mathf.Abs(player2Target.z - player2.transform.position.z) > Mathf.Abs(ballPosition.z - player2.transform.position.z))
            {
                player2Target = ballPosition;
            }
        }

        player1.Goto(player1Target);
        player2.Goto(player2Target);

    }

    private void OnDrawGizmos()
    {
        if (!running) return;
        foreach (Vector3 ballPosition in ballPositions)
        {
            Gizmos.DrawSphere(ballPosition, 0.5f);
        }
    }

}
