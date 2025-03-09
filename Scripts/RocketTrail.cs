using UnityEngine;
using System.Collections.Generic;

public class RocketTrail : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float positionScale = 1f; // RocketFlight와 동기화 필요

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.1f * positionScale;
        lineRenderer.endWidth = 0.1f * positionScale;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    public void UpdateLineFirst(Vector3 pos)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, pos);
    }

    public void UpdateLineLast(List<Vector3> positions, int index)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, positions[index]);
    }
}