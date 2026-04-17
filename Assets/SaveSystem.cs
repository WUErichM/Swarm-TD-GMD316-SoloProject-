using UnityEngine;
using System.IO;
using System.Collections.Generic;

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
            Debug.LogWarning("Save failed: Missing components.");
            return;
        }

        SaveData data;

        if (File.Exists(Path))
        {
            string existingJson = File.ReadAllText(Path);
            data = JsonUtility.FromJson<SaveData>(existingJson);
        }
        else
        {
            data = new SaveData();
        }

        data.hp = gm.playerHP;
        data.money = gm.money;
        data.score = gm.score;

        data.lastTowerCost = bm.GetLastCost();
        data.towersBuilt = bm.GetTowerCount();
        data.gameTime = spawner.GetGameTime();
        data.enemyHP = spawner.enemyHP;
        data.enemySpeed = spawner.enemySpeed;

        data.towers = new List<TowerData>();
        data.enemies = new List<EnemyData>();

        foreach (Tower t in Object.FindObjectsOfType<Tower>())
        {
            data.towers.Add(new TowerData(t));
        }

        foreach (Enemy e in Object.FindObjectsOfType<Enemy>())
        {
            data.enemies.Add(new EnemyData(e));
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path, json);

        Debug.Log("Game Saved WITHOUT deleting leaderboard");
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(Path))
        {
            return null;
        }

        string json = File.ReadAllText(Path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteSave()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }

    public static bool SaveExists()
    {
        return File.Exists(Path);
    }
}