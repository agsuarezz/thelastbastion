using UnityEngine;

public class PrincipalRoute : MonoBehaviour
{
    public Transform[] waypoints;

    public Vector3[] GetPositions()
    {
        Vector3[] positions = new Vector3[waypoints.Length];

        for (int i = 0; i < waypoints.Length; i++)
        {
            positions[i] = waypoints[i].position;
        }

        return positions;
    }
}