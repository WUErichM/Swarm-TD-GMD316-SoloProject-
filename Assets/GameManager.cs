using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static SaveData loadedSaveData;

    public int playerHP = 10;
    public int money = 100;

    public int score = 0;

    public int lastTowerCost = 25;
    public int towersBuilt = 0;

    public float gameTime = 0f;

    public TextMeshProUGUI hpText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    public GameObject gameOverUI;

    // ✅ NEW: track game over state
    public bool isGameOver = false;

    void Awake()
    {
        instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (loadedSaveData != null)
        {
            ApplyLoadedData();
        }
        else if (SaveSystem.SaveExists())
        {
            SaveData data = SaveSystem.LoadGame();
            if (data != null)
            {
                loadedSaveData = data;
                ApplyLoadedData();
            }
        }

        UpdateUI();
    }

    void Update()
    {
        // ❌ STOP time tracking if game over
        if (isGameOver) return;

        gameTime += Time.deltaTime;
        UpdateUI();
    }

    public void LoseLife(int amount)
    {
        if (isGameOver) return;

        playerHP -= amount;
        UpdateUI();

        if (playerHP <= 0)
        {
            GameOver();
        }
    }

    public void AddMoney(int amount)
    {
        if (isGameOver) return;

        money += amount;
        score += amount;
        UpdateUI();
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void UpdateUI()
    {
        if (hpText != null) hpText.text = "HP: " + playerHP;
        if (moneyText != null) moneyText.text = "Money: " + money;
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (timeText != null) timeText.text = "Time: " + FormatTime(gameTime);
    }

    string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    void GameOver()
    {
        isGameOver = true; // ✅ LOCK GAME STATE
        Time.timeScale = 0f;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ✅ NEW: go to main menu
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void ApplyLoadedData()
    {
        playerHP = loadedSaveData.hp;
        money = loadedSaveData.money;
        score = loadedSaveData.score;

        lastTowerCost = Mathf.FloorToInt(loadedSaveData.lastTowerCost);
        towersBuilt = loadedSaveData.towersBuilt;
        gameTime = loadedSaveData.gameTime;

        UpdateUI();

        BuildManager.instance.LoadData(
            Mathf.FloorToInt(loadedSaveData.lastTowerCost),
            loadedSaveData.towersBuilt
        );

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.SetGameTime(loadedSaveData.gameTime);
            spawner.enemyHP = loadedSaveData.enemyHP;
            spawner.enemySpeed = loadedSaveData.enemySpeed;
        }

        foreach (Tower t in FindObjectsOfType<Tower>()) Destroy(t.gameObject);
        foreach (Enemy e in FindObjectsOfType<Enemy>()) Destroy(e.gameObject);

        foreach (TowerData td in loadedSaveData.towers)
        {
            Vector3 pos = new Vector3(td.x, td.y, td.z);

            GameObject prefabToSpawn = BuildManager.instance.standardTowerPrefab;

            if (td.towerType == (int)Tower.TowerType.Melee)
                prefabToSpawn = BuildManager.instance.meleeTowerPrefab;
            else if (td.towerType == (int)Tower.TowerType.Sniper)
                prefabToSpawn = BuildManager.instance.sniperTowerPrefab;

            GameObject tower = Instantiate(prefabToSpawn, pos, Quaternion.identity);
            Tower towerComp = tower.GetComponent<Tower>();
            towerComp.LoadFromData(td);
        }

        foreach (EnemyData ed in loadedSaveData.enemies)
        {
            GameObject enemyObj = Instantiate(
                spawner.enemyPrefab,
                new Vector3(ed.x, ed.y, ed.z),
                Quaternion.identity
            );

            Enemy e = enemyObj.GetComponent<Enemy>();
            e.health = Mathf.FloorToInt(ed.health);
            e.speed = ed.speed;
            e.reward = ed.reward;
            e.Setup(spawner.waypoints);
        }

        loadedSaveData = null;
    }
}