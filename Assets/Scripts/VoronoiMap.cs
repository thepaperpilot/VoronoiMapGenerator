using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiMap : MonoBehaviour {

    public struct Cell {
        public Vector2 pos;
        public List<Vertex> vertices;
    }

    public struct Vertex {
        public Vector2 pos;
        public List<Edge> edges;
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
        public bool siteEvent;
        public Vector2 pos;
    }
    private class VoronoiEventComparer : IComparer<VoronoiEvent>  {
        public int Compare(VoronoiEvent x, VoronoiEvent y) => (int)(x.pos.x - y.pos.y);
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

        // We'll use a sweeping algorithm to calculate the vertices and edges
        // Start with a priority queue for our events, initially storing
        // all our site events (each cell) sorted by x-coord
        SortedSet<VoronoiEvent> events = new SortedSet<VoronoiEvent>(new VoronoiEventComparer());
        foreach (Cell cell in cells) {
            events.Add(new VoronoiEvent {
                pos = cell.pos,
                siteEvent = true
            });
        }

        while (events.Count > 0) {
            VoronoiEvent e = events.Min;
            events.Remove(e);
            if (e.siteEvent) {

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
}
