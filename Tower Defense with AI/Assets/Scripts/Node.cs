using UnityEngine;

[System.Serializable]
public class Node
{
	public bool isWalkable;      
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;

	public int gCost;            
	public int hCost;           
	public int movementPenalty;  

	public Node parent;          

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