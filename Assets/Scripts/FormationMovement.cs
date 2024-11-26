using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationMovement : MonoBehaviour
{
    [SerializeField] private Transform formationLeader;
    [SerializeField] private float formationDistanceOffset;
    [SerializeField] private float formationRoatationOffset;

    [SerializeField] private Transform trans;
    [SerializeField] private Rigidbody rb;

    private float moveSpeed;
    private float turnSpeed;
    private float radiusOfSatisfaction;

    private void Start()
    {
        moveSpeed = 10f;
        turnSpeed = 10f;
        radiusOfSatisfaction = 1.5f;
    }

    private void Update()
    {
        // Project point forward from leader's forward facing vector
        Vector3 projectedPoint = formationLeader.forward * formationDistanceOffset;

        // Rotate that point to find this character's spot in the formation
        Vector3 positionInFormation = Quaternion.Euler(0f, formationRoatationOffset, 0f) * projectedPoint;
        positionInFormation += formationLeader.position;

        // Check if the character needs to move towards the formation point
        if (Vector3.Distance(trans.position, positionInFormation) > radiusOfSatisfaction)
        {
            // Calculate vector to the position in the formation
            Vector3 towards = positionInFormation - trans.position;

            // Normalize vector to standardize movement speed
            towards.Normalize();
            towards *= moveSpeed;
            rb.velocity = towards;

            Debug.DrawLine(trans.position, positionInFormation, Color.red);

            // Face character along movement vector
            Quaternion targetRotation = Quaternion.LookRotation(towards);
            trans.rotation = Quaternion.Lerp(trans.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        else
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Collision avoidance logic
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            // Create vector from the character to the obstacle that it just hit
            Vector3 towardsObstacle = (collision.transform.position - trans.position).normalized;
            towardsObstacle.y = 0f;  // Ensure the movement is constrained to the XZ plane

            // Calculate the dot product to determine if the obstacle is to the left or right
            float dot = Vector3.Dot(trans.right, towardsObstacle);
            Debug.Log("Dot: " + dot);

            // If obstacle is on the right, push the character to the left
            if (dot > 0)
            {
                trans.position -= towardsObstacle * 0.5f;
                Debug.Log("Avoiding obstacle: Moving left.");
            }
            // If obstacle is on the left, push the character to the right
            else
            {
                trans.position += towardsObstacle * 0.5f;
                Debug.Log("Avoiding obstacle: Moving right.");
            }

            // Optional: slow down velocity while avoiding
            rb.velocity *= 0.5f;
        }
    }
}
