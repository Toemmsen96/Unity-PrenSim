using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

public class PrenController : MonoBehaviour
{
    private LineRendererController lineRendererController;
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;

    public int goalNodeIndex;
    public GameObject explosionPrefab;
    public bool explode = false;

    private Vector3 forwardDirection = new Vector3(1,0,0);
    private Vector3 rightDirection = new Vector3(0,-1,0);
    private bool isDriving = true;
    public float positionTolerance = 2f;
    public float defaultAngleThreshold = 5f;
    public float lineThreshold = 1f;
    public float barrierDistance = 1f;
    private Vector3 targetDirection;
    private DrivingMode  drivingMode = DrivingMode.start;
    private int nextNode = 0;
    private int currentNode = -1;
    private Text infoText;
    public bool moveBarrier = false;
    private BarrierLiftState barrierState = BarrierLiftState.LIFTING;
    private float backupStartTime;
    public float backupDuration = 1.0f;
    private LineRendererController.Barrier barrierToMove;
    private GameObject tempBarrier;
    private LineRendererController.Barrier originalBarrier;
    private bool displayMenu = false;


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
        barriermode,
        recenter,
        none
    }
    private enum BarrierLiftState {
        LIFTING,
        TURNING_WITH_BARRIER,
        BACKING_UP,
        LOWERING,
        TURNING_TO_NEXT,
        DONE
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
        goalNodeIndex = lineRendererController.goalNode;
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
            //TurnToTransform();
        }
        if (isDriving && drivingMode == DrivingMode.drive){
            DriveToNextNode();
        }
        if (!isDriving && drivingMode == DrivingMode.barriermode){
            LiftBarrier(IsInFrontOfBarrier());
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
        if (Input.GetKeyDown(KeyCode.R)){
            infoText.text = "Restarting...";
            drivingMode = DrivingMode.start;
            DriveToStart();
        }
        if (Input.GetKeyDown(KeyCode.G)){
            infoText.text = "Driving to goal...";
            drivingMode = DrivingMode.goal;
            DriveToGoal();
        }
        if (Input.GetKeyDown(KeyCode.E)){
            infoText.text = "Resetting...";
            drivingMode = DrivingMode.none;
            Reset();
        }
        if (Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }
        if (displayMenu){
            //TODO
        }
    }
    public LineRendererController.Barrier IsInFrontOfBarrier(){
        foreach (var barrier in lineRendererController.barriers){
            Vector2 barrierPos2D = new Vector2(barrier.barrierObject.transform.position.x, barrier.barrierObject.transform.position.z);
            Vector2 robotPos2D = new Vector2(transform.position.x, transform.position.z);
            if (Vector3.Distance(barrierPos2D,robotPos2D) <= barrierDistance){
                if ((barrier.GetConnection().GetStart() == currentNode || barrier.GetConnection().GetEnd() == currentNode)&& (barrier.GetConnection().GetStart() == nextNode || barrier.GetConnection().GetEnd() == nextNode) && IsFacingNode(barrier.barrierObject.transform, 90)){
                    moveBarrier = true;
                    return barrier;
                }
            }
        }
        return null;
    }

    public void DriveToStart(){
        isDriving = true;
        if (IsFacingNode(lineRendererController.nodes[0].transform, defaultAngleThreshold)){
            Debug.Log("Facing node");

            Vector2 nodePos2D = new Vector2(lineRendererController.nodes[0].transform.position.x, lineRendererController.nodes[0].transform.position.z);
            Vector2 robotPos2D = new Vector2(transform.position.x, transform.position.z);
            if (Vector2.Distance(nodePos2D, robotPos2D) <= positionTolerance)
            {
                Debug.Log("Reached node");
                drivingMode = DrivingMode.drive;
                isDriving = false;
                currentNode = 0;
                infoText.text = "Goal Node: " + goalNodes[goalNodeIndex];
                FindNextPath();
                DriveToNextNode();
            }
            if (lineRendererController.nodes[0].transform.position != transform.position){
                MoveForward();
            }
        }
        else{
            targetDirection = GetDirectionToTarget(lineRendererController.nodes[0].transform);
            TurnToTransform(lineRendererController.nodes[0].transform);
            //Debug.Log("Turning to face node");

        }
    }

    
    public void DriveToGoal(){
        isDriving = true;
        if (IsFacingNode(lineRendererController.nodes[goalNodeIndex].transform,defaultAngleThreshold)){
            Debug.Log("Facing node");

            Vector2 nodePos2D = new Vector2(lineRendererController.nodes[goalNodeIndex].transform.position.x, lineRendererController.nodes[goalNodeIndex].transform.position.z);
            Vector2 robotPos2D = new Vector2(transform.position.x, transform.position.z);
            if (Vector2.Distance(nodePos2D,robotPos2D) <= positionTolerance)
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
            TurnToTransform(lineRendererController.nodes[goalNodeIndex].transform);
            //Debug.Log("Turning to face node");

        }
    }
    public bool IsOnLine(LineRendererController.Connection connection, float threshold) {
        if (connection == null) {
            return true;
        }
        Vector2 robotPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 startNodePos2D = new Vector2(lineRendererController.nodes[connection.GetStart()].transform.position.x, 
                                           lineRendererController.nodes[connection.GetStart()].transform.position.z);
        Vector2 endNodePos2D = new Vector2(lineRendererController.nodes[connection.GetEnd()].transform.position.x, 
                                         lineRendererController.nodes[connection.GetEnd()].transform.position.z);
    
        Vector2 lineDirection = (endNodePos2D - startNodePos2D).normalized;
        Vector2 robotToStart = robotPos2D - startNodePos2D;
        
        float dotProduct = Vector2.Dot(robotToStart, lineDirection);
        Vector2 projection = startNodePos2D + lineDirection * dotProduct;
        float distance = Vector2.Distance(robotPos2D, projection);
        
        return distance < threshold;
    }

    public Transform GetClosestPointOnLine(LineRendererController.Connection connection) {
        Vector2 robotPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 startNodePos2D = new Vector2(lineRendererController.nodes[connection.GetStart()].transform.position.x, 
                                           lineRendererController.nodes[connection.GetStart()].transform.position.z);
        Vector2 endNodePos2D = new Vector2(lineRendererController.nodes[connection.GetEnd()].transform.position.x, 
                                         lineRendererController.nodes[connection.GetEnd()].transform.position.z);
    
        Vector2 lineDirection = (endNodePos2D - startNodePos2D).normalized;
        Vector2 robotToStart = robotPos2D - startNodePos2D;
        float dotProduct = Vector2.Dot(robotToStart, lineDirection);
        
        // Clamp projection to line segment
        float lineLength = Vector2.Distance(startNodePos2D, endNodePos2D);
        dotProduct = Mathf.Clamp(dotProduct, 0, lineLength);
        
        Vector2 projection = startNodePos2D + lineDirection * dotProduct;
        
        // Create temporary GameObject at projection point
        GameObject projectionPoint = new GameObject("ProjectionPoint");
        projectionPoint.transform.position = new Vector3(projection.x, transform.position.y, projection.y);
        
        return projectionPoint.transform;
    }

    public LineRendererController.Connection GetConnection(int nodeA, int nodeB){
        foreach (var connection in lineRendererController.connections){
            if (connection.GetStart() == nodeA && connection.GetEnd() == nodeB){
                return connection;
            }
            if (connection.GetStart() == nodeB && connection.GetEnd() == nodeA){
                return connection;
            }
        }
        return null;
    }

    public LineRendererController.Connection GetClosestConnection(){
        float minDistance = float.MaxValue;
        LineRendererController.Connection closestConnection = null;
        foreach (var connection in lineRendererController.connections){
            Vector2 robotPos2D = new Vector2(transform.position.x, transform.position.z);
            Vector2 startNodePos2D = new Vector2(lineRendererController.nodes[connection.GetStart()].transform.position.x, 
                                           lineRendererController.nodes[connection.GetStart()].transform.position.z);
            Vector2 endNodePos2D = new Vector2(lineRendererController.nodes[connection.GetEnd()].transform.position.x, 
                                         lineRendererController.nodes[connection.GetEnd()].transform.position.z);
            Vector2 lineDirection = (endNodePos2D - startNodePos2D).normalized;
            Vector2 robotToStart = robotPos2D - startNodePos2D;
            float dotProduct = Vector2.Dot(robotToStart, lineDirection);
            Vector2 projection = startNodePos2D + lineDirection * dotProduct;
            float distance = Vector2.Distance(robotPos2D, projection);
            if (distance < minDistance){
                minDistance = distance;
                closestConnection = connection;
            }
        }
        return closestConnection;
    }

    public void RecenterOnLine(){
        TurnToTransform(GetClosestPointOnLine(GetConnection(currentNode, nextNode)));
        MoveForward();
        if (IsOnLine(GetConnection(currentNode,nextNode), lineThreshold)){
            TurnToTransform(lineRendererController.nodes[nextNode].transform);
        }
        //TODO
    }


    public void DriveToNextNode(){
        isDriving = true;
        barrierToMove = IsInFrontOfBarrier();
        Debug.Log("IsOnLine: " + IsOnLine(GetConnection(currentNode,nextNode), lineThreshold));
        if (currentNode == goalNodeIndex){
            Finish();
            return;
        }
        else if (!IsOnLine(GetConnection(currentNode,nextNode), lineThreshold)){
            Debug.Log("Not on line");
            RecenterOnLine();
            return;

        }
        else if (nextNode == -1){
            isDriving = false;
            Debug.Log("No path found");
            return;
        }
        else if (barrierToMove != null){
            Debug.Log("Barrier in front");
            drivingMode = DrivingMode.barriermode;
            barrierState = BarrierLiftState.LIFTING;
            isDriving = false;
            LiftBarrier(barrierToMove);
            return;
        }
        else if (IsFacingNode(lineRendererController.nodes[nextNode].transform, defaultAngleThreshold)){
            Debug.Log("Facing node");
            Vector2 nodePos2D = new Vector2(lineRendererController.nodes[nextNode].transform.position.x, lineRendererController.nodes[nextNode].transform.position.z);
            Vector2 robotPos2D = new Vector2(transform.position.x, transform.position.z);
            if (Vector2.Distance(nodePos2D,robotPos2D) <= positionTolerance)
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
            TurnToTransform(lineRendererController.nodes[nextNode].transform);
            //Debug.Log("Turning to face node");

        }
    }
    public void TurnToTransform(Transform node){
            //Debug.Log("Turning to face node");
            targetDirection = GetDirectionToTarget(node);
            if (GetAngleToTarget(node) > 0){
                RotateRight();
            }
            else{
                RotateLeft();
            }        
    }

    public bool ConnectionExists(int nodeA, int nodeB){
        int min = Math.Min(nodeA, nodeB);
        int max = Math.Max(nodeA, nodeB);
        Debug.Log("Checking for connection between " + min + " and " + max);
        return lineRendererController.connections.Exists(x => x.GetStart() == min && x.GetEnd() == max);
    }

 
