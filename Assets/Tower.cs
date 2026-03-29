using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 7.5f;
    public float fireRate = 1f;
    public int damage = 2;

    public GameObject projectilePrefab;
    public Transform firePoint;

    [HideInInspector] public int level = 1;

    private float fireTimer;

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
        foreach (Enemy e in enemies)
        {
            if (Vector3.Distance(transform.position, e.transform.position) <= range)
                return e;
        }
        return null;
    }

    void Shoot(Enemy target)
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();
        p.damage = damage;
        p.SetTarget(target);
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
    }
}