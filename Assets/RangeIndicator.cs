using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    public LineRenderer line;
    public int segments = 50;

    public void SetRange(float range)
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        line.positionCount = segments + 1;

        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * range;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * range;

            line.SetPosition(i, new Vector3(x, 0.05f, z));

            angle += 360f / segments;
        }
    }
}