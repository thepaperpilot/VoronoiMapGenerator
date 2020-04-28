using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Geometry;
using static BeachLine;
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

        public Vertex(Vector2 pos, List<Cell> centers)
        {
            this.pos = pos;
            this.centers = centers;
            foreach(Cell c in centers)
            {
                c.vertices.Add(this);
            }
        }
    }

    public class Edge : System.IEquatable<Edge>
    {
        public Vertex v1;
        public Vertex v2;
        public Cell c1;
        public Cell c2;

        public Edge(Cell c1, Cell c2)
        {
            this.c1 = c1;
            this.c2 = c2;

            c1.edges.Add(this);
            c2.edges.Add(this);
        }

        public bool Equals(Edge other)
        {
            return (c1 == other.c1 && c2 == other.c2) || (c1 == other.c2 && c2 == other.c1);
        }

        public void AddVertex(Vertex v)
        {
            if(v1 == null)
            {
                v1 = v;
                v1.edges.Add(this);
            }
            else if(v2 == null)
            {
                v2 = v;
                v2.edges.Add(this);
            }
            else
            {
                if (v1.edges.Count == 1 && v1.centers.Count == 2)
                {
                    foreach(Cell c in v1.centers)
                    {
                        c.vertices.Remove(v1);
                        c.vertices.Add(v);
                    }
                    v1 = v;
                    v1.edges.Add(this);
                }
            }
        }
    }

    // Used for generating the maps itself
    private abstract class VoronoiEvent {
        public abstract Vector2 pos { get; }
    }

    private class SiteEvent : VoronoiEvent
    {
        public Cell site;
        public override Vector2 pos { get { return site.pos; } }
    }

    private class VertexEvent : VoronoiEvent, System.IEquatable<VertexEvent>
    {
        public List<Cell> centers;
        public Vector2 middle;
        Vector2 bottom;
        public override Vector2 pos { get { return bottom; } }

        public VertexEvent(Cell c1, Cell c2, Cell c3)
        {
            if (c1 == null || c2 == null || c3 == null)
                return;
            centers = new List<Cell>() { c1, c2, c3 };
            middle = CircumcenterPoints(c1.pos, c2.pos, c3.pos);
            bottom = new Vector2(middle.x, middle.y - (c1.pos - middle).magnitude); //Bottom of the circle
        }

        public bool Equals(VertexEvent other)
        {
            foreach(Cell c in centers)
            {
                if (!other.centers.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }
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

        if (cells == null)
            StartCoroutine(Randomize());

        //ConstructMap();
    }

    public IEnumerator Randomize() {
        // Generate cells
        cells = new List<Cell>();
        for (int i = 0; i < ConfigurationManager.Instance.numCells; i++) {
            cells.Add(new Cell {
                pos = new Vector2(
                    Random.Range(0, ConfigurationManager.Instance.width),
                    Random.Range(0, ConfigurationManager.Instance.height)
                )
            });
        }
        edges = new List<Edge>();
        vertices = new List<Vertex>();
        // We'll use a sweeping algorithm to calculate the vertices and edges
        // Start with a priority queue for our events, initially storing
        // all our site events (each cell) sorted by y-coord
        SortedSet<VoronoiEvent> events = new SortedSet<VoronoiEvent>(new VoronoiEventComparer());
        foreach (Cell cell in cells) {
            events.Add(new SiteEvent {
                site = cell
            });
        }
        float xMin = cells.Select(x => x.pos.x).Min();
        float xMax = cells.Select(x => x.pos.x).Max();
        //Simplistic processing of first site
        SiteEvent first = (SiteEvent)events.Min;
        events.Remove(first);
        float sweep = first.pos.y;
        BeachLine beach = new BeachLine(xMin, xMax, sweep);
        beach.Insert(first.site);

        GameObject sweepObj = Instantiate(sweepPrefab, lineRenderersContainer);
        LineRenderer sweepRend = sweepObj.GetComponentInChildren<LineRenderer>();
        GameObject eventObj = Instantiate(eventPrefab, verticesContainer);
        List<GameObject> beachObjs = new List<GameObject>();
        List<GameObject> diagramObjs = new List<GameObject>();
        foreach (Cell cell in cells)
        {
            GameObject cellGObject = Instantiate(cellPrefab, cellsContainer);
            cellGObject.transform.localPosition = cell.pos;
            // TODO implement CellController and initialize it
        }
        sweepRend.SetPositions(new Vector3[] { new Vector2(xMin, sweep), new Vector2(xMax, sweep) });
        MakeBeach(beachObjs, beach.GetPoints(sweep));
        

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
            if (e.GetType() == typeof(SiteEvent)) {
                SiteEvent se = (SiteEvent)e;
                Debug.Log("Site Event");

                sweep = se.pos.y;
                
                sweepRend.SetPositions(new Vector3[] { new Vector2(xMin,sweep), new Vector2(xMax,sweep) });
                eventObj.transform.localPosition = se.pos;
                //beach.Validate(sweep);
                MakeBeach(beachObjs, beach.GetPoints(sweep));
                pause = true;
                while (pause)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        pause = false;
                    }
                    yield return null;
                }
                beach.Validate(sweep);
                MakeBeach(beachObjs, beach.GetPoints(sweep));
                pause = true;
                while (pause)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        pause = false;
                    }
                    yield return null;
                }
                BeachLine.HitInfo hit = beach.Insert(se.site);
                Debug.Log(se.site == null);
                Debug.Log(hit.target == null);
                Edge newEdge = new Edge(se.site, hit.target);
                Vertex newVert = new Vertex((se.site.pos + hit.target.pos) / 2, new Cell[] { se.site, hit.target }.ToList());
                newEdge.v1 = newVert;
                edges.Add(newEdge);
                vertices.Add(newVert);

                VertexEvent leftTriplet = new VertexEvent(hit.leftMost, hit.target, se.site);
                if (!events.Contains(leftTriplet) && hit.leftMost != null)
                {
                    events.Add(leftTriplet);
                }
                VertexEvent rightTriplet = new VertexEvent(se.site, hit.target, hit.rightMost);
                if (!events.Contains(rightTriplet) && hit.rightMost != null)
                {
                    events.Add(rightTriplet);
                }
                MakeBeach(beachObjs, beach.GetPoints(sweep));
                pause = true;
                while (pause)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        pause = false;
                    }
                    yield return null;
                }
                //MakeBeach(beachObjs, beach.GetPoints(sweep));
                //pause = true;
                while (pause)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        pause = false;
                    }
                    yield return null;
                }
            } else {
                VertexEvent ve = (VertexEvent)e;
                Debug.Log("Vertex event");

                sweep = ve.pos.y;
                
                sweepRend.SetPositions(new Vector3[] { new Vector2(xMin, sweep), new Vector2(xMax, sweep) });
                eventObj.transform.localPosition = ve.middle;
                //beach.Validate(sweep);
                MakeBeach(beachObjs, beach.GetPoints(sweep));
                pause = true;
                while (pause)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        pause = false;
                    }
                    yield return null;
                }
                beach.Validate(sweep);
                MakeBeach(beachObjs, beach.GetPoints(sweep));
                pause = true;
                while (pause)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        pause = false;
                    }
                    yield return null;
                }
                if (!beach.Delete(ve.centers[0], ve.centers[1], ve.centers[2], new Vector2(ve.pos.x, sweep)))
                {
                    continue;
                }
                Vertex v = new Vertex(ve.middle, ve.centers);
                Edge query = new Edge(ve.centers[0], ve.centers[1]);
                Edge e1 = ve.centers[1].edges.Find(edge => edge.Equals(query));
                e1.AddVertex(v);
                query = new Edge(ve.centers[1], ve.centers[2]);
                Edge e2 = ve.centers[1].edges.Find(edge => edge.Equals(query));
                e2.AddVertex(v);
                Edge e3 = new Edge(ve.centers[0], ve.centers[2]);
                e3.AddVertex(v);
                edges.Add(e3);
                MakeVoronoi(diagramObjs);
                pause = true;
                while (pause)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        pause = false;
                    }
                    yield return null;
                }
                List<VertexEvent> toRemove = events.OfType<VertexEvent>().Where(vEvent=> vEvent.centers.Contains(ve.centers[1])).ToList();
                foreach(VertexEvent vEvent in toRemove)
                {
                    events.Remove(vEvent);
                    if (!vEvent.centers.Contains(ve.centers[0]))
                    {
                        VertexEvent newTriplet = new VertexEvent(vEvent.centers[0], vEvent.centers[1], vEvent.centers[2]);
                        int index = newTriplet.centers.IndexOf(ve.centers[1]);
                        newTriplet.centers[index] = ve.centers[0];
                        if (!events.Contains(newTriplet))
                        {
                            events.Add(newTriplet);
                        }
                    }
                    if (!vEvent.centers.Contains(ve.centers[2]))
                    {
                        VertexEvent newTriplet = new VertexEvent(vEvent.centers[0], vEvent.centers[1], vEvent.centers[2]);
                        int index = newTriplet.centers.IndexOf(ve.centers[1]);
                        newTriplet.centers[index] = ve.centers[2];
                        if (!events.Contains(newTriplet))
                        {
                            events.Add(newTriplet);
                        }
                    }
                }
            }
        }

        Debug.Log("missing v1 " + edges.Where(edge => edge.v1 == null && edge.v2 != null).Count());
        Debug.Log("missing v2 " + edges.Where(edge => edge.v1 != null && edge.v2 == null).Count());
        Debug.Log("missing both " + edges.Where(edge => edge.v1 == null && edge.v2 == null).Count());

        edges.RemoveAll(edge => edge.v1 == null && edge.v2 == null);
        foreach (Edge edge in edges)
        {
            if(edge.v2 == null)
            {
                Debug.Log("endpoint");
                edge.v2 = new Vertex((edge.c1.pos + edge.c2.pos)/2f, new Cell[]{ edge.c1, edge.c2 }.ToList());
            }
        }

        Debug.Log("missing v1 " + edges.Where(edge => edge.v1 == null && edge.v2 != null).Count());
        Debug.Log("missing v2 " + edges.Where(edge => edge.v1 != null && edge.v2 == null).Count());
        Debug.Log("missing both " + edges.Where(edge => edge.v1 == null && edge.v2 == null).Count());

        /*
        // Remove old map features
        while (lineRenderersContainer.childCount > 0)
            DestroyImmediate(lineRenderersContainer.GetChild(0));
        while (cellsContainer.childCount > 0)
            DestroyImmediate(cellsContainer.GetChild(0));
        while (verticesContainer.childCount > 0)
            DestroyImmediate(verticesContainer.GetChild(0));

        // Construct current map
        //ConstructMap();
        */
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
        foreach (Vertex v in vertices)
        {
            GameObject g = Instantiate(vertexPrefab, verticesContainer);
            g.transform.localPosition = v.pos;
            objs.Add(g);
        }
        foreach(Edge e in edges)
        {
            if (e.v1 == null || e.v2 == null)
                continue;
            GameObject g = Instantiate(lineRendererPrefab, lineRenderersContainer);
            LineRenderer line = g.GetComponentInChildren<LineRenderer>();
            line.SetPositions(new Vector3[] { e.v1.pos, e.v2.pos });
            objs.Add(g);
        }
    }

}
