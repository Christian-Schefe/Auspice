using UnityEngine;

public class PortalVisuals : EntityVisuals
{
    public LineRenderer lineRenderer;

    private Camera cam;

    public void SetDestination(Vector3 destination)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, destination);
    }

    private void Update()
    {
        if (cam == null) cam = Camera.main;

        var worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        var dist = Mathf.Max(Mathf.Abs(worldPos.x - transform.position.x), Mathf.Abs(worldPos.y - transform.position.y));

        lineRenderer.enabled = dist <= 0.5f;
    }
}
