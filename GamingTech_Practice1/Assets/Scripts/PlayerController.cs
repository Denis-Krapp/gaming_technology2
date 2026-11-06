using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 8.0f;

    void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveDirection += Vector3.right;

        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection.normalized * speed * Time.deltaTime;
        }
    }
}