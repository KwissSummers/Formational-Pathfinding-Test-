using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicArrive_Formation : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab; // For right-click obstacle placement
    [SerializeField] private Camera mainCamera;

    [SerializeField] private float obstacleBumpSpeed = 0.1f; // Speed for obstacle avoidance

    void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // Right-click for obstacle placement
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Instantiate obstacle at the clicked point on the plane
                Instantiate(obstaclePrefab, hit.point, Quaternion.identity);
            }
        }
    }

    // This is for obstacle avoidance; attach this script to followers
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag != "Obstacle")
            return;

        Vector3 toObstacle = collision.gameObject.transform.position - transform.position;
        toObstacle.Normalize();
        toObstacle.y = 0f;

        float dot = Vector3.Dot(transform.right, toObstacle);

        // Push character away from obstacle
        if (dot < 0f)
        {
            transform.position += transform.right * obstacleBumpSpeed;
        }
        else
        {
            transform.position += transform.right * -1f * obstacleBumpSpeed;
        }
    }
}
