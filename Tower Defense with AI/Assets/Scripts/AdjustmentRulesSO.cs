using UnityEngine;

[CreateAssetMenu(fileName = "AdjustmentRules", menuName = "DDA/Adjustment Rules")]
public class AdjustmentRulesSO : ScriptableObject
{
	[Header("Success thresholds (1.0 = normal)")]
	public float highPerformanceThreshold = 1.2f; 
	public float lowPerformanceThreshold = 0.8f;  

	[Header("Reaction to Success (Complication)")]
	public float enemyCountMultiplier_Harder = 1.1f; 
	public float spawnRateMultiplier_Harder = 1.1f;  
	public float goldRewardMultiplier_Bonus = 1.2f;  

	[Header("Reaction to Failure (Relief)")]
	public float enemyCountMultiplier_Easier = 0.9f; 
	public float spawnRateMultiplier_Easier = 0.9f;  

	[Header("Difficulty limits")]
	[Tooltip("Minimum possible multiplier (eg 0.8 = difficulty will not drop below 80%)")]
	public float minDifficultyLimit = 0.8f;

	[Tooltip("Maximum possible multiplier (e.g. 1.2 = difficulty will not increase above 120%)")]
	public float maxDifficultyLimit = 1.2f;
}