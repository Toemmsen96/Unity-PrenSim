using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class PrenController : MonoBehaviour
{
    private LineRendererController lineRendererController;
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;

    public int goalNodeIndex;
    public GameObject explosionPrefab;

    private Vector3 forwardDirection = new Vector3(1,0,0);
    private Vector3 rightDirection = new Vector3(0,-1,0);
    private bool isDriving = true;
    public float POSITION_TOLERANCE = 2f;
    private Vector3 targetDirection;
    private DrivingMode  drivingMode = DrivingMode.start;
    private int nextNode = 0;
    private int currentNode = -1;
    private Text infoText;
    private Dictionary<int,String> goalNodes = new Dictionary<int, String>(){
        {3, "C"},
        {4, "A"},
        {5, "B"}
    };


    enum DrivingMode{
        start,
        goal,
        turn,
        drive,
        none
    }


    void Start() {
        StartCoroutine(WaitForInitialization());
    }

    private IEnumerator WaitForInitialization() {
        lineRendererController = LineRendererController.Instance;
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) {
            GameObject infos = canvas.transform.Find("Infos").gameObject;
            if (infos != null) {
                infoText = infos.GetComponentInChildren<Text>();
            }
        }
        while (lineRendererController == null || !lineRendererController.IsInitialized) {
            Debug.Log("Waiting for LineRendererController to initialize...");
            yield return null;
        }
        lineRendererController = LineRendererController.Instance;
        goalNodeIndex = lineRendererController.GOAL_NODE;
        Debug.Log("Goal node is: " + goalNodeIndex);
        if (infoText != null) {
            infoText.text = "Goal Node: " + goalNodes[goalNodeIndex];
        }
        
        // Continue with initialization that depends on LineRendererController
    }


    void Update()
    {
        if (isDriving && drivingMode == DrivingMode.start){
            DriveToStart();
        }
        if (isDriving && drivingMode == DrivingMode.goal){
            DriveToGoal();
        }
        if (isDriving && drivingMode == DrivingMode.turn){
            //TurnToNextNode();
        }
        if (isDriving && drivingMode == DrivingMode.drive){
            DriveToNextNode();
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
                drivingMode = DrivingMode.drive;
                isDriving = false;
                currentNode = 0;
                FindNextPath();
                DriveToNextNode();
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
                Debug.Log("Reached Goal!");
            }
            if (lineRendererController.nodes[goalNodeIndex].transform.position != transform.position){
                MoveForward();
            }
        }
        else{
            targetDirection = GetDirectionToTarget(lineRendererController.nodes[goalNodeIndex].transform);
            TurnToNextNode(lineRendererController.nodes[goalNodeIndex].transform);
            //Debug.Log("Turning to face node");

        }
    }
    public void DriveToNextNode(){
        isDriving = true;
        if (nextNode == -1){
            isDriving = false;
            drivingMode = DrivingMode.none;
            Debug.Log("Reached Goal!");
            infoText.text = "Reached Goal!";
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            return;
        }
        else if (IsFacingNode(lineRendererController.nodes[nextNode].transform)){
            Debug.Log("Facing node");

            if (Vector3.Distance(lineRendererController.nodes[nextNode].transform.position, transform.position) <= POSITION_TOLERANCE)
            {
                Debug.Log("Reached node");
                currentNode = nextNode;
                FindNextPath();
            }
            else if (lineRendererController.nodes[nextNode].transform.position != transform.position){
                MoveForward();
            }
        }
        else{
            targetDirection = GetDirectionToTarget(lineRendererController.nodes[nextNode].transform);
            TurnToNextNode(lineRendererController.nodes[nextNode].transform);
            //Debug.Log("Turning to face node");

        }
    }
    public void TurnToNextNode(Transform node){
            //Debug.Log("Turning to face node");
            targetDirection = GetDirectionToTarget(node);
            if (GetAngleToTarget(node) > 0){
                RotateLeft();
            }
            else{
                RotateRight();
            }        
    }

    public bool ConnectionExists(int nodeA, int nodeB){
        int min = Math.Min(nodeA, nodeB);
        int max = Math.Max(nodeA, nodeB);
        Debug.Log("Checking for connection between " + min + " and " + max);
        return lineRendererController.connections.Exists(x => x.GetStart() == min && x.GetEnd() == max);
    }

 

    public void LiftBarrier(){
        //TODO: Implement
    }
    private bool IsFacingNode(Transform targetNode, float angleThreshold = 5f)
    {
        float angle = GetAngleToTarget(targetNode);
        angle = Mathf.Abs(angle);
        Debug.Log("Absolute Angle: " + angle);
        
        // Check if within threshold
        return angle <= angleThreshold;
    }
    private float GetAngleToTarget(Transform targetNode)
{
         // Get direction to target and flatten to XZ plane
        Vector3 directionToTarget = GetDirectionToTarget(targetNode);
        Vector3 flatDirectionToTarget = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        
        // Get forward direction flattened to XZ plane
        Vector3 flatForward = new Vector3(transform.right.x, 0, transform.right.z).normalized;
        
        // Calculate signed angle between directions (gives -180 to +180)
        float angle = Vector3.SignedAngle(flatForward, flatDirectionToTarget, Vector3.up);
        
        Debug.Log("Angle: " + angle);
        return angle;
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
    //Debug.Log("Direction: " + direction);
    
    // Optional: Debug visualization
    Debug.DrawRay(currentPos, direction * 5f, Color.red);
    
    return direction;
}

    private void FindNextPath() {
    if (currentNode == goalNodeIndex) {
        nextNode = -1;
        return;
    }

    var queue = new System.Collections.Generic.Queue<int>();
    var visited = new System.Collections.Generic.HashSet<int>();
    var parentMap = new System.Collections.Generic.Dictionary<int, int>();
    
    queue.Enqueue(currentNode);
    visited.Add(currentNode);

       while (queue.Count > 0) {
        int node = queue.Dequeue();
        
        if (node == goalNodeIndex) {
            // Reconstruct path from goal to current
            int current = goalNodeIndex;
            while (parentMap[current] != currentNode) {
                current = parentMap[current];
            }
            nextNode = current;
            return;
        }
    
        foreach (var connection in lineRendererController.connections) {
            if (connection.GetStart() == node && !visited.Contains(connection.GetEnd())) {
                if (!lineRendererController.HasConeAtNode(connection.GetEnd())) {
                    visited.Add(connection.GetEnd());
                    parentMap[connection.GetEnd()] = node;
                    queue.Enqueue(connection.GetEnd());
                }
            }
            if (connection.GetEnd() == node && !visited.Contains(connection.GetStart())) {
                if (!lineRendererController.HasConeAtNode(connection.GetStart())) {
                    visited.Add(connection.GetStart());
                    parentMap[connection.GetStart()] = node;
                    queue.Enqueue(connection.GetStart());
                }
            }
        }
    }

    // If no path to goal found, use existing connection logic
    foreach (var connection in lineRendererController.connections) {
        if (connection.GetStart() == currentNode) {
            nextNode = connection.GetEnd();
            break;
        }
        if (connection.GetEnd() == currentNode) {
            nextNode = connection.GetStart();
            break;
        }
    }
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
