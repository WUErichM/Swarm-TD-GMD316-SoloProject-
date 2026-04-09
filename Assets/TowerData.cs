using System;

[Serializable]
public class TowerData
{
    public float x, y, z;
    public int damage;
    public float fireRate;
    public float range;
    public int level;

    public int towerType; // NEW

    public TowerData(Tower tower)
    {
        x = tower.transform.position.x;
        y = tower.transform.position.y;
        z = tower.transform.position.z;
        damage = tower.damage;
        fireRate = tower.fireRate;
        range = tower.range;
        level = tower.level;
        towerType = (int)tower.towerType;
    }
}