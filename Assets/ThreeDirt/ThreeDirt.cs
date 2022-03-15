using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class ThreeDirt : Agent
{
    private int counter = 0;

    private float moveSpeed = 3f;

    private float turnSpeed = 300;

    private Vector3 startPosition;

    private Vector3 dirtPosition;

    new private Rigidbody rigidbody;

    [SerializeField] private GameObject[] goals = new GameObject[3];

    public override void Initialize()
    {
        dirtPosition = transform.position;

        startPosition = transform.position;

        rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;

        dirtPosition.y = 0.1f;

        for(int i = 0; i < goals.Length; i++)
        {
            goals[i].SetActive(true);
            goals[i].transform.position = dirtPosition + Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * Vector3.forward * Random.Range(5f, 15f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int vertical = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));

        int horizontal = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));

        ActionSegment<int> actions = actionsOut.DiscreteActions;

        actions[0] = vertical >= 0 ? vertical : 2;
        actions[1] = horizontal >= 0 ? horizontal : 2;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if(Vector3.Distance(startPosition, transform.position) > 25f)
        {
            AddReward(-1);
            EndEpisode();
        }

        float vertical = actions.DiscreteActions[0] <= 1 ? actions.DiscreteActions[0] : -1;
        float horizontal = actions.DiscreteActions[1] <= 1 ? actions.DiscreteActions[1] : -1;

        if (horizontal != 0f)
        {
            float angle = Mathf.Clamp(horizontal, -1f, 1f) * turnSpeed;
            transform.Rotate(Vector3.up, Time.fixedDeltaTime * angle);
        }

        Vector3 move = transform.forward * Mathf.Clamp(vertical, -1f, 1f) * moveSpeed * Time.fixedDeltaTime;
        rigidbody.MovePosition(transform.position + move);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "goal")
        {
            other.gameObject.SetActive(false);
            counter += 1;
            AddReward(1f);
            if(counter == 3)
            {
                counter = 0;
                EndEpisode();
            }
        }
    }   
}
