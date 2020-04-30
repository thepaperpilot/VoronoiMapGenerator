using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Geometry;
using static BeachTree;
using System.Linq;
using TMPro;
using System;

public class VoronoiMap : MonoBehaviour {

    public class Cell {
        public string name = "";
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

    public class Edge : System.IEquatable<Edge>
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

            start.edges.Add(this);
            left.edges.Add(this);
            right.edges.Add(this);

            f = (right.pos.x - left.pos.x) / (left.pos.y - right.pos.y);
            g = start.pos.y - f * start.pos.x;
            direction = new Vector2(right.pos.y - left.pos.y, left.pos.x- right.pos.x);
        }

        bool IEquatable<Edge>.Equals(Edge other)
        {
            return (start == other.start && end == other.end) || (start == other.end && end == other.start);
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
    [SerializeField]
    private GameObject raycastBGPrefab;

    [HideInInspector]
    public Transform lineRenderersContainer;
    [HideInInspector]
    public Transform cellsContainer;
    [HideInInspector]
    public Transform verticesContainer;
    [HideInInspector]
    public Transform raycastBG;

    private void Awake() {
        // Create our containers, for easier organization during development
        lineRenderersContainer = new GameObject("Line Renderers").transform;
        lineRenderersContainer.SetParent(transform);
        cellsContainer = new GameObject("Cells").transform;
        cellsContainer.SetParent(transform);
        verticesContainer = new GameObject("Vertices").transform;
        verticesContainer.SetParent(transform);
        raycastBG = Instantiate(raycastBGPrefab, transform).transform;
        raycastBG.localPosition = Vector3.zero;

        if (diagram == null)
        {
            Randomize();
        }


        //ConstructMap();
    }

    public void Randomize() {
        ClearCurrent();

        diagram = new Diagram();
        for (int i = 0; i < ConfigurationManager.Instance.numCells; i++)
        {
            diagram.cells.Add(new Cell
            {
                pos = new Vector2(
                    UnityEngine.Random.Range(0, ConfigurationManager.Instance.width),
                    UnityEngine.Random.Range(0, ConfigurationManager.Instance.height)
                ),
                name = "P" + i
            });
        }

        if (ConfigurationManager.Instance.visualize)
            StartCoroutine(GenerateVisualized());
        else
            Generate();
    }

    public void Regenerate()
    {
        ClearCurrent();
        diagram.edges = new List<Edge>();
        diagram.vertices = new List<Vertex>();
        if (ConfigurationManager.Instance.visualize)
            StartCoroutine(GenerateVisualized());
        else
            Generate();
    }

    private void Generate()
    {
        
        // We'll use a sweeping algorithm to calculate the vertices and edges
        // Start with a priority queue for our events, initially storing
        // all our site events (each cell) sorted by y-coord
        SortedSet<VoronoiEvent> events = new SortedSet<VoronoiEvent>(new VoronoiEventComparer());
        foreach (Cell cell in diagram.cells)
        {
            events.Add(new SiteEvent
            {
                site = cell
            });
        }

        float sweep = ConfigurationManager.Instance.height;
        BeachTree beach = new BeachTree(ConfigurationManager.Instance.width, ConfigurationManager.Instance.height, diagram, events);

        while (events.Count > 0)
        {
            VoronoiEvent e = events.Min;
            events.Remove(e);

            sweep = e.pos.y;
            beach.sweep = e.pos.y;

            if (e.GetType() == typeof(SiteEvent))
            {
                SiteEvent se = (SiteEvent)e;
                beach.Insert(se.site);

            }
            else
            {
                VertexEvent ve = (VertexEvent)e;
                beach.Remove(ve);
            }
        }
        beach.Finish();

        List<Edge> halves = diagram.edges.Where(e => e.section != null).ToList();
        foreach (Edge e in halves)
        {
            e.start.edges.Remove(e);
            e.start = e.section.end;
            e.section.left.edges.Remove(e.section);
            e.section.right.edges.Remove(e.section);
            e.section.end.edges.Remove(e.section);
            e.section.start.edges.Remove(e.section);
            diagram.edges.Remove(e.section);
        }

        List<Vertex> orphanVerts = diagram.vertices.Where(v => v.edges.Count == 0).ToList();
        while (orphanVerts.Count > 0)
        {
            diagram.vertices.Remove(orphanVerts[0]);
            orphanVerts.RemoveAt(0);
        }
        List<Edge> orphanEdges = diagram.edges.Where(e => e.start == null && e.end == null).ToList();
        while (orphanEdges.Count > 0)
        {
            diagram.edges.Remove(orphanEdges[0]);
            orphanEdges.RemoveAt(0);
        }

        List<Edge> noDupes = new List<Edge>();
        foreach(Edge e in diagram.edges)
        {
            if(noDupes.Where(x=>x.Equals(e)).Count() > 0)
            {
                e.start.edges.Remove(e);
                e.end.edges.Remove(e);
                e.left.edges.Remove(e);
                e.right.edges.Remove(e);
                Debug.Log("deleted dupe");
            }
            else
            {
                Debug.Log("unique");
                noDupes.Add(e);
            }
        }
        diagram.edges = noDupes;

        Debug.Log("Done");

        ConstructMap();
    }

    private IEnumerator GenerateVisualized()
    {
        // We'll use a sweeping algorithm to calculate the vertices and edges
        // Start with a priority queue for our events, initially storing
        // all our site events (each cell) sorted by y-coord
        SortedSet<VoronoiEvent> events = new SortedSet<VoronoiEvent>(new VoronoiEventComparer());
        foreach (Cell cell in diagram.cells)
        {
            events.Add(new SiteEvent
            {
                site = cell
            });
        }

        float sweep = ConfigurationManager.Instance.height;
        BeachTree beach = new BeachTree(ConfigurationManager.Instance.width, ConfigurationManager.Instance.height, diagram, events);


        GameObject sweepObj = Instantiate(sweepPrefab, lineRenderersContainer);
        LineRenderer sweepRend = sweepObj.GetComponentInChildren<LineRenderer>();
        GameObject eventObj = Instantiate(eventPrefab, verticesContainer);
        List<GameObject> beachObjs = new List<GameObject>();
        List<GameObject> diagramObjs = new List<GameObject>();
        foreach (Cell cell in diagram.cells)
        {
            GameObject cellGObject = Instantiate(cellPrefab, cellsContainer);
            cellGObject.transform.localPosition = cell.pos;
            cellGObject.transform.GetComponentInChildren<TextMeshPro>().text = cell.name;
            // TODO implement CellController and initialize it
        }

        //MakeBeach(beachObjs, beach.GetPoints(sweep));

        sweepRend.SetPositions(new Vector3[] { new Vector2(0, sweep), new Vector2(ConfigurationManager.Instance.width, sweep) });
        yield return new WaitForSeconds(0.05f);
        while (events.Count > 0)
        {
            VoronoiEvent e = events.Min;
            events.Remove(e);
            eventObj.transform.localPosition = e.pos;
            while (sweep-0.1f > e.pos.y)
            {
                sweep = sweep - 0.1f;
                beach.sweep = sweep;

                sweepRend.SetPositions(new Vector3[] { new Vector2(0, sweep), new Vector2(ConfigurationManager.Instance.width, sweep) });
                MakeVoronoi(diagramObjs);
                MakeBeach(beachObjs, beach.GetBeachPoints());
                yield return new WaitForSeconds(0.1f);
            }
            sweep = e.pos.y;
            beach.sweep = e.pos.y;

            sweepRend.SetPositions(new Vector3[] { new Vector2(0, sweep), new Vector2(ConfigurationManager.Instance.width, sweep) });
            MakeVoronoi(diagramObjs);
            MakeBeach(beachObjs, beach.GetBeachPoints());
            yield return new WaitForSeconds(0.1f);

            if (e.GetType() == typeof(SiteEvent))
            {
                SiteEvent se = (SiteEvent)e;
                beach.Insert(se.site);

            }
            else
            {
                VertexEvent ve = (VertexEvent)e;
                beach.Remove(ve);

            }
            MakeVoronoi(diagramObjs);
            MakeBeach(beachObjs, beach.GetBeachPoints());
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Finished Events");
        beach.Finish();

        List<Edge> halves = diagram.edges.Where(e => e.section != null).ToList();
        foreach (Edge e in halves)
        {
            e.start.edges.Remove(e);
            e.start = e.section.end;
            e.section.left.edges.Remove(e.section);
            e.section.right.edges.Remove(e.section);
            e.section.end.edges.Remove(e.section);
            e.section.start.edges.Remove(e.section);
            diagram.edges.Remove(e.section);
        }

        List<Vertex> orphanVerts = diagram.vertices.Where(v => v.edges.Count == 0).ToList();
        while (orphanVerts.Count > 0)
        {
            diagram.vertices.Remove(orphanVerts[0]);
            orphanVerts.RemoveAt(0);
        }
        List<Edge> orphanEdges = diagram.edges.Where(e => e.start == null && e.end == null).ToList();
        while (orphanEdges.Count > 0)
        {
            diagram.edges.Remove(orphanEdges[0]);
            orphanEdges.RemoveAt(0);
        }


        while (beachObjs.Count > 0)
        {
            Destroy(beachObjs[0]);
            beachObjs.RemoveAt(0);
        }
        while (diagramObjs.Count > 0)
        {
            Destroy(diagramObjs[0]);
            diagramObjs.RemoveAt(0);
        }
        // Construct current map
        ConstructMap();
    }

    private void ClearCurrent()
    {
        List<Transform> toDestroy = new List<Transform>();
        for(int i = 0; i < lineRenderersContainer.childCount; i++)
        {
            toDestroy.Add(lineRenderersContainer.GetChild(i));
        }
        for(int i = 0; i < cellsContainer.childCount; i++)
        {
            toDestroy.Add(cellsContainer.GetChild(i));
        }
        for(int i = 0; i < verticesContainer.childCount; i++)
        {
            toDestroy.Add(verticesContainer.GetChild(i));
        }

        while(toDestroy.Count > 0)
        {
            Destroy(toDestroy[0].gameObject);
            toDestroy.RemoveAt(0);
        }

        raycastBG.localScale = new Vector3(ConfigurationManager.Instance.width, ConfigurationManager.Instance.height, 1);
    }

    private void ConstructMap() {
        foreach (Cell cell in diagram.cells) {
            GameObject cellGObject = Instantiate(cellPrefab, cellsContainer);
            cellGObject.transform.localPosition = cell.pos;
            cellGObject.GetComponent<PictureCell>().cell = cell;
            // TODO implement CellController and initialize it
        }

        foreach (Vertex vertex in diagram.vertices) {
            //GameObject vertexGObject = Instantiate(vertexPrefab, verticesContainer);
            //vertexGObject.transform.localPosition = vertex.pos;
        }

        foreach (Edge edge in diagram.edges) {
            
            if(edge.start != null && edge.end != null)
            {
                GameObject edgeGObject = Instantiate(lineRendererPrefab, lineRenderersContainer);
                LineRenderer line = edgeGObject.GetComponentInChildren<LineRenderer>();
                line.SetPositions(new Vector3[] { edge.start.pos, edge.end.pos });
                edgeGObject.GetComponent<PictureEdge>().edge = edge;
                edgeGObject.GetComponent<PictureEdge>().Init();
            }
                
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
            if (float.IsNaN(points[i].x) || float.IsNaN(points[i].y))
                continue;
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

    public Diagram GetDiagram()
    {
        return diagram;
    }

}
