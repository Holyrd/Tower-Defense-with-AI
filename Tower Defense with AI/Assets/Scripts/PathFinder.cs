using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{
	public static Pathfinder main;

	[Header("Visuals")]
	public LineRenderer lineRenderer; 
	public Transform startPoint;      
	public Transform endPoint;        

	[Header("Grid Settings")]
	public LayerMask unwalkableMask;  
	public Vector2 gridWorldSize;     
	public float nodeRadius = 0.5f;   

	Node[,] grid;
	float nodeDiameter;
	int gridSizeX, gridSizeY;

	private Dictionary<Vector2Int, Plot> plotMap = new Dictionary<Vector2Int, Plot>();

	[Header("Wave Logic")]
	public bool useDangerLogic = false; 
	public int towerDangerPenalty = 50; 

	public List<Node> currentWavePath = new List<Node>();

	private void Awake()
	{
		main = this;
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		ScanGrid();   

		SnapPlotsToGrid();
		CachePlots(); 
	}

	private void Start()
	{
		RecalculatePath();
	}

	void SnapPlotsToGrid()
	{
		Plot[] allPlots = FindObjectsByType<Plot>(FindObjectsSortMode.None);
		foreach (Plot p in allPlots)
		{
			Node n = NodeFromWorldPoint(p.transform.position);

			p.transform.position = new Vector3(n.worldPosition.x, n.worldPosition.y, p.transform.position.z);
		}
	}

	void CachePlots()
	{
		plotMap.Clear();
		Plot[] allPlots = FindObjectsByType<Plot>(FindObjectsSortMode.None);
		foreach (Plot p in allPlots)
		{
			Node n = NodeFromWorldPoint(p.transform.position);
			Vector2Int gridPos = new Vector2Int(n.gridX, n.gridY);

			if (!plotMap.ContainsKey(gridPos))
			{
				plotMap.Add(gridPos, p);
			}
			else
			{
				Debug.LogWarning("Two rafts on one cell! Check the arrangement: " + gridPos);
			}
		}
	}

	public void ScanGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);

				bool walkable = !(Physics2D.OverlapCircle(worldPoint, nodeRadius - 0.1f, unwalkableMask));

				grid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}

		if (useDangerLogic) UpdateDangerMap();
	}

	public void UpdateDangerMap()
	{
		if (grid == null) return;

		foreach (Node n in grid) n.movementPenalty = 0;

		if (!useDangerLogic) return;

		Turret[] towers = FindObjectsByType<Turret>(FindObjectsSortMode.None);
		foreach (Turret tower in towers)
		{
			Node towerNode = NodeFromWorldPoint(tower.transform.position);
			List<Node> neighbors = GetNeighbors(towerNode);
			foreach (Node neighbor in neighbors)
			{
				if (neighbor.isWalkable)
				{
					neighbor.movementPenalty += towerDangerPenalty;
				}
			}
		}
	}


	public void RecalculatePath()
	{
		if (startPoint == null || endPoint == null) return;

		currentWavePath = FindPath(startPoint.position, endPoint.position);

		if (lineRenderer != null) lineRenderer.positionCount = 0;
		DrawPathLine(currentWavePath);


		UpdatePlotStatus(currentWavePath);
	}

	void DrawPathLine(List<Node> path)
	{
		if (path == null || path.Count == 0) return;

		lineRenderer.positionCount = path.Count + 2;

		lineRenderer.SetPosition(0, startPoint.position + Vector3.back);

		for (int i = 0; i < path.Count; i++)
		{
			lineRenderer.SetPosition(i + 1, path[i].worldPosition + Vector3.back);
		}

		lineRenderer.SetPosition(path.Count + 1, endPoint.position + Vector3.back);
	}

	void UpdatePlotStatus(List<Node> path)
	{
		foreach (var kvp in plotMap)
		{
			kvp.Value.BecomeBuildable();
		}

		if (path == null) return;

		foreach (Node n in path)
		{
			Vector2Int gridPos = new Vector2Int(n.gridX, n.gridY);
			if (plotMap.ContainsKey(gridPos))
			{
				plotMap[gridPos].BecomePath();
			}
		}
	}

	public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
	{
		Node startNode = NodeFromWorldPoint(startPos);
		Node targetNode = NodeFromWorldPoint(targetPos);

		List<Node> openSet = new List<Node>();
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (openSet.Count > 0)
		{
			Node currentNode = openSet[0];
			for (int i = 1; i < openSet.Count; i++)
			{
				if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
				{
					currentNode = openSet[i];
				}
			}

			openSet.Remove(currentNode);
			closedSet.Add(currentNode);

			if (currentNode == targetNode)
			{
				return RetracePath(startNode, targetNode);
			}

			foreach (Node neighbor in GetNeighbors(currentNode))
			{
				if (!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;

				int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;

				if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
				{
					neighbor.gCost = newMovementCostToNeighbor;
					neighbor.hCost = GetDistance(neighbor, targetNode);
					neighbor.parent = currentNode;

					if (!openSet.Contains(neighbor))
						openSet.Add(neighbor);
				}
			}
		}
		return null;
	}

	List<Node> RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();
		return path;
	}

	int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		return 10 * (dstX + dstY);
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}

	public List<Node> GetNeighbors(Node node)
	{
		List<Node> neighbors = new List<Node>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0) continue;

				if (Mathf.Abs(x) + Mathf.Abs(y) != 1) continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbors.Add(grid[checkX, checkY]);
				}
			}
		}
		return neighbors;
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

		if (grid != null)
		{
			foreach (Node n in grid)
			{
				Gizmos.color = (n.isWalkable) ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f);
				if (n.movementPenalty > 0) Gizmos.color = new Color(1, 0.92f, 0.016f, 0.5f);
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
			}
		}
	}
}