using System;
using Unity.VisualScripting;
using UnityEngine;

public class PrenController : MonoBehaviour
{
    private LineRendererController lineRendererController;
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;
    

    private Vector3 forwardDirection = new Vector3(0,1,0);
    private Vector3 rightDirection = new Vector3(0,0,1);
    private bool isDriving = true;
    private String DrivingMode = "start";

    void Start()
    {
        // Empty as we don't need per-frame updates
        lineRendererController = LineRendererController.Instance;
    }


    void Update()
    {
        lineRendererController = LineRendererController.Instance;
        if (isDriving && DrivingMode == "start"){
            DriveToStart();
        }
        if (isDriving && DrivingMode == "goal"){
            DriveToGoal();
        }
        if (isDriving && DrivingMode == "turn"){
            TurnToNextNode();
        }
        // Forward/Backward movement
        if (Input.GetKey(KeyCode.W))
        {
            MoveForward();
        }
        if (Input.GetKey(KeyCode.S))
        {
            MoveBackward();
        }

        // Left/Right rotation
        if (Input.GetKey(KeyCode.A))
        {
            RotateLeft();
        }
        if (Input.GetKey(KeyCode.D))
        {
            RotateRight();
        }
    }

    public void DriveToStart(){
        isDriving = true;
        if (IsFacingNode(lineRendererController.nodes[0].transform)){
            Debug.Log("Facing node");
            if (lineRendererController.nodes[0].transform.position == transform.position){
                isDriving = false;
                DrivingMode = "goal";
            }
            if (lineRendererController.nodes[0].transform.position != transform.position){
                MoveForward();
            }
        }
        else{
            RotateRight();
            Debug.Log("Turning to face node");

        }
    }
    public void DriveToGoal(){
        
    }
    public void TurnToNextNode(){
        
    }

 

    public void LiftBarrier(){
        
    }
    private bool IsFacingNode(Transform targetNode, float angleThreshold = 10f)
{
    // Get direction to target
    Vector3 directionToTarget = (targetNode.position - transform.position).normalized;
    
    // Get forward direction
    Vector3 forward = forwardDirection;
    
    // Calculate angle between directions
    float angle = Vector3.Angle(forward, directionToTarget);
    
    // Check if within threshold
    return angle <= angleThreshold;
}

    public void MoveForward()
    {
        transform.Translate(forwardDirection * moveSpeed * Time.deltaTime);
    }

    public void MoveBackward()
    {
        transform.Translate(-forwardDirection * moveSpeed * Time.deltaTime);
    }

    public void RotateLeft()
    {
        transform.Rotate(rightDirection, -rotateSpeed * Time.deltaTime);
    }

    public void RotateRight()
    {
        transform.Rotate(rightDirection, rotateSpeed * Time.deltaTime);
    }
}
