using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;
    [SerializeField] private int currencyForEnemy = 25;

    private bool isDestroyd = false;

    public void TakeDamage(int damage)
    {
        hitPoint -= damage;

        if(hitPoint <= 0 && !isDestroyd)
        {
            EnemySpawner.OnEnemyDestroy.Invoke();
            LevelManager.main.IncreasCurrency(currencyForEnemy);
            isDestroyd = true;
            Destroy(gameObject);
        }
    }

    public int GiveDamage()
    {
        return hitPoint;
    }
}
