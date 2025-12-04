using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FixedWaveConfig", menuName = "DDA/Fixed Wave Config")]
public class WaveConfigSO : ScriptableObject
{
	[System.Serializable]
	public struct EnemyGroup
	{
		public GameObject enemyPrefab;
		public int count; 
	}

	[System.Serializable]
	public struct WaveData
	{
		[Header("Настройки состава")]
		public List<EnemyGroup> enemyGroups; 
		public float spawnRate; 
	}

	public List<WaveData> waves = new List<WaveData>();
}