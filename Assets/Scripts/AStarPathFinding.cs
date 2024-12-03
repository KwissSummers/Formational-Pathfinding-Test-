using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    // References
    public Transform character; // Leader character to move
    public Transform target;    // Target position
    public float speed = 5f;    // Movement speed

    // Grid properties (from WorldDecomposer)
    private int[,] worldData; // Grid representation of the world
    private int nodeSize = 2; // Node size for grid cells
    private int rows, cols;   // Dimensions of the grid
    private Vector3 gridOrigin = new Vector3(-50, 0, -50); // World origin offset for grid alignment

    private List<Vector3> currentPath = null; // Stores the current path for visualization

    private void Start()
    {
        // Initialize the grid based on WorldDecomposer logic
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        // Wait for the WorldDecomposer to be initialized
        WorldDecomposer worldDecomposer = FindObjectOfType<WorldDecomposer>();
        while (worldDecomposer == null || !worldDecomposer.isInitialized)
        {
            yield return null; // Wait until WorldDecomposer is ready
        }

        // Fetch the grid data and dimensions
        worldData = worldDecomposer.GetWorldData();
        rows = worldDecomposer.GetRows();
        cols = worldDecomposer.GetCols();
        Debug.Log("WorldDecomposer Initialized. Grid Size: " + rows + "x" + cols);
    }

    // Convert a world position to grid indices
    private Node PositionToNode(Vector3 position)
    {
        int row = Mathf.FloorToInt((position.z - gridOrigin.z) / nodeSize);
        int col = Mathf.FloorToInt((position.x - gridOrigin.x) / nodeSize);
        return new Node(row, col);
    }

    // Check if a node is walkable (not an obstacle)
    private bool IsWalkable(Node node)
    {
        return node.row >= 0 && node.row < rows &&
               node.col >= 0 && node.col < cols &&
               worldData[node.row, node.col] == 0;
    }

    // A* Pathfinding logic
    private List<Vector3> FindPath(Vector3 startPos, Vector3 goalPos)
    {
        // Convert world positions to grid indices
        Node startNode = PositionToNode(startPos);
        Node goalNode = PositionToNode(goalPos);

        // Validate start and goal nodes
        if (!IsWalkable(startNode) || !IsWalkable(goalNode))
        {
            Debug.LogError("Start or Goal node is not walkable!");
            return null;
        }

        List<Node> openList = new List<Node>(); // Nodes to evaluate
        HashSet<Node> closedList = new HashSet<Node>(); // Nodes already evaluated
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            // Find the node with the lowest f-score
            foreach (Node node in openList)
            {
                if (node.f < currentNode.f || (node.f == currentNode.f && node.h < currentNode.h))
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode); // Remove the current node from open list
            closedList.Add(currentNode); // Add it to the closed list

            // If the goal is reached, reconstruct the path
            if (currentNode.row == goalNode.row && currentNode.col == goalNode.col)
            {
                return ReconstructPath(currentNode);
            }

            // Get valid neighbors of the current node
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedList.Contains(neighbor) || !IsWalkable(neighbor))
                {
                    continue; // Skip invalid neighbors
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

    // Get valid neighbors for a given node
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        int[] rowOffsets = { -1, 0, 1, 0 };
        int[] colOffsets = { 0, 1, 0, -1 };

        for (int i = 0; i < 4; i++)
        {
            int newRow = node.row + rowOffsets[i];
            int newCol = node.col + colOffsets[i];

            if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols)
            {
                neighbors.Add(new Node(newRow, newCol));
            }
        }

        return neighbors;
    }

    // Calculate Manhattan distance heuristic
    private float GetHeuristic(Node a, Node b)
    {
        return Mathf.Abs(a.row - b.row) + Mathf.Abs(a.col - b.col);
    }

    // Reconstruct the path from the goal node
    private List<Vector3> ReconstructPath(Node node)
    {
        List<Vector3> path = new List<Vector3>();
        while (node != null)
        {
            path.Add(new Vector3(
                node.col * nodeSize + gridOrigin.x,
                0,
                node.row * nodeSize + gridOrigin.z
            ));
            node = node.parent;
        }
        path.Reverse();
        currentPath = path; // Store the path for visualization
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
        }

        Debug.Log("Path completed!");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click to set target
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 alignedTarget = new Vector3(
                    Mathf.Floor(hit.point.x / nodeSize) * nodeSize + nodeSize / 2f,
                    hit.point.y,
                    Mathf.Floor(hit.point.z / nodeSize) * nodeSize + nodeSize / 2f
                );
                target.position = alignedTarget;

                List<Vector3> path = FindPath(character.position, alignedTarget);
                if (path != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(MoveAlongPath(path));
                }
            }
        }
    }

    // Visualize the grid and path using Gizmos
    private void OnDrawGizmos()
    {
        if (worldData != null)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector3 cellCenter = new Vector3(
                        col * nodeSize + gridOrigin.x,
                        0,
                        row * nodeSize + gridOrigin.z
                    );

                    Gizmos.color = worldData[row, col] == 1 ? Color.red : Color.green;
                    Gizmos.DrawWireCube(cellCenter, Vector3.one * nodeSize);
                }
            }
        }

        // Draw the path
        if (currentPath != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
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
        }
    }
}
