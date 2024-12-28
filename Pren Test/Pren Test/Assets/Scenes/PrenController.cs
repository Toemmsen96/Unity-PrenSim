using System;
using Unity.VisualScripting;
using UnityEngine;

public class PrenController : MonoBehaviour
{
    private LineRendererController lineRendererController;
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;

    public int goalNodeIndex = 1;
    

    private Vector3 forwardDirection = new Vector3(1,0,0);
    private Vector3 rightDirection = new Vector3(0,-1,0);
    private bool isDriving = true;
    private float POSITION_TOLERANCE = 2f;
    private Vector3 targetDirection;
    private DrivingMode  drivingMode = DrivingMode.start;

    enum DrivingMode{
        start,
        goal,
        turn,
        drive,
        none
    }


    void Start()
    {
        // Empty as we don't need per-frame updates
        lineRendererController = LineRendererController.Instance;
    }


    void Update()
    {
        lineRendererController = LineRendererController.Instance;
        if (isDriving && drivingMode == DrivingMode.start){
            DriveToStart();
        }
        if (isDriving && drivingMode == DrivingMode.goal){
            DriveToGoal();
        }
        if (isDriving && drivingMode == DrivingMode.turn){
            //TurnToNextNode();
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

            if (Vector3.Distance(lineRendererController.nodes[0].transform.position, transform.position) <= POSITION_TOLERANCE)
            {
                Debug.Log("Reached node");
                drivingMode = DrivingMode.goal;
                DriveToGoal();
            }
            if (lineRendererController.nodes[0].transform.position != transform.position){
                MoveForward();
            }
        }
        else{
            targetDirection = GetDirectionToTarget(lineRendererController.nodes[0].transform);
            TurnToNextNode(lineRendererController.nodes[0].transform);
            //Debug.Log("Turning to face node");

        }
    }
    public void DriveToGoal(){
                isDriving = true;
        if (IsFacingNode(lineRendererController.nodes[goalNodeIndex].transform)){
            Debug.Log("Facing node");

            if (Vector3.Distance(lineRendererController.nodes[goalNodeIndex].transform.position, transform.position) <= POSITION_TOLERANCE)
            {
                isDriving = false;
                drivingMode = DrivingMode.none;
            }
            if (lineRendererController.nodes[goalNodeIndex].transform.position != transform.position){
                MoveForward();
            }
        }
        else{
            targetDirection = GetDirectionToTarget(lineRendererController.nodes[0].transform);
            TurnToNextNode(lineRendererController.nodes[0].transform);
            //Debug.Log("Turning to face node");

        }
    }
    public void TurnToNextNode(Transform node){
            //Debug.Log("Turning to face node");
            targetDirection = GetDirectionToTarget(node);
            if (Vector3.Angle(transform.forward, targetDirection) > 0){
                RotateLeft();
            }
            else{
                RotateRight();
            }        
    }

 

    public void LiftBarrier(){
        
    }
    private bool IsFacingNode(Transform targetNode, float angleThreshold = 10f)
    {
        // Get direction to target and flatten to XZ plane
        Vector3 directionToTarget = GetDirectionToTarget(targetNode);
        Vector3 flatDirectionToTarget = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        
        // Get forward direction flattened to XZ plane and rotate 90 degrees
        Vector3 flatForward = new Vector3(transform.right.x, 0, transform.right.z).normalized;
        
        // Calculate angle between flattened directions
        float angle = Vector3.Angle(flatForward, flatDirectionToTarget);
        Debug.Log("XZ Plane Angle: " + angle);
        
        // Check if within threshold
        return angle <= angleThreshold;
    }
public Vector3 GetDirectionToTarget(Transform targetNode)
{
    if (targetNode == null)
    {
        Debug.LogWarning("Target node is null!");
        return Vector3.zero;
    }

    // Get positions (ignoring Y axis for 2D movement)
    Vector3 targetPos = new Vector3(targetNode.position.x, 0, targetNode.position.z);
    Vector3 currentPos = new Vector3(transform.position.x, 0, transform.position.z);
    
    // Calculate direction
    Vector3 direction = (targetPos - currentPos).normalized;
    
    // Optional: Debug visualization
    Debug.DrawRay(currentPos, direction * 5f, Color.red);
    
    return direction;
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
