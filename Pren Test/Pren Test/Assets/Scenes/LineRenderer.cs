using System;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject nodePrefab;
    public GameObject conePrefab;

    public Transform[] points;
    public GameObject[] nodes;
    public GameObject[] cones;
    public int[] coneIndices;
    public const bool FIXED_INITIATION = true;

    public float DistanceMultiplier = 4;

    public const int MAX_CONES = 2;

    public const int MAX_POINTS = 8;

    void Start()
    {
        points = new Transform[MAX_POINTS]; // Initialize the array with the correct size
        nodes = new GameObject[MAX_POINTS]; // Initialize the array with the correct size
        cones = new GameObject[MAX_CONES]; // Initialize the array with the correct size
        coneIndices = new int[MAX_CONES]; // Initialize the array with the correct size

        lineRenderer.positionCount = points.Length;
        if (FIXED_INITIATION)
        {
            // Hexagon
            nodes[0] = Instantiate(nodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            nodes[1] = Instantiate(nodePrefab, new Vector3(1*DistanceMultiplier, 0, 1*DistanceMultiplier), Quaternion.identity);
            nodes[2] = Instantiate(nodePrefab, new Vector3(1*DistanceMultiplier, 0, -1*DistanceMultiplier), Quaternion.identity);
            nodes[3] = Instantiate(nodePrefab, new Vector3(2*DistanceMultiplier, 0, 1*DistanceMultiplier), Quaternion.identity);
            nodes[4] = Instantiate(nodePrefab, new Vector3(2*DistanceMultiplier, 0, -1*DistanceMultiplier), Quaternion.identity);
            nodes[5] = Instantiate(nodePrefab, new Vector3(3*DistanceMultiplier, 0, 0), Quaternion.identity);
            // extra nodes
            nodes[6] = Instantiate(nodePrefab, new Vector3(1*DistanceMultiplier, 0, -0.5f*DistanceMultiplier), Quaternion.identity); // Center node
            nodes[7] = Instantiate(nodePrefab, new Vector3(2*DistanceMultiplier, 0, 0), Quaternion.identity); // Center node

            GenerateRandomCones();

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
                
                points[i] = nodes[i].transform; // Initialize points[i] with the transform of the instantiated node
                Console.WriteLine("Node " + i + " created at " + points[i].position);
            }
        }


    }

    void Update()
    {
        // Clear previous positions
        lineRenderer.positionCount = 0;

        // List to store the positions for the LineRenderer
        List<Vector3> positions = new List<Vector3>();
        foreach (GameObject node in nodes)
            {
                points[Array.IndexOf(nodes, node)] = node.transform; // Initialize points[i] with the transform of the instantiated node
            }
        // Iterate through each point
        for (int i = 0; i < points.Length; i++)
        {
            // List to store distances to other points
            List<Tuple<float, int>> distances = new List<Tuple<float, int>>();

            // Find the distances to all other points
            for (int j = 0; j < points.Length; j++)
            {
                if (i != j)
                {
                    float distance = Vector3.Distance(points[i].position, points[j].position);
                    distances.Add(new Tuple<float, int>(distance, j));
                }
            }

            // Sort the distances
            distances.Sort((a, b) => a.Item1.CompareTo(b.Item1));

            // Connect to the two closest points
            for (int k = 0; k < 2 && k < distances.Count; k++)
            {
                int closestIndex = distances[k].Item2;
                positions.Add(points[i].position);
                positions.Add(points[closestIndex].position);
                Debug.Log("Connecting Node " + i + " to Node " + closestIndex + " with distance " + distances[k].Item1);
            }
        }

        // Set the positions to the LineRenderer
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
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
}
