using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiMap : MonoBehaviour {

    public class Cell {
        public Vector2 pos;
        public List<Vertex> vertices;
    }

    public class Vertex {
        public Vector2 pos;
        public List<Edge> edges;
        public List<Cell> centers;
    }

    public struct Edge {
        public Vertex v1;
        public Vertex v2;
        public Cell c1;
        public Cell c2;
    }

    // Used for generating the maps itself
    private struct VoronoiEvent {
        // If false its a circle event
        public bool isSiteEvent;
        public Vector2 pos;
    }
    private class VoronoiEventComparer : IComparer<VoronoiEvent> {
        public int Compare(VoronoiEvent x, VoronoiEvent y) => (x.pos.y != y.pos.y ? -x.pos.y.CompareTo(y.pos.y) : x.pos.x.CompareTo(y.pos.x));
    };

    public List<Cell> cells;
    public List<Vertex> vertices;
    public List<Edge> edges;

    [SerializeField]
    private GameObject lineRendererPrefab;
    [SerializeField]
    private GameObject cellPrefab;
    [SerializeField]
    private GameObject vertexPrefab;

    private Transform lineRenderersContainer;
    private Transform cellsContainer;
    private Transform verticesContainer;

    private void Awake() {
        // Create our containers, for easier organization during development
        lineRenderersContainer = new GameObject("Line Renderers").transform;
        lineRenderersContainer.SetParent(transform);
        cellsContainer = new GameObject("Cells").transform;
        cellsContainer.SetParent(transform);
        verticesContainer = new GameObject("Vertices").transform;
        verticesContainer.SetParent(transform);

        if (cells == null)
            Randomize();

        ConstructMap();
    }

    public void Randomize() {
        // Generate cells
        cells = new List<Cell>();
        for (int i = 0; i < ConfigurationManager.Instance.numCells; i++) {
            cells[i] = new Cell {
                pos = new Vector2(
                    Random.Range(0, ConfigurationManager.Instance.width),
                    Random.Range(0, ConfigurationManager.Instance.height)
                )
            };
        }
        edges = new List<Edge>();
        vertices = new List<Vertex>();
        // We'll use a sweeping algorithm to calculate the vertices and edges
        // Start with a priority queue for our events, initially storing
        // all our site events (each cell) sorted by y-coord
        SortedSet<VoronoiEvent> events = new SortedSet<VoronoiEvent>(new VoronoiEventComparer());
        foreach (Cell cell in cells) {
            events.Add(new VoronoiEvent {
                pos = cell.pos,
                isSiteEvent = true
            });
        }


        while (events.Count > 0) {
            VoronoiEvent e = events.Min;
            events.Remove(e);
            if (e.isSiteEvent) {

            } else {

            }
        }

        // Remove old map features
        while (lineRenderersContainer.childCount > 0)
            DestroyImmediate(lineRenderersContainer.GetChild(0));
        while (cellsContainer.childCount > 0)
            DestroyImmediate(cellsContainer.GetChild(0));
        while (verticesContainer.childCount > 0)
            DestroyImmediate(verticesContainer.GetChild(0));

        // Construct current map
        ConstructMap();
    }

    private void ConstructMap() {
        foreach (Cell cell in cells) {
            GameObject cellGObject = Instantiate(cellPrefab, cellsContainer);
            cellGObject.transform.localPosition = cell.pos;
            // TODO implement CellController and initialize it
        }

        foreach (Vertex vertex in vertices) {
            GameObject vertexGObject = Instantiate(vertexPrefab, verticesContainer);
            vertexGObject.transform.localPosition = vertex.pos;
        }

        foreach (Edge edge in edges) {
            GameObject edgeGObject = Instantiate(lineRendererPrefab, lineRenderersContainer);
            LineRenderer line = edgeGObject.GetComponentInChildren<LineRenderer>();
            line.SetPositions(new Vector3[] { edge.v1.pos, edge.v2.pos });
        }
    }

    private Vector2 CircumcenterPoints(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Line bisector1 = PerpendicularBisector(p1, p2);
        Line bisector2 = PerpendicularBisector(p2, p3);
        return IntersectLines(bisector1, bisector2);
    }

    //Thank you https://en.wikipedia.org/wiki/Special_cases_of_Apollonius%27_problem#Type_4:_Two_points,_one_line
    private Vector2 CircumcenterSweep(Vector2 p1, Vector2 p2, float ySweep)
    {
        Line sweep = new Line(new Vector2(0, ySweep), new Vector2(1, ySweep));
        Vector2 g = IntersectLines(sweep, new Line(p1, p2));
        float dist = Mathf.Sqrt((p1 - g).magnitude * (p2 - g).magnitude);

        //There are two candidates
        Vector2 c1 = new Vector2(g.x + dist, ySweep);
        Vector2 c2 = new Vector2(g.x - dist, ySweep);

        Vector2 m1 = CircumcenterPoints(p1, p2, c1);
        Vector2 m2 = CircumcenterPoints(p1, p2, c2);
        
        //Return the middle of the circle with the smaller radius - I don't know if this is correct but it should be?
        if((p1 - m1).sqrMagnitude <= (p1 - m2).sqrMagnitude)
        {
            return m1;
        }
        else
        {
            return m2;
        }
    }

    //ax+by=c
    class Line
    {
        public float a;
        public float b;
        public float c;

        public Line(float a, float b, float c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public Line(Vector2 p1, Vector2 p2)
        {
            a = p2.y - p1.y;
            b = p1.x - p2.x;
            c = a * p1.x + b * p1.y;
        }
    }

    private Line PerpendicularBisector(Vector2 p1, Vector2 p2)
    {
        Line side = new Line(p1, p2);
        Vector2 midpoint = 0.5f * (p1 + p2);
        return new Line(-side.b, side.a, -side.b * midpoint.x + side.a * midpoint.y);
    }

    private Vector2 IntersectLines(Line l1, Line l2)
    {
        float det = l1.a * l2.b - l2.a * l1.b;
        if(det == 0)
        {
            return Vector2.zero; //Your triangle should not have parallel lines :(
        }
        float x = (l2.b * l1.c - l1.b * l2.c) / det;
        float y = (l1.a * l2.c - l2.a * l1.c) / det;
        return new Vector2(x, y);
    }
}
