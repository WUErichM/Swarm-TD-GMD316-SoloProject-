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

    private float slowPercent = 0.35f;

    private float damageBuff = 0.5f;
    private float speedBuff = 0.3f;
    private float rangeBuff = 2f;

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
                range = 8f;
                break;

            case TowerType.Empower:
                range = 2f;
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
            t.range = t.GetBaseRange() + rangeBuff;
        }
    }

    public void SetInitialLevel(int newLevel, bool isSupport)
    {
        level = Mathf.Max(newLevel, 1);

        if (!isSupport)
        {
            damage += (level - 1);
            fireRate *= Mathf.Pow(0.92f, level - 1);
        }
        else
        {
            if (towerType == TowerType.Slow)
                slowPercent = Mathf.Clamp(0.35f + (level * 0.01f), 0f, 0.95f);

            else if (towerType == TowerType.Empower)
            {
                damageBuff = 0.5f + (level * 0.05f);
                speedBuff = 0.3f + (level * 0.01f);
            }

            range += (level / 5);
        }

        baseRange = range;
        baseFireRate = fireRate;
        baseDamage = damage;
    }

    public int GetBaseDamage() { return baseDamage; }
    public float GetBaseFireRate() { return baseFireRate; }
    public float GetBaseRange() { return baseRange; }

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
        bool isSupport = (towerType == TowerType.Slow || towerType == TowerType.Empower);

        int highestLevel = isSupport
            ? BuildManager.instance.GetHighestSupportLevel()
            : BuildManager.instance.GetHighestOffensiveLevel();

        if (level >= highestLevel) return;

        int baseCost = Mathf.Max(BuildManager.instance.GetLastCost(), 1);

        int upgradeCost = isSupport
            ? Mathf.FloorToInt(baseCost * 0.5f)
            : Mathf.FloorToInt(baseCost * 0.6f);

        if (!GameManager.instance.SpendMoney(upgradeCost)) return;

        int levelsToGain = highestLevel - level;
        level = highestLevel;

        if (!isSupport)
        {
            damage += levelsToGain;
            fireRate *= Mathf.Pow(0.92f, levelsToGain);
        }
        else
        {
            if (towerType == TowerType.Slow)
                slowPercent = Mathf.Clamp(slowPercent + (levelsToGain * 0.01f), 0f, 0.95f);

            else if (towerType == TowerType.Empower)
            {
                damageBuff += levelsToGain * 0.05f;
                speedBuff += levelsToGain * 0.01f;
            }

            range += (levelsToGain / 5);
        }

        baseRange = range;
        baseFireRate = fireRate;
        baseDamage = damage;

        if (TowerUI.instance != null)
            TowerUI.instance.Show(this);
    }

    public void LoadFromData(TowerData td)
    {
        damage = td.damage;
        fireRate = td.fireRate;
        range = td.range;
        level = Mathf.Max(td.level, 1);
        towerType = (TowerType)td.towerType;

        ApplyTowerTypeStats();

        baseRange = range;
        baseFireRate = fireRate;
        baseDamage = damage;
    }
}