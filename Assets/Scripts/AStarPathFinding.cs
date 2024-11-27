using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    // References
    public Transform character; // Leader
    public Transform target;    // Target position
    public float speed = 5f;    // Movement speed

    // Grid properties (from WorldDecomposer)
    private int[,] worldData;
    private int nodeSize = 2; // Should match WorldDecomposer
    private int rows, cols;

    private void Start()
    {
        // Initialize the grid based on WorldDecomposer logic
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        WorldDecomposer worldDecomposer = FindObjectOfType<WorldDecomposer>();
        while (worldDecomposer == null || !worldDecomposer.isInitialized)
        {
            yield return null; // Wait until WorldDecomposer is ready
        }

        worldData = worldDecomposer.GetWorldData();
        rows = worldDecomposer.GetRows();
        cols = worldDecomposer.GetCols();
        Debug.Log("WorldDecomposer Initialized. Grid Size: " + rows + "x" + cols);
    }

    // A* Pathfinding logic
    private List<Vector3> FindPath(Vector3 startPos, Vector3 goalPos)
    {
        // Convert positions to grid indices
        Node startNode = new Node((int)(startPos.x / nodeSize), (int)(startPos.z / nodeSize));
        Node goalNode = new Node((int)(goalPos.x / nodeSize), (int)(goalPos.z / nodeSize));

        Debug.Log("Start Node: " + startNode.row + ", " + startNode.col);
        Debug.Log("Goal Node: " + goalNode.row + ", " + goalNode.col);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            foreach (Node node in openList)
            {
                if (node.f < currentNode.f || (node.f == currentNode.f && node.h < currentNode.h))
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.row == goalNode.row && currentNode.col == goalNode.col)
            {
                Debug.Log("Goal reached! Reconstructing path.");
                return ReconstructPath(currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedList.Contains(neighbor) || worldData[neighbor.row, neighbor.col] == 1)
                {
                    continue;
                }

                float tentativeG = currentNode.g + 1; // Distance between nodes is 1
                if (!openList.Contains(neighbor) || tentativeG < neighbor.g)
                {
                    neighbor.g = tentativeG;
                    neighbor.h = GetHeuristic(neighbor, goalNode);
                    neighbor.f = neighbor.g + neighbor.h;
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogError("No path found!");
        return null; // No path found
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        int[] rowOffsets = { -1, 0, 1, 0 };
        int[] colOffsets = { 0, 1, 0, -1 };

        for (int i = 0; i < 4; i++)
        {
            int newRow = node.row + rowOffsets[i];
            int newCol = node.col + colOffsets[i];

            // Ensure the neighbor is within bounds and walkable
            if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols)
            {
                if (worldData[newRow, newCol] == 0)  // Make sure it's not an obstacle
                {
                    neighbors.Add(new Node(newRow, newCol));
                }
            }
        }

        // Debug the neighbors to check which are added
        Debug.Log("Neighbors of (" + node.row + ", " + node.col + "): ");
        foreach (var neighbor in neighbors)
        {
            Debug.Log("  Neighbor: (" + neighbor.row + ", " + neighbor.col + ")");
        }

        return neighbors;
    }

    private float GetHeuristic(Node a, Node b)
    {
        return Mathf.Abs(a.row - b.row) + Mathf.Abs(a.col - b.col); // Manhattan distance
    }

    private List<Vector3> ReconstructPath(Node node)
    {
        List<Vector3> path = new List<Vector3>();
        while (node != null)
        {
            path.Add(new Vector3(node.row * nodeSize, 0, node.col * nodeSize));
            node = node.parent;
        }
        path.Reverse();

        // Debug the reconstructed path
        foreach (var point in path)
        {
            Debug.Log("Reconstructed path point: " + point);
        }

        return path;
    }

    // Move the character along the path
    private IEnumerator MoveAlongPath(List<Vector3> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogError("No path found or path is empty!");
            yield break;
        }

        foreach (Vector3 point in path)
        {
            while (Vector3.Distance(character.position, point) > 0.1f)
            {
                Vector3 direction = (point - character.position).normalized;
                character.position += direction * speed * Time.deltaTime;

                // Smooth rotation
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                character.rotation = Quaternion.Slerp(character.rotation, targetRotation, Time.deltaTime * 5f);

                yield return null;
            }

            Debug.Log("Arrived at path point: " + point);
        }

        Debug.Log("Path completed!");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click to move
        {
            // Get mouse position and translate to world position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                target.position = hit.point;
                print("Mouse click position: " + hit.point);

                // Find and follow the path using A* algorithm
                Vector3 startPos = character.position;
                Vector3 goalPos = hit.point;

                List<Vector3> path = FindPath(startPos, goalPos);

                if (path != null && path.Count > 0)
                {
                    StopAllCoroutines();
                    StartCoroutine(MoveAlongPath(path)); // Start moving along the calculated path
                    Debug.Log("Starting pathfinding...");
                }
                else
                {
                    Debug.LogError("No path found!"); //error2
                }
            }
        }
    }

    // Node class for A* algorithm
    private class Node
    {
        public int row, col;
        public float g, h, f;
        public Node parent;

        public Node(int row, int col)
        {
            this.row = row;
            this.col = col;
            this.g = 0;
            this.h = 0;
            this.f = 0;
            this.parent = null;
        }
    }
}
