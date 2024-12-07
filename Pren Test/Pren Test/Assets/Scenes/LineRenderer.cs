using System;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject nodePrefab;

    public Transform[] points;
    public GameObject[] nodes;

    public const int MAX_POINTS = 7;

    void Start()
    {
        points = new Transform[MAX_POINTS]; // Initialize the array with the correct size
        nodes = new GameObject[MAX_POINTS]; // Initialize the array with the correct size
        lineRenderer.positionCount = points.Length;
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

    void Update()
    {
        // Clear previous positions
        lineRenderer.positionCount = 0;

        // List to store the positions for the LineRenderer
        List<Vector3> positions = new List<Vector3>();

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
                Console.WriteLine("Connecting Node " + i + " to Node " + closestIndex + " with distance " + distances[k].Item1);
            }
        }

        // Set the positions to the LineRenderer
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }
}
