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

    private float baseRange;
    private float baseFireRate;
    private int baseDamage;

    private float slowPercent = 0.20f;
    private float damageBuff = 0.20f;
    private float speedBuff = 0.10f;

    void Start()
    {
        ApplyTowerTypeStats();

        baseRange = range;
        baseFireRate = fireRate;
        baseDamage = damage;
    }

    void Update()
    {
        ResetToBaseStats();

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

    void ResetToBaseStats()
    {
        range = baseRange;
        fireRate = baseFireRate;
        damage = baseDamage;
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
                range = 5f;
                break;

            case TowerType.Empower:
                range = 3f;
                break;
        }
    }

    void ApplySlow()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        foreach (Collider c in hits)
        {
            Enemy e = c.GetComponent<Enemy>();
            if (e != null)
                e.ApplySlow(Mathf.Clamp(slowPercent, 0f, 0.95f));
        }
    }

    void ApplyBuff()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        foreach (Collider c in hits)
        {
            Tower t = c.GetComponent<Tower>();

            if (t == null || t == this) continue;
            if (t.towerType == TowerType.Slow || t.towerType == TowerType.Empower) continue;

            t.damage = Mathf.RoundToInt(t.GetBaseDamage() * (1f + damageBuff));
            t.fireRate = t.GetBaseFireRate() * (1f - speedBuff);
        }
    }

    public void SetInitialLevel(int newLevel, bool isSupport)
    {
        int maxLevel = BuildManager.instance != null ? BuildManager.instance.GetTowerCount() + 1 : newLevel;

        level = Mathf.Clamp(newLevel, 1, maxLevel);

        int levelOffset = level - 1;

        if (!isSupport)
        {
            damage += levelOffset;
            fireRate *= Mathf.Pow(0.92f, levelOffset);
        }
        else
        {
            if (towerType == TowerType.Slow)
            {
                slowPercent = Mathf.Clamp(0.20f + (levelOffset * 0.02f), 0f, 0.95f);
            }
            else if (towerType == TowerType.Empower)
            {
                damageBuff = 0.20f + (levelOffset * 0.01f);
                speedBuff = 0.10f + (levelOffset * 0.01f);
            }
        }

        baseRange = range;
        baseFireRate = fireRate;
        baseDamage = damage;
    }

    public int GetBaseDamage() { return baseDamage; }
    public float GetBaseFireRate() { return baseFireRate; }
    public float GetBaseRange() { return baseRange; }

    public float GetDamageBuff() { return damageBuff; }
    public float GetSpeedBuff() { return speedBuff; }

    void OnMouseDown()
    {
        if (TowerUI.instance != null)
            TowerUI.instance.Show(this);
    }

    Enemy FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy best = null;
        float bestValue = 0f;

        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist > range) continue;

            if (towerType == TowerType.Melee)
            {
                if (best == null || dist < bestValue)
                {
                    bestValue = dist;
                    best = e;
                }
            }
            else if (towerType == TowerType.Sniper)
            {
                if (best == null || dist > bestValue)
                {
                    bestValue = dist;
                    best = e;
                }
            }
            else
            {
                return e;
            }
        }

        return best;
    }

    void Shoot(Enemy target)
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();

        p.damage = damage;
        p.SetTarget(target, this);
    }

    public float GetSlowPercent()
    {
        return slowPercent;
    }

    public void UpgradeToMatchHighest()
    {
        int maxLevel = BuildManager.instance.GetTowerCount();

        if (level >= maxLevel) return;

        int baseCost = Mathf.Max(BuildManager.instance.GetLastCost(), 1);

        float costMultiplier = (towerType == TowerType.Slow || towerType == TowerType.Empower) ? 0.5f : 0.6f;

        int upgradeCost = Mathf.FloorToInt(baseCost * costMultiplier * 0.8f);

        if (!GameManager.instance.SpendMoney(upgradeCost)) return;

        int targetLevel = Mathf.Clamp(maxLevel, 1, maxLevel);

        SetInitialLevel(targetLevel, towerType == TowerType.Slow || towerType == TowerType.Empower);

        if (TowerUI.instance != null)
            TowerUI.instance.Show(this);
    }

    public void LoadFromData(TowerData td)
    {
        damage = td.damage;
        fireRate = td.fireRate;
        range = td.range;

        int maxLevel = BuildManager.instance != null ? BuildManager.instance.GetTowerCount() : td.level;

        level = Mathf.Clamp(td.level, 1, maxLevel);

        towerType = (TowerType)td.towerType;

        ApplyTowerTypeStats();
        SetInitialLevel(level, towerType == TowerType.Slow || towerType == TowerType.Empower);

        baseRange = range;
        baseFireRate = fireRate;
        baseDamage = damage;
    }
}