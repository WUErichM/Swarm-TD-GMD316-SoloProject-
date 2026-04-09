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

        // ✅ Slightly stronger mid-game scaling (controlled)
        float difficultyFactor = 1f + (gameTime / 28f) + Mathf.Pow(gameTime / 80f, 1.15f);

        // Keep spawn mostly similar (minor mid-game pressure)
        float spawnRate = Mathf.Max(0.18f, baseSpawnRate / (1f + gameTime / 45f));

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

            // ✅ Slightly stronger enemies in mid-game
            enemyHP = baseEnemyHP * difficultyFactor;
            enemySpeed = baseEnemySpeed * (1f + difficultyFactor * 0.2f);

            e.health = Mathf.FloorToInt(enemyHP);
            e.speed = enemySpeed;

            // ✅ LOWER EARLY GAME MONEY
            // Starts at ~60% instead of 80%
            // Slowly drops to ~45% late game
            float rewardScale = Mathf.Clamp(0.6f - (gameTime / 250f), 0.45f, 0.6f);

            e.reward = Mathf.FloorToInt(baseEnemyReward * difficultyFactor * rewardScale);
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