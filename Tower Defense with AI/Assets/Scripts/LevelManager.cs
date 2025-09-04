using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public static LevelManager main;

	public void Awake()
	{
		main = this;
	}


	public Transform startPoint;
	public Transform[] path;

}
