using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    private float baseSpeed;
    private float currentSpeed;

    public int health = 4;
    public int reward = 10;

    private Transform[] waypoints;
    private int waypointIndex = 0;

    private float slowMultiplier = 1f;
    private float slowTimer = 0f;

    void Start()
    {
        baseSpeed = speed;
        currentSpeed = speed;
    }

    public void Setup(Transform[] path)
    {
        waypoints = path;
    }

    void Update()
    {
        if (slowTimer > 0f)
            slowTimer -= Time.deltaTime;
        else
            slowMultiplier = 1f;

        currentSpeed = baseSpeed * slowMultiplier;

        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[waypointIndex];
        Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            waypointIndex++;

            if (waypointIndex >= waypoints.Length)
                ReachEnd();
        }
    }

    public void ApplySlow(float percent)
    {
        float multiplier = Mathf.Clamp(1f - percent, 0.05f, 1f);

        if (multiplier < slowMultiplier)
            slowMultiplier = multiplier;

        slowTimer = 0.2f;
    }

    void ReachEnd()
    {
        GameManager.instance.LoseLife(1);

        // Kill all enemies
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy e in enemies)
            Destroy(e.gameObject);

        // STRONGER nerf
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
            spawner.ApplyGlobalNerf(0.8f, 0.85f); // -20% HP, -15% Speed

        Destroy(gameObject);
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            GameManager.instance.AddMoney(reward);
            Destroy(gameObject);
        }
    }
}