using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{
	public static Pathfinder main;

	[Header("Visuals")]
	public LineRenderer lineRenderer; // Перетащи сюда компонент LineRenderer
	public Transform startPoint;      // Точка спавна врагов
	public Transform endPoint;        // База

	[Header("Grid Settings")]
	public LayerMask unwalkableMask;  // Слой препятствий (камни, стены)
	public Vector2 gridWorldSize;     // Размер поля
	public float nodeRadius = 0.5f;   // Радиус клетки

	Node[,] grid;
	float nodeDiameter;
	int gridSizeX, gridSizeY;

	// Словарь для быстрой связи координат сетки и объектов Plot
	private Dictionary<Vector2Int, Plot> plotMap = new Dictionary<Vector2Int, Plot>();

	[Header("Wave Logic")]
	public bool useDangerLogic = false; // Включать на 5+ волне
	public int towerDangerPenalty = 50; // Штраф за проход рядом с башней

	public List<Node> currentWavePath = new List<Node>();

	private void Awake()
	{
		main = this;
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		ScanGrid();   // Создаем сетку

		SnapPlotsToGrid();
		CachePlots(); // Запоминаем все места для стройки
	}

	private void Start()
	{
		// При старте сразу строим и рисуем путь
		RecalculatePath();
	}

	// --- 1. РАБОТА С СЕТКОЙ И PLOTS ---

	void SnapPlotsToGrid()
	{
		Plot[] allPlots = FindObjectsByType<Plot>(FindObjectsSortMode.None);
		foreach (Plot p in allPlots)
		{
			// Узнаем, какой клетке принадлежит этот Плот
			Node n = NodeFromWorldPoint(p.transform.position);

			// Насильно ставим его в центр этой клетки
			// (Сохраняем Z координату, меняем только X и Y)
			p.transform.position = new Vector3(n.worldPosition.x, n.worldPosition.y, p.transform.position.z);
		}
	}

	// Находим все Plots на сцене и запоминаем их координаты
	void CachePlots()
	{
		plotMap.Clear();
		Plot[] allPlots = FindObjectsByType<Plot>(FindObjectsSortMode.None);
		foreach (Plot p in allPlots)
		{
			// Получаем узел, который НАИБОЛЕЕ БЛИЗОК к позиции плота
			Node n = NodeFromWorldPoint(p.transform.position);
			Vector2Int gridPos = new Vector2Int(n.gridX, n.gridY);

			if (!plotMap.ContainsKey(gridPos))
			{
				plotMap.Add(gridPos, p);
			}
			else
			{
				Debug.LogWarning("Два плота на одной клетке! Проверь расстановку: " + gridPos);
			}
		}
	}

	// Сканируем физический мир и обновляем проходимость
	public void ScanGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);

				// Проверяем, есть ли препятствие
				bool walkable = !(Physics2D.OverlapCircle(worldPoint, nodeRadius - 0.1f, unwalkableMask));

				grid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}

		// Если включена логика опасности, добавляем штрафы
		if (useDangerLogic) UpdateDangerMap();
	}

	public void UpdateDangerMap()
	{
		if (grid == null) return;

		// Сброс старых штрафов
		foreach (Node n in grid) n.movementPenalty = 0;

		if (!useDangerLogic) return;

		// Ищем все турели и делаем клетки вокруг них дорогими
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

	// --- 2. УПРАВЛЕНИЕ ПУТЕМ (ГЛАВНЫЙ МЕТОД) ---

	// Вызывать этот метод при: Старте, Постройке башни, Смене волны
	public void RecalculatePath()
	{
		if (startPoint == null || endPoint == null) return;

		// 1. Считаем путь и СОХРАНЯЕМ его в публичную переменную
		currentWavePath = FindPath(startPoint.position, endPoint.position);

		// 2. Рисуем линию по этому пути
		if (lineRenderer != null) lineRenderer.positionCount = 0;
		DrawPathLine(currentWavePath);

		// 3. Блокируем плоты
		UpdatePlotStatus(currentWavePath);

		// Событие OnPathUpdated здесь НЕ НУЖНО вызывать для врагов, 
		// так как они не должны менять путь на лету.
	}

	void DrawPathLine(List<Node> path)
	{
		if (path == null || path.Count == 0) return;

		// Точек линии = Путь + Старт + Финиш
		lineRenderer.positionCount = path.Count + 2;

		// 0. Начало (физическая точка спавна)
		lineRenderer.SetPosition(0, startPoint.position + Vector3.back);

		// 1...N. Точки пути (центры клеток)
		for (int i = 0; i < path.Count; i++)
		{
			lineRenderer.SetPosition(i + 1, path[i].worldPosition + Vector3.back);
		}

		// Last. Конец (физическая точка базы)
		lineRenderer.SetPosition(path.Count + 1, endPoint.position + Vector3.back);
	}

	void UpdatePlotStatus(List<Node> path)
	{
		// 1. Сброс: Разрешаем строить везде, ГДЕ НЕТ БАШЕН
		foreach (var kvp in plotMap)
		{
			// Метод BecomeBuildable теперь сам проверяет HasTower() внутри
			kvp.Value.BecomeBuildable();
		}

		if (path == null) return;

		// 2. Блокируем путь
		foreach (Node n in path)
		{
			Vector2Int gridPos = new Vector2Int(n.gridX, n.gridY);
			if (plotMap.ContainsKey(gridPos))
			{
				plotMap[gridPos].BecomePath();
			}
		}
	}

	// --- 3. АЛГОРИТМ A* (A-Star) ---

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

				// Стоимость = G + Расстояние + Штраф(Опасность)
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
		return null; // Путь не найден
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

	// --- 4. ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

	int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		// Манхэттенская дистанция (сумма катетов)
		// Просто складываем разницу по X и Y и умножаем на 10
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

		// Проходим по соседям: Слева, Справа, Сверху, Снизу
		// x = -1 (слева), x = 1 (справа), y = -1 (снизу), y = 1 (сверху)
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				// Пропускаем самого себя
				if (x == 0 && y == 0) continue;

				// --- ГЛАВНОЕ ИСПРАВЛЕНИЕ ---
				// Если мы меняем И x, И y (например 1, 1) — это диагональ.
				// Сумма модулей для прямых соседей всегда равна 1 (1+0 или 0+1).
				// Если сумма равна 2 (1+1) — это диагональ, мы её пропускаем.
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

	// --- 5. ВИЗУАЛИЗАЦИЯ В РЕДАКТОРЕ ---
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