public void LiftBarrier(LineRendererController.Barrier barrier) {
    if (!moveBarrier) {
        targetDirection = GetDirectionToTarget(lineRendererController.nodes[nextNode].transform);
        TurnToTransform(lineRendererController.nodes[nextNode].transform);
        DriveToNextNode();
        return;
    }

    switch (barrierState) {
        case BarrierLiftState.LIFTING:
            if (tempBarrier == null) {
                originalBarrier = barrier;
                tempBarrier = GameObject.Instantiate(barrier.GetBarrier(), barrier.GetBarrier().transform.position, barrier.GetBarrier().transform.rotation);
                originalBarrier.SetActive(false);
            }
            
            tempBarrier.transform.Translate(Vector3.up * 2f * Time.deltaTime);
            if (tempBarrier.transform.position.y >= 0.5f) {
                tempBarrier.transform.SetParent(transform);
                barrierState = BarrierLiftState.TURNING_WITH_BARRIER;
            }
            break;
        
        case BarrierLiftState.TURNING_WITH_BARRIER:
            TurnToTransform(lineRendererController.nodes[currentNode].transform);
            if (IsFacingNode(lineRendererController.nodes[currentNode].transform, defaultAngleThreshold)) {
                backupStartTime = Time.time;
                barrierState = BarrierLiftState.BACKING_UP;
            }
            break;
        case BarrierLiftState.BACKING_UP:
            if (Time.time - backupStartTime >= backupDuration) {
                barrierState = BarrierLiftState.LOWERING;
            }
            MoveBackward();
            break;
        
        case BarrierLiftState.LOWERING:
            tempBarrier.transform.SetParent(null);
            tempBarrier.transform.Translate(Vector3.down * 2f * Time.deltaTime);
            MoveBackward();
            if (tempBarrier.transform.position.y <= 0.25f) {
                GameObject.Destroy(tempBarrier);
                originalBarrier.SetActive(true);
                barrierState = BarrierLiftState.TURNING_TO_NEXT;
            }
            break;

        case BarrierLiftState.TURNING_TO_NEXT:
            targetDirection = GetDirectionToTarget(lineRendererController.nodes[nextNode].transform);
            TurnToTransform(lineRendererController.nodes[nextNode].transform);
            if (IsFacingNode(lineRendererController.nodes[nextNode].transform, defaultAngleThreshold)) {
                barrierState = BarrierLiftState.DONE;
                moveBarrier = false;
                isDriving = true;
                drivingMode = DrivingMode.drive;
                DriveToNextNode();
            }
            break;
    }
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
            if (connection.IsEnabled()){
                Debug.Log("Checking connection between " + connection.GetStart() + " and " + connection.GetEnd());
                if (connection.GetStart() == node && !visited.Contains(connection.GetEnd())) {
                    if (!lineRendererController.HasConeAtNode(connection.GetEnd()) ) {
                        visited.Add(connection.GetEnd());
                        parentMap[connection.GetEnd()] = node;
                        queue.Enqueue(connection.GetEnd());
                        Debug.Log("Adding " + connection.GetEnd() + " to queue");
                    }
                }
                if (connection.GetEnd() == node && !visited.Contains(connection.GetStart())) {
                    if (!lineRendererController.HasConeAtNode(connection.GetStart())) {
                        visited.Add(connection.GetStart());
                        parentMap[connection.GetStart()] = node;
                        queue.Enqueue(connection.GetStart());
                        Debug.Log("Adding " + connection.GetStart() + " to queue");
                    }
                }
            }
        }
    }
    Debug.Log("No path found");
    infoText.text = "No path found!";
    nextNode = -1;
/*
        // If no path to goal found, use existing connection logic
        foreach (var connection in lineRendererController.connections) {
            Debug.Log("Checking connection between " + connection.GetStart() + " and " + connection.GetEnd());
            if (connection.GetStart() == currentNode) {
                nextNode = connection.GetEnd();
                break;
            }
            if (connection.GetEnd() == currentNode) {
                nextNode = connection.GetStart();
                break;
            }
        }
        */
    }

    public void Finish(){
        isDriving = false;
        drivingMode = DrivingMode.none;
        infoText.text = "Reached Goal!";
        Debug.Log("Reached Goal!");
        if (explode) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
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
        transform.Rotate(rightDirection, rotateSpeed * Time.deltaTime);
    }

    public void RotateRight()
    {
        transform.Rotate(rightDirection, -rotateSpeed * Time.deltaTime);
    }
    public void Reset(){
        lineRendererController.ResetField();
        Start();
        drivingMode = DrivingMode.start;
    }

    public void OpenMenu(){
        displayMenu = true;
    }
    public void CloseMenu(){
        displayMenu = false;
    }
}
