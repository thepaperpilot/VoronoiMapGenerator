using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Geometry;
using static BeachTree;
using System.Linq;

public class VoronoiMap : MonoBehaviour {

    public class Cell {
        public Vector2 pos;
        public List<Vertex> vertices = new List<Vertex>();
        public List<Edge> edges = new List<Edge>();
    }

    public class Vertex {
        public Vector2 pos;
        public List<Edge> edges = new List<Edge>();
        public List<Cell> centers = new List<Cell>();

        public Vertex(Vector2 pos)
        {
            this.pos = pos;
        }
    }

    public class Edge
    {
        public Vertex start;
        public Vertex end;
        public Cell left;
        public Cell right;
        public Edge section; //Edge may have disconnected sections

        public Vector2 direction;
        public float f;
        public float g; //directional values

        public Edge(Vertex start, Cell left, Cell right)
        {
            this.start = start;
            this.left = left;
            this.right = right;

            f = (right.pos.x - left.pos.x) / (left.pos.y - right.pos.y);
            g = start.pos.y - f * start.pos.x;
            direction = new Vector2(right.pos.y - left.pos.y, left.pos.x- right.pos.x);
        }
    }

    public class Diagram
    {
        public List<Cell> cells = new List<Cell>();
        public List<Vertex> vertices = new List<Vertex>();
        public List<Edge> edges = new List<Edge>();
    }

    // Used for generating the maps itself
    public abstract class VoronoiEvent {
        public abstract Vector2 pos { get; }
    }

    public class SiteEvent : VoronoiEvent
    {
        public Cell site;
        public override Vector2 pos { get { return site.pos; } }
    }

    public class VertexEvent : VoronoiEvent, System.IEquatable<VertexEvent>
    {
        public List<BeachArc> arcs;
        public Vector2 center;
        Vector2 bottom;
        public override Vector2 pos { get { return bottom; } }

        public VertexEvent(BeachArc cl, BeachArc cm, BeachArc cr)
        {
            if (cl == null || cm == null || cr == null)
                return;
            arcs = new List<BeachArc>() { cl, cm, cr };
            center = CircumcenterPoints(cl.site.pos, cm.site.pos, cr.site.pos);
            bottom = new Vector2(center.x, center.y - (cl.site.pos - center).magnitude); //Bottom of the circle
        }

        public bool Equals(VertexEvent other)
        {
            return false;
        }
    }


    private class VoronoiEventComparer : IComparer<VoronoiEvent> {
        public int Compare(VoronoiEvent x, VoronoiEvent y) => (x.pos.y != y.pos.y ? -x.pos.y.CompareTo(y.pos.y) : x.pos.x.CompareTo(y.pos.x));
    };

    Diagram diagram;

    [SerializeField]
    private GameObject lineRendererPrefab;
    [SerializeField]
    private GameObject cellPrefab;
    [SerializeField]
    private GameObject vertexPrefab;

    [SerializeField]
    private GameObject eventPrefab;
    [SerializeField]
    private GameObject beachLinePrefab;
    [SerializeField]
    private GameObject beachVertPrefab;
    [SerializeField]
    private GameObject sweepPrefab;

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

        if (diagram == null)
            StartCoroutine(Randomize());

