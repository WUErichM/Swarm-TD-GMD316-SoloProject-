using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public int health = 5;
    public int reward = 10;

    private Transform[] waypoints;
    private int waypointIndex = 0;

    public void Setup(Transform[] path)
    {
        waypoints = path;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[waypointIndex];

        Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            waypointIndex++;

            if (waypointIndex >= waypoints.Length)
            {
                ReachEnd();
            }
        }
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