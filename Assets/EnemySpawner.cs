using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner instance;

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

    private float globalHPModifier = 1f;
    private float globalSpeedModifier = 1f;
    private float globalRewardModifier = 1f;

    private float hpCheckTimer = 0f;
    private float timeBuffTimer = 0f;
    private float survivalTimer = 0f;

    private int lastPlayerHP;
    private int rewardBonusStacks = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (GameManager.instance != null)
            lastPlayerHP = GameManager.instance.playerHP;
    }

    void Update()
    {
        gameTime += Time.deltaTime;
        timer += Time.deltaTime;

        hpCheckTimer += Time.deltaTime;
        timeBuffTimer += Time.deltaTime;
        survivalTimer += Time.deltaTime;

        float difficultyFactor = 1f + (gameTime / 28f) + Mathf.Pow(gameTime / 80f, 1.15f);
        float spawnRate = Mathf.Max(0.18f, baseSpawnRate / (1f + gameTime / 45f));

        if (timer >= spawnRate)
        {
            SpawnEnemy(difficultyFactor);
            timer = 0f;
        }

        if (hpCheckTimer >= 120f)
        {
            ApplyHPScaling();
            hpCheckTimer = 0f;
        }

        if (timeBuffTimer >= 180f)
        {
            ApplyTimeBuff();
            timeBuffTimer = 0f;
        }

        if (survivalTimer >= 420f)
        {
            ApplySurvivalScaling();
            survivalTimer = 0f;
        }

        TrackPlayerHP();
    }

    void SpawnEnemy(float difficultyFactor)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Enemy e = enemy.GetComponent<Enemy>();

        if (e != null)
        {
            e.Setup(waypoints);

            enemyHP = baseEnemyHP * difficultyFactor * globalHPModifier;
            enemySpeed = baseEnemySpeed * (1f + difficultyFactor * 0.2f) * globalSpeedModifier;

            e.health = Mathf.FloorToInt(enemyHP);
            e.speed = enemySpeed;

            float rewardScale = Mathf.Clamp(0.6f - (gameTime / 250f), 0.45f, 0.6f);

            e.reward = Mathf.FloorToInt(
                baseEnemyReward * difficultyFactor * rewardScale * globalRewardModifier
            );
        }
    }

    public void ApplyGlobalNerf(float hpMult, float speedMult)
    {
        globalHPModifier *= hpMult;
        globalSpeedModifier *= speedMult;
    }

    void ApplyHPScaling()
    {
        if (GameManager.instance == null) return;

        int hp = GameManager.instance.playerHP;
        float buffPercent = hp * 0.02f;

        globalHPModifier *= (1f + buffPercent);
    }

    // ✅ UPDATED HERE (30% reward instead of 25%)
    void ApplyTimeBuff()
    {
        globalHPModifier *= 1.30f;
        globalSpeedModifier *= 1.15f;
        globalRewardModifier *= 1.30f; // was 1.25
    }

    void ApplySurvivalScaling()
    {
        if (GameManager.instance == null) return;

        int currentHP = GameManager.instance.playerHP;

        if (currentHP >= lastPlayerHP)
        {
            globalSpeedModifier *= 1.10f;

            rewardBonusStacks++;
            float rewardBoost = 0.4f + (0.2f * (rewardBonusStacks - 1));
            globalRewardModifier *= (1f + rewardBoost);
        }
    }

    void TrackPlayerHP()
    {
        if (GameManager.instance == null) return;

        int currentHP = GameManager.instance.playerHP;

        if (currentHP < lastPlayerHP)
        {
            lastPlayerHP = currentHP;
            rewardBonusStacks = 0;
        }
    }

    // ✅ IMPORTANT FOR UPGRADE SCALING
    public float GetGameTime()
    {
        return gameTime;
    }

    public float GetHPModifier() => globalHPModifier;
    public float GetSpeedModifier() => globalSpeedModifier;
    public float GetRewardModifier() => globalRewardModifier;

    public void SetGameTime(float t)
    {
        gameTime = t;
    }
}