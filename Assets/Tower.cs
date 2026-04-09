using UnityEngine;

public class Tower : MonoBehaviour
{
    public enum TowerType { Standard, Melee, Sniper }
    public TowerType towerType;

    public float range = 7.5f;
    public float fireRate = 1f;
    public int damage = 2;

    public GameObject projectilePrefab;
    public Transform firePoint;

    [HideInInspector] public int level = 1;

    private float fireTimer;

    void Start()
    {
        ApplyTowerTypeStats();
    }

    void Update()
    {
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

            case TowerType.Standard:
            default:
                break;
        }
    }

    public void ApplyScaling(int towerIndex)
    {
        damage += towerIndex;
        fireRate *= Mathf.Max(0.7f, 1f - (towerIndex * 0.03f));
    }

    void OnMouseDown()
    {
        Upgrade();
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

            // ✅ MELEE → closest enemy
            if (towerType == TowerType.Melee)
            {
                if (bestTarget == null || dist < bestValue)
                {
                    bestValue = dist;
                    bestTarget = e;
                }
            }
            // ✅ SNIPER → furthest enemy
            else if (towerType == TowerType.Sniper)
            {
                if (bestTarget == null || dist > bestValue)
                {
                    bestValue = dist;
                    bestTarget = e;
                }
            }
            // ✅ STANDARD → first found
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

        // ✅ IMPORTANT: pass this tower into projectile
        p.SetTarget(target, this);
    }

    public void Upgrade()
    {
        int cost = level * 50;
        if (!GameManager.instance.SpendMoney(cost)) return;

        level++;
        damage += 1;
        fireRate *= 0.9f;
    }

    public void LoadFromData(TowerData td)
    {
        damage = td.damage;
        fireRate = td.fireRate;
        range = td.range;
        level = td.level;

        // ✅ Cast int → enum
        towerType = (TowerType)td.towerType;

        ApplyTowerTypeStats();
    }
}