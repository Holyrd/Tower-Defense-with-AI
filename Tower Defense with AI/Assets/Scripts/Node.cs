using UnityEngine;

[System.Serializable]
public class Node
{
	public bool isWalkable;      // Можно ли здесь ходить
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;

	public int gCost;            // Стоимость от старта
	public int hCost;            // Стоимость до конца
	public int movementPenalty;  // ШТРАФ (Опасность от башен)

	public Node parent;          // Откуда мы пришли в эту клетку

	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
	{
		isWalkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
		movementPenalty = 0;
	}

	public int fCost
	{
		get { return gCost + hCost; }
	}
}