using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoronoiMap;

public class PictureEdge : MonoBehaviour
{
    public Edge edge;
    public bool painted = false;
    [SerializeField]
    Transform collider;

    public void Init()
    {
        LineRenderer rend = GetComponent<LineRenderer>();
        Vector2 start = rend.GetPosition(0);
        Vector2 end = rend.GetPosition(1);
        collider.localPosition = start;
        collider.LookAt(end);
        collider.localScale = new Vector3(1, 1, (end - start).magnitude);
    }
}
