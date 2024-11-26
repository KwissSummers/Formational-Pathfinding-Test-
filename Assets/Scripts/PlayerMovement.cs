using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	[SerializeField] private Transform trans;
	[SerializeField] private Rigidbody rb;

	private float moveSpeed;
	private float turnSpeed;

	private void Start () {
		moveSpeed = 10f;
		turnSpeed = 10f;
	}

	private void Update () {

		Vector3 inputVector = Vector3.zero;

		// Check for movement input
		if (Input.GetKey (KeyCode.UpArrow)) {
			inputVector += Vector3.forward;
		}
		else if (Input.GetKey (KeyCode.DownArrow)) {
			inputVector += Vector3.back;
		}

		if (Input.GetKey (KeyCode.LeftArrow)) {
			inputVector += Vector3.left;
		}
		else if (Input.GetKey (KeyCode.RightArrow)) {
			inputVector += Vector3.right;
		}

		if (inputVector != Vector3.zero) {

			// Normalize input vector to standardize movement speed
			inputVector.Normalize ();
			inputVector *= moveSpeed;
			rb.velocity = inputVector;

			// Face player along movement vector
			Quaternion targetRotation = Quaternion.LookRotation (inputVector);
			trans.rotation = Quaternion.Lerp (trans.rotation, targetRotation, turnSpeed * Time.deltaTime);
		}
		else {
			rb.velocity = Vector3.zero;
		}
	}
}
