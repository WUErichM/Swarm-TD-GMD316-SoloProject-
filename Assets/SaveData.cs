using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int hp;
    public int money;
    public int score;

    public float lastTowerCost;
    public int towersBuilt;

    public float gameTime;

    public float enemyHP;
    public float enemySpeed;

    public List<TowerData> towers = new List<TowerData>();
    public List<EnemyData> enemies = new List<EnemyData>();

    public List<RunData> leaderboard = new List<RunData>();
}

[Serializable]
public class EnemyData
{
    public float x, y, z;
    public int health;
    public float speed;
    public int reward;

    public EnemyData() { }

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