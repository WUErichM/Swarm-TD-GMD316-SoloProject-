using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    private static string _path;
    private static string Path
    {
        get
        {
            if (string.IsNullOrEmpty(_path))
            {
                _path = Application.persistentDataPath + "/save.json";
            }
            return _path;
        }
    }

    public static void SaveGame()
    {
        GameManager gm = GameManager.instance;
        EnemySpawner spawner = Object.FindObjectOfType<EnemySpawner>();
        BuildManager bm = BuildManager.instance;

        if (gm == null || spawner == null || bm == null)
        {
            Debug.LogWarning("Save failed: Missing GameManager, EnemySpawner, or BuildManager in scene.");
            return;
        }

        SaveData data = new SaveData
        {
            hp = gm.playerHP,
            money = gm.money,
            score = gm.score, // ✅ SAVE SCORE

            lastTowerCost = bm.GetLastCost(),
            towersBuilt = bm.GetTowerCount(),
            gameTime = spawner.GetGameTime(),
            enemyHP = spawner.enemyHP,
            enemySpeed = spawner.enemySpeed
        };

        Tower[] towers = Object.FindObjectsOfType<Tower>();
        foreach (Tower t in towers)
        {
            data.towers.Add(new TowerData(t));
        }

        Enemy[] enemies = Object.FindObjectsOfType<Enemy>();
        foreach (Enemy e in enemies)
        {
            data.enemies.Add(new EnemyData(e));
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path, json);
        Debug.Log("Game Saved");
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(Path))
        {
            Debug.Log("No Save Found");
            return null;
        }

        string json = File.ReadAllText(Path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("Game Loaded");
        return data;
    }

    public static void DeleteSave()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
            Debug.Log("Save Deleted");
        }
    }

    public static bool SaveExists()
    {
        return File.Exists(Path);
    }
}