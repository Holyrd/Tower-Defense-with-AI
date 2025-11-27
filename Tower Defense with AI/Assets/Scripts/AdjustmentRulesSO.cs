using UnityEngine;

[CreateAssetMenu(fileName = "AdjustmentRules", menuName = "DDA/Adjustment Rules")]
public class AdjustmentRulesSO : ScriptableObject
{
	[Header("Пороги успеха (1.0 = норма)")]
	public float highPerformanceThreshold = 1.2f; // Игрок на 20% эффективнее нормы
	public float lowPerformanceThreshold = 0.8f;  // Игрок на 20% слабее нормы

	[Header("Реакция на Успех (Усложнение)")]
	public float enemyCountMultiplier_Harder = 1.1f; // +10% врагов
	public float spawnRateMultiplier_Harder = 1.1f;  // +10% скорости спавна
	public float goldRewardMultiplier_Bonus = 1.2f;  // +20% денег (Поощрение!)

	[Header("Реакция на Провал (Облегчение)")]
	public float enemyCountMultiplier_Easier = 0.9f; // -10% врагов
	public float spawnRateMultiplier_Easier = 0.9f;  // -10% скорости
}