using UnityEngine;

public class Tower : MonoBehaviour
{
    public enum TowerType { Standard, Melee, Sniper, Slow, Empower }
    public TowerType towerType;

    public float range = 7.5f;
    public float fireRate = 1f;
    public int damage = 2;

    public GameObject projectilePrefab;
    public Transform firePoint;

    [HideInInspector] public int level = 1;

    private float fireTimer;

    private float slowPercent = 0.35f;
    private float damageBuffPercent = 0.5f;

    void Start()
    {
        ApplyTowerTypeStats();
    }

    void Update()
    {
        if (towerType == TowerType.Slow)
        {
            ApplySlow();
            return;
        }

        if (towerType == TowerType.Empower)
        {
            ApplyBuff();
            return;
        }

        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            Enemy target = FindTarget();
            if (target != null)
            {
                Shoot(target);
                fireTimer = 0f;
            }
        }
    }

    void ApplyTowerTypeStats()
    {
        switch (towerType)
        {
            case TowerType.Melee:
                range = 3f;
                damage += 2;
                fireRate *= 0.7f;
                break;

            case TowerType.Sniper:
                range = 999f;
                damage += 4;
                fireRate *= 1.5f;
                break;

            case TowerType.Slow:
            case TowerType.Empower:
                range = 6f;
                break;
        }
    }

    void ApplySlow()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy e in enemies)
        {
            if (Vector3.Distance(transform.position, e.transform.position) <= range)
            {
                float slowAmount = Mathf.Clamp(slowPercent, 0f, 0.95f);
                e.ApplySlow(slowAmount);
            }
        }
    }

    void ApplyBuff()
    {
        Tower[] towers = FindObjectsOfType<Tower>();

        foreach (Tower t in towers)
        {
            if (t == this) continue;
            if (t.towerType == TowerType.Slow || t.towerType == TowerType.Empower) continue;

            if (Vector3.Distance(transform.position, t.transform.position) <= range)
            {
                t.damage += Mathf.RoundToInt(damageBuffPercent * Time.deltaTime);
            }
        }
    }

    public void SetInitialLevel(int newLevel)
    {
        level = newLevel;

        if (towerType != TowerType.Slow && towerType != TowerType.Empower)
        {
            damage += (level - 1);
            fireRate *= Mathf.Pow(0.92f, level - 1);
        }

        if (towerType == TowerType.Slow)
            slowPercent = Mathf.Clamp(0.35f + (level * 0.01f), 0f, 0.95f);

        if (towerType == TowerType.Empower)
            damageBuffPercent = 0.5f + (level * 0.05f);

        range += (level / 5);
    }

    void OnMouseDown()
    {
        if (TowerUI.instance != null)
            TowerUI.instance.Show(this);
    }

    Enemy FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy bestTarget = null;
        float bestValue = 0f;

        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);

            if (dist > range) continue;

            if (towerType == TowerType.Melee)
            {
                if (bestTarget == null || dist < bestValue)
                {
                    bestValue = dist;
                    bestTarget = e;
                }
            }
            else if (towerType == TowerType.Sniper)
            {
                if (bestTarget == null || dist > bestValue)
                {
                    bestValue = dist;
                    bestTarget = e;
                }
            }
            else
            {
                return e;
            }
        }

        return bestTarget;
    }

    void Shoot(Enemy target)
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();

        p.damage = damage;
        p.SetTarget(target, this);
    }

    public void UpgradeToMatchHighest()
    {
        int highestLevel = BuildManager.instance.GetTowerCount();

        if (level >= highestLevel) return;

        int baseCost = BuildManager.instance.GetLastCost();
        int upgradeCost = Mathf.FloorToInt(baseCost * 0.6f);

        if (!GameManager.instance.SpendMoney(upgradeCost)) return;

        int levelsToGain = highestLevel - level;
        level = highestLevel;

        if (towerType == TowerType.Slow)
            slowPercent = Mathf.Clamp(0.35f + (level * 0.01f), 0f, 0.95f);
        else if (towerType == TowerType.Empower)
            damageBuffPercent = 0.5f + (level * 0.05f);
        else
        {
            damage += levelsToGain;
            fireRate *= Mathf.Pow(0.92f, levelsToGain);
        }

        range += (level / 5);

        if (TowerUI.instance != null)
            TowerUI.instance.Show(this);
    }

    public void LoadFromData(TowerData td)
    {
        damage = td.damage;
        fireRate = td.fireRate;
        range = td.range;
        level = td.level;
        towerType = (TowerType)td.towerType;

        ApplyTowerTypeStats();
    }
}