        //ConstructMap();
    }

    public IEnumerator Randomize() {
        // Generate cells
        diagram = new Diagram();
        for (int i = 0; i < ConfigurationManager.Instance.numCells; i++) {
            diagram.cells.Add(new Cell {
                pos = new Vector2(
                    Random.Range(0, ConfigurationManager.Instance.width),
                    Random.Range(0, ConfigurationManager.Instance.height)
                )
            });
        }
        // We'll use a sweeping algorithm to calculate the vertices and edges
        // Start with a priority queue for our events, initially storing
        // all our site events (each cell) sorted by y-coord
        SortedSet<VoronoiEvent> events = new SortedSet<VoronoiEvent>(new VoronoiEventComparer());
        foreach (Cell cell in diagram.cells) {
            events.Add(new SiteEvent {
                site = cell
            });
        }

        float sweep = ConfigurationManager.Instance.height;
        BeachTree beach = new BeachTree(ConfigurationManager.Instance.width, ConfigurationManager.Instance.height,diagram, events);

        
        GameObject sweepObj = Instantiate(sweepPrefab, lineRenderersContainer);
        LineRenderer sweepRend = sweepObj.GetComponentInChildren<LineRenderer>();
        GameObject eventObj = Instantiate(eventPrefab, verticesContainer);
        List<GameObject> beachObjs = new List<GameObject>();
        List<GameObject> diagramObjs = new List<GameObject>();
        foreach (Cell cell in diagram.cells)
        {
            GameObject cellGObject = Instantiate(cellPrefab, cellsContainer);
            cellGObject.transform.localPosition = cell.pos;
            // TODO implement CellController and initialize it
        }

        //MakeBeach(beachObjs, beach.GetPoints(sweep));

        sweepRend.SetPositions(new Vector3[] { new Vector2(0, sweep), new Vector2(ConfigurationManager.Instance.width, sweep) });
        bool pause = true;
        while (pause)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                pause = false;
            }
            yield return null;
        }

        while (events.Count > 0) {
            VoronoiEvent e = events.Min;
            events.Remove(e);
            sweep = e.pos.y;
            beach.sweep = e.pos.y;


            eventObj.transform.localPosition = e.pos;
            MakeVoronoi(diagramObjs);
            sweepRend.SetPositions(new Vector3[] { new Vector2(0, sweep), new Vector2(ConfigurationManager.Instance.width, sweep) });
            pause = true;
            while (pause)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    pause = false;
                }
                yield return null;
            }
            if (e.GetType() == typeof(SiteEvent)) {
                SiteEvent se = (SiteEvent)e;
                beach.Insert(se.site);
                
            } else {
                VertexEvent ve = (VertexEvent)e;
                beach.Remove(ve);

            }
        }

        beach.Finish();

        Debug.Log("Done");

        yield return null;

        // Remove old map features
        while (lineRenderersContainer.childCount > 0)
            DestroyImmediate(lineRenderersContainer.GetChild(0));
        while (cellsContainer.childCount > 0)
            DestroyImmediate(cellsContainer.GetChild(0));
        while (verticesContainer.childCount > 0)
            DestroyImmediate(verticesContainer.GetChild(0));

        // Construct current map
        //ConstructMap();

    }

    private void ConstructMap() {
        foreach (Cell cell in diagram.cells) {
            GameObject cellGObject = Instantiate(cellPrefab, cellsContainer);
            cellGObject.transform.localPosition = cell.pos;
            // TODO implement CellController and initialize it
        }

        foreach (Vertex vertex in diagram.vertices) {
            GameObject vertexGObject = Instantiate(vertexPrefab, verticesContainer);
            vertexGObject.transform.localPosition = vertex.pos;
        }

        foreach (Edge edge in diagram.edges) {
            GameObject edgeGObject = Instantiate(lineRendererPrefab, lineRenderersContainer);
            LineRenderer line = edgeGObject.GetComponentInChildren<LineRenderer>();
            if(edge.start != null && edge.end != null)
                line.SetPositions(new Vector3[] { edge.start.pos, edge.end.pos });
        }
    }

    private void MakeBeach(List<GameObject>beach, List<Vector2>points)
    {
        while(beach.Count > 0)
        {
            Destroy(beach[0]);
            beach.RemoveAt(0);
        }
        GameObject p = Instantiate(beachVertPrefab, verticesContainer);
        p.transform.localPosition = points[0];
        beach.Add(p);
        for(int i = 1; i < points.Count; i++)
        {
            p = Instantiate(beachVertPrefab, verticesContainer);
            p.transform.localPosition = points[i];
            beach.Add(p);

            GameObject l = Instantiate(beachLinePrefab, lineRenderersContainer);
            LineRenderer line = l.GetComponentInChildren<LineRenderer>();
            line.SetPositions(new Vector3[] { points[i-1], points[i] });
            beach.Add(l);
        }
    }

    private void MakeVoronoi(List<GameObject> objs)
    {
        while (objs.Count > 0)
        {
            Destroy(objs[0]);
            objs.RemoveAt(0);
        }
        foreach (Vertex v in diagram.vertices)
        {
            GameObject g = Instantiate(vertexPrefab, verticesContainer);
            g.transform.localPosition = v.pos;
            objs.Add(g);
        }
        foreach(Edge e in diagram.edges)
        {
            if (e.start == null || e.end == null)
                continue;
            GameObject g = Instantiate(lineRendererPrefab, lineRenderersContainer);
            LineRenderer line = g.GetComponentInChildren<LineRenderer>();
            line.SetPositions(new Vector3[] { e.start.pos, e.end.pos });
            objs.Add(g);
        }
    }

}
