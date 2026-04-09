using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int hp;
    public int money;

    public int score; // ✅ NEW

    public float lastTowerCost;
    public int towersBuilt;

    public float gameTime;

    // Enemy stats for saving spawner
    public float enemyHP;
    public float enemySpeed;

    // Save towers
    public List<TowerData> towers = new List<TowerData>();

    // Save enemies
    public List<EnemyData> enemies = new List<EnemyData>();
}

// Save individual enemy
[Serializable]
public class EnemyData
{
    public float x, y, z;
    public int health;
    public float speed;
    public int reward;

    public EnemyData(Enemy e)
    {
        x = e.transform.position.x;
        y = e.transform.position.y;
        z = e.transform.position.z;
        health = e.health;
        speed = e.speed;
        reward = e.reward;
    }
}