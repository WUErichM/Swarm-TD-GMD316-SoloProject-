using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float baseSpeed = 10f;
    public int damage = 2;

    private Enemy target;
    private Tower sourceTower;

    public void SetTarget(Enemy t, Tower tower)
    {
        target = t;
        sourceTower = tower;

        if (target != null)
        {
            baseSpeed = Mathf.Max(baseSpeed, target.speed * 2f);
        }

        // ✅ Sniper projectiles are 3x faster
        if (sourceTower != null && sourceTower.towerType == Tower.TowerType.Sniper)
        {
            baseSpeed *= 3f;
        }
    }

    void Update()
    {
        // ✅ If target is gone, try to find a new one (sniper only)
        if (target == null)
        {
            if (sourceTower != null && sourceTower.towerType == Tower.TowerType.Sniper)
            {
                target = FindFurthestEnemy();

                if (target == null)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector3 dir = (target.transform.position - transform.position).normalized;
        transform.position += dir * baseSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.transform.position) < 0.3f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    // ✅ Find furthest enemy in scene
    Enemy FindFurthestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy furthest = null;
        float maxDist = 0f;

        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);

            if (furthest == null || dist > maxDist)
            {
                maxDist = dist;
                furthest = e;
            }
        }

        return furthest;
    }
}