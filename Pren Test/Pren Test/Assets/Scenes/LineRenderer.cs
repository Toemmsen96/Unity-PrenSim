using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LineRendererController : MonoBehaviour
{
    //public LineRenderer lineRenderer;
    public GameObject nodePrefab;
    public GameObject conePrefab;
    public GameObject linePrefab;
    public List<Connection> connections = new List<Connection>();

    public GameObject[] nodes;
    public GameObject[] cones;
    public GameObject[] lines;
    public int[] coneIndices;
    public const bool FIXED_INITIATION = true;

    public float DistanceMultiplier = 4;

    public const int MAX_CONES = 2;

    public const int MAX_POINTS = 8;

    public const int MAX_LINES = 18;

    void Start()
    {
        Instance = this;
        nodes = new GameObject[MAX_POINTS]; // Initialize the array with the correct size
        cones = new GameObject[MAX_CONES]; // Initialize the array with the correct size
        lines = new GameObject[MAX_LINES]; // Initialize the array with the correct size
        coneIndices = new int[MAX_CONES]; // Initialize the array with the correct size

        //lineRenderer.positionCount = nodes.Length;
        if (FIXED_INITIATION)
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
            for (int i = 0; i < MAX_LINES; i++)
            {
                GameObject line = Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                lines[i] = line;
            }
            GenerateRandomCones();
            GenerateConnections();



        }
        else{
            for (int i = 0; i < MAX_POINTS; i++)
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
        for (int i = 0; i < MAX_CONES; i++)
        {
            int randomIndex;
            do{
            randomIndex = UnityEngine.Random.Range(1, MAX_POINTS-1);
            } while (Array.Exists(coneIndices, element => element == randomIndex));
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
}
