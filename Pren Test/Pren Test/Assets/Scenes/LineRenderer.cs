using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class LineRendererController : MonoBehaviour
{
    //public LineRenderer lineRenderer;
    public bool IsInitialized { get; private set; } = false;
    public GameObject nodePrefab;
    public GameObject conePrefab;
    public GameObject barrierPrefab;
    public GameObject linePrefab;
    public List<Connection> connections = new List<Connection>();
    public List<Barrier> barriers = new List<Barrier>();

    public GameObject[] nodes;
    public GameObject[] cones;
    public GameObject[] lines;
    public int[] coneIndices;
    public  bool fixedInitiation = true;
    public bool randomNodes = false;
    public float randomNodeRange = 0.01f;
    public int goalNode = 0; // cant be 0, randomizes if 0

    public int DisableConnectionsAmount = 0;

    public float DistanceMultiplier = 4;

    public int maxCones = 2;

    public int maxBarriers = 2;

    public int maxPoints = 8;

    public int maxLines = 15;

    void Start()
    {
        Instance = this;
        nodes = new GameObject[maxPoints]; // Initialize the array with the correct size
        cones = new GameObject[maxCones]; // Initialize the array with the correct size
        lines = new GameObject[maxLines]; // Initialize the array with the correct size
        coneIndices = new int[maxCones]; // Initialize the array with the correct size
        if (goalNode == 0)
        {
        goalNode = UnityEngine.Random.Range(3, 6);
        Debug.Log("Goal node is: " + goalNode);
        }

        //lineRenderer.positionCount = nodes.Length;
        if (fixedInitiation)
        {
            // Hexagon
            nodes[0] = Instantiate(nodePrefab, new Vector3(0, 0, 0), Quaternion.identity); // Start node
            nodes[1] = Instantiate(nodePrefab, new Vector3(1*DistanceMultiplier, 0, 1*DistanceMultiplier), Quaternion.identity); // Left node
            nodes[2] = Instantiate(nodePrefab, new Vector3(1*DistanceMultiplier, 0, -1*DistanceMultiplier), Quaternion.identity); // Right node
            nodes[3] = Instantiate(nodePrefab, new Vector3(2*DistanceMultiplier, 0, 1*DistanceMultiplier), Quaternion.identity); // Left node
            nodes[4] = Instantiate(nodePrefab, new Vector3(2*DistanceMultiplier, 0, -1*DistanceMultiplier), Quaternion.identity); // Right node
            nodes[5] = Instantiate(nodePrefab, new Vector3(3*DistanceMultiplier, 0, 0), Quaternion.identity); // top node
            // extra nodes
            nodes[6] = Instantiate(nodePrefab, new Vector3(1*DistanceMultiplier, 0, -0.5f*DistanceMultiplier), Quaternion.identity); // Center node
            nodes[7] = Instantiate(nodePrefab, new Vector3(2*DistanceMultiplier, 0, 0), Quaternion.identity); // Center node
            for (int i = 0; i < maxLines; i++)
            {
                GameObject line = Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                lines[i] = line;
            }
            GenerateRandomCones();
            GenerateConnections();
            DisableRandomConnections();
            GenerateRandomBarriers();

        }
        else{
            for (int i = 0; i < maxPoints; i++)
            {
                if (i == 0)
                {
                    nodes[i] = Instantiate(nodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                }
                else
                {
                    nodes[i] = Instantiate(nodePrefab, new Vector3(i + 5*(i%2), 0, i-5*(i%2)), Quaternion.identity);
                }
                
            }
        }
        IsInitialized = true;


    }
    public void ConnectPoints(GameObject connector, Vector3 startPos, Vector3 endPos)
    {
        // Get direction and distance
        Vector3 direction = endPos - startPos;
        float distance = direction.magnitude;
        
        // Position at midpoint
        Vector3 midPoint = startPos + (direction * 0.5f);
        connector.transform.position = midPoint;
        
        // Rotate to face direction
        connector.transform.rotation = Quaternion.LookRotation(direction);
        
        // Scale length to match distance
        Vector3 scale = connector.transform.localScale;
        scale.z = distance;  // Assuming object's forward axis is Z
        connector.transform.localScale = scale;
    }
    public void DisableRandomConnections(){
        List<Connection> availableConnections = new List<Connection>(connections);
        for (int i = 0; i < DisableConnectionsAmount && availableConnections.Count > 0; i++) {
            int randomIndex = UnityEngine.Random.Range(0, availableConnections.Count);
            Connection selectedConnection = availableConnections[randomIndex];
            selectedConnection.Disable();
            availableConnections.RemoveAt(randomIndex);
        }
    }

    public void GenerateRandomBarriers() {
        List<Connection> availableConnections = new List<Connection>(connections);
        List<Connection> barrierConnections = new List<Connection>();
        
        for (int i = 0; i < maxBarriers && availableConnections.Count > 0; i++) {
            int randomIndex = UnityEngine.Random.Range(0, availableConnections.Count);
            Connection selectedConnection = availableConnections[randomIndex];
            if (!selectedConnection.IsEnabled())
            {
                i--;
                continue;
            }
            
            // Get node positions
            Vector3 startPos = nodes[selectedConnection.GetStart()].transform.position;
            Vector3 endPos = nodes[selectedConnection.GetEnd()].transform.position;
            
            // Calculate midpoint
            Vector3 midpoint = (startPos + endPos) / 2f;
            
            // Create barrier at midpoint
            Barrier barrier = new Barrier(Instantiate(barrierPrefab), selectedConnection);
            midpoint.y = 0.25f;
            barrier.barrierObject.transform.position = midpoint;
            barrier.barrierObject.transform.LookAt(endPos);
            barrier.barrierObject.transform.rotation = Quaternion.Euler(0, barrier.barrierObject.transform.rotation.eulerAngles.y + 90, 0);
            barrier.barrierObject.name = "Barrier_" + i;
            barriers.Add(barrier);
            
            // Disable connection
            //selectedConnection.Disable();
            
            // Remove from available connections
            availableConnections.RemoveAt(randomIndex);
        }
    }

    void Update()
    {
        //ConnectPoints(lines[0], nodes[0].transform.position, nodes[1].transform.position); // Start to left
        //ConnectPoints(lines[1], nodes[0].transform.position, nodes[6].transform.position); // Start to Center 1
        //ConnectPoints(lines[2], nodes[0].transform.position, nodes[2].transform.position); // Start to right
        //ConnectPoints(lines[3], nodes[1].transform.position, nodes[6].transform.position); // Left to Center 1
        //ConnectPoints(lines[4], nodes[2].transform.position, nodes[6].transform.position); // Right to Center 1
        //ConnectPoints(lines[5], nodes[1].transform.position, nodes[3].transform.position); // Left to left
        //ConnectPoints(lines[6], nodes[2].transform.position, nodes[4].transform.position); // Right to right
        //ConnectPoints(lines[7], nodes[3].transform.position, nodes[4].transform.position); // Left to right
        if (randomNodes){
        foreach (GameObject node in nodes)
        {
            node.transform.position = new Vector3(
                node.transform.position.x + UnityEngine.Random.Range(-randomNodeRange, randomNodeRange), 
                node.transform.position.y, 
                node.transform.position.z + UnityEngine.Random.Range(-randomNodeRange, randomNodeRange)
            );
        }}
        DrawLines();

    }
    private void DrawLines(){
        int i = 0;
        foreach (Connection connection in connections)
        {
            if (connection.IsEnabled())
            {
                ConnectPoints(lines[i], nodes[connection.GetStart()].transform.position, nodes[connection.GetEnd()].transform.position);
            i++;
            }
        }
    }
    private void GenerateRandomCones()
    {
        // Generate random cones
        for (int i = 0; i < maxCones; i++)
        {
            int randomIndex;
            do{
            randomIndex = UnityEngine.Random.Range(1, maxPoints-1);
            } while (Array.Exists(coneIndices, element => element == randomIndex)|| randomIndex == goalNode);
            Debug.Log("Random cone generated at index: " + randomIndex);
            coneIndices[i] = randomIndex;
            cones[i] = Instantiate(conePrefab, new Vector3(nodes[randomIndex].transform.position.x, 0, nodes[randomIndex].transform.position.z), Quaternion.identity);
        }
    }

    private void GenerateConnections(){
            connections.Add(new Connection(0, 1));
            connections.Add(new Connection(0, 6));
            connections.Add(new Connection(0, 2));
            connections.Add(new Connection(1, 3));
            connections.Add(new Connection(1, 6));
            connections.Add(new Connection(1, 7));
            connections.Add(new Connection(2, 4));
            connections.Add(new Connection(2, 6));
            connections.Add(new Connection(3, 5));
            connections.Add(new Connection(3, 7));
            connections.Add(new Connection(4, 5));
            connections.Add(new Connection(4, 6));
            connections.Add(new Connection(4, 7));
            connections.Add(new Connection(5, 7));
            connections.Add(new Connection(6, 7));
    }
    public bool HasConeAtNode(int nodeIndex) {
    return Array.Exists(coneIndices, element => element == nodeIndex);
    }

    public static LineRendererController Instance { get; private set; }

    public class Connection{
        private int start;
        private int end;
        private bool isEnabled = true;
        public Connection(int start, int end){
            this.start = start;
            this.end = end;
        }
        public int GetStart(){
            return start;
        }
        public int GetEnd(){
            return end;
        }
        public bool IsEnabled(){
            return isEnabled;
        }
        public void Enable(){
            isEnabled = true;
        }
        public void Disable(){
            isEnabled = false;
        }
    }

    public class Barrier{
        public GameObject barrierObject;
        public Connection connection;
        public Barrier(GameObject barrier, Connection connection){
            this.barrierObject = barrier;
            this.connection = connection;
        }
        public GameObject GetBarrier(){
            return barrierObject;
        }
        public Connection GetConnection(){
            return connection;
        }
        public void SetConnection(Connection connection){
            this.connection = connection;
        }
        public void SetActive(bool active){
            barrierObject.SetActive(active);
        }
    }
    public bool ResetField(){
        if (IsInitialized)
        {
            foreach (GameObject node in nodes)
            {
                Destroy(node);
            }
            foreach (GameObject cone in cones)
            {
                Destroy(cone);
            }
            foreach (GameObject line in lines)
            {
                Destroy(line);
            }
            foreach (Barrier barrier in barriers)
            {
                Destroy(barrier.barrierObject);
            }
            IsInitialized = false;
            connections.Clear();
            barriers.Clear();
            Start();
            return true;
        }
        return false;
    }
}
