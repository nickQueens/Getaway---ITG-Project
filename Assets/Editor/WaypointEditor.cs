using UnityEditor;
using UnityEngine;

[InitializeOnLoad()]
public class WaypointEditor
{
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(Waypoint waypoint, GizmoType gizmoType)
    {
        return;
        if ((gizmoType & GizmoType.Selected) != 0)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.yellow * 0.5f;
        }

        Gizmos.DrawSphere(waypoint.transform.position, .5f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(waypoint.transform.position + (waypoint.transform.right * waypoint.width / 2f),
            waypoint.transform.position - (waypoint.transform.right * waypoint.width / 2f));

        if (waypoint.previousWaypoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 offset = waypoint.transform.right * waypoint.width / 2f;
            Vector3 offsetTo = waypoint.previousWaypoint.transform.right * waypoint.previousWaypoint.width / 2f;

            Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.previousWaypoint.transform.position + offsetTo);
        }

        if (waypoint.nextWaypoint != null)
        {
            Gizmos.color = Color.green;
            Vector3 offset = waypoint.transform.right * -waypoint.width / 2f;
            Vector3 offsetTo = waypoint.nextWaypoint.transform.right * -waypoint.nextWaypoint.width / 2f;

            Vector3 toNext = waypoint.nextWaypoint.transform.position - waypoint.transform.position;
            Vector3 arrowTo = waypoint.transform.position + (toNext * 0.25f);

            Gizmos.DrawLine(waypoint.transform.position, arrowTo);
            Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.nextWaypoint.transform.position + offsetTo);
        }

        if (waypoint.branches != null)
        {
            foreach(Waypoint branch in waypoint.branches)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(waypoint.transform.position, branch.transform.position);
            }
        }
    }
}
