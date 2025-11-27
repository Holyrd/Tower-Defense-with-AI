[System.Serializable]
public class SessionStats
{
	public int enemiesKilled;
	public int totalDamageDealt;
	public int moneyEarned;
	public int moneySpent;
	public int towersBuilt;
	public int wavesSurvived;

	public void Reset()
	{
		enemiesKilled = 0;
		totalDamageDealt = 0;
		moneyEarned = 0;
		moneySpent = 0;
		towersBuilt = 0;
		wavesSurvived = 0;
	}
}
