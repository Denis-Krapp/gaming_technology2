using UnityEngine;

public class TargetController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0f) * moveSpeed * Time.deltaTime;
        transform.position += move;
    }
}