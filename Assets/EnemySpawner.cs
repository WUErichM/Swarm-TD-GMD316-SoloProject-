using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform[] waypoints;

    private float timer = 0f;
    private float gameTime = 0f;

    public float baseSpawnRate = 2f;

    public float baseEnemyHP = 5f;
    public float baseEnemySpeed = 2f;
    public int baseEnemyReward = 10;

    public float enemyHP;
    public float enemySpeed;

    void Update()
    {
        gameTime += Time.deltaTime;
        timer += Time.deltaTime;

        float difficultyFactor = 1f + gameTime / 30f;
        float spawnRate = Mathf.Max(0.2f, baseSpawnRate / difficultyFactor);

        if (timer >= spawnRate)
        {
            SpawnEnemy(difficultyFactor);
            timer = 0f;
        }
    }

    void SpawnEnemy(float difficultyFactor)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Enemy e = enemy.GetComponent<Enemy>();

        if (e != null)
        {
            e.Setup(waypoints);

            enemyHP = baseEnemyHP * difficultyFactor;
            enemySpeed = baseEnemySpeed * difficultyFactor;

            e.health = Mathf.FloorToInt(enemyHP);
            e.speed = enemySpeed;
            e.reward = Mathf.FloorToInt(baseEnemyReward * difficultyFactor);
        }
    }

    public float GetGameTime()
    {
        return gameTime;
    }

    public void SetGameTime(float t)
    {
        gameTime = t;
    }
}