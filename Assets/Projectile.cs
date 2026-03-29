using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float baseSpeed = 10f; // starting speed
    public int damage = 2;

    private Enemy target;

    public void SetTarget(Enemy t)
    {
        target = t;

        // Increase speed based on enemy speed
        if (target != null)
        {
            baseSpeed = Mathf.Max(baseSpeed, target.speed * 2f);
            // ensures projectile is fast enough to reach target
        }
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.transform.position - transform.position).normalized;
        transform.position += dir * baseSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.transform.position) < 0.3f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}