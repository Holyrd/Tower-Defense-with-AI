using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyProfile", menuName = "DDA/Difficulty Profile")]
public class DifficultyProfileSO : ScriptableObject
{
	public string difficultyName = "Normal";
	public float baseHealthMultiplier = 1.0f;
	public float baseSpeedMultiplier = 1.0f;
	public float scoreRequirementMultiplier = 1.0f; // На сложном уровне требования выше
}