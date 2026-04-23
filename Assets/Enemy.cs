using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    private float baseSpeed;

    public int health = 5;
    public int reward = 10;

    private Transform[] waypoints;
    private int waypointIndex = 0;

    void Start()
    {
        baseSpeed = speed;
    }

    public void Setup(Transform[] path)
    {
        waypoints = path;
    }

    void Update()
    {
        speed = baseSpeed; // ✅ RESET EACH FRAME

        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[waypointIndex];

        Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            waypointIndex++;

            if (waypointIndex >= waypoints.Length)
                ReachEnd();
        }
    }

    public void ApplySlow(float percent)
    {
        speed *= (1f - percent);
    }

    void ReachEnd()
    {
        GameManager.instance.LoseLife(1);
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