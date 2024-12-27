using Unity.VisualScripting;
using UnityEngine;

public class PrenController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;

    private Vector3 forwardDirection = new Vector3(0,1,0);


    void Update()
    {
        // Forward/Backward movement
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(forwardDirection * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-forwardDirection * moveSpeed * Time.deltaTime);
        }

        // Left/Right rotation
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(new Vector3(0,0,1), -rotateSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(new Vector3(0,0,1), rotateSpeed * Time.deltaTime);
        }
    }
}
