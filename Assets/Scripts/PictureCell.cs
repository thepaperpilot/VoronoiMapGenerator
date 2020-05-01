using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoronoiMap;

public class PictureCell : MonoBehaviour
{
    public Cell cell;

    public void Init()
    {
        Debug.Log("I have " + cell.vertices.Count + " vertices");
        List<Vector2> verts = new List<Vector2>();
        foreach(Vertex v in cell.vertices)
        {
            verts.Add(v.pos - cell.pos);
        }
        verts.Sort(new CounterClockwiseVector2Comparer());
        List<Vector2> hull = (List<Vector2>)ConvexHull.MakeHull(verts);

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(hull.ToArray());
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[hull.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(hull[i].x, hull[i].y, 0);
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        gameObject.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void Rotate(float degrees)
    {
        Renderer rend = GetComponent<Renderer>();
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        rend.GetPropertyBlock(props);
        props.SetFloat("_Rotation", degrees);
        rend.SetPropertyBlock(props);
    }

    public class CounterClockwiseVector2Comparer : IComparer<Vector2>
    {
        public int Compare(Vector2 v1, Vector2 v2)
        {
            return -Mathf.Atan2(v1.x, v1.y).CompareTo(Mathf.Atan2(v2.x, v2.y));
        }
    }
}
