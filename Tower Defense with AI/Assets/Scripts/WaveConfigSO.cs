using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FixedWaveConfig", menuName = "DDA/Fixed Wave Config")]
public class WaveConfigSO : ScriptableObject
{
	[System.Serializable]
	public struct EnemyGroup
	{
		public GameObject enemyPrefab;
		public int count; // Сколько штук этого типа
	}

	[System.Serializable]
	public struct WaveData
	{
		[Header("Настройки состава")]
		public List<EnemyGroup> enemyGroups; // Список групп (Сначала пойдут эти, потом следующие)
		public float spawnRate; // Врагов в секунду
	}

	public List<WaveData> waves = new List<WaveData>();
}