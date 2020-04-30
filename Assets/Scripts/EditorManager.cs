using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static VoronoiMap;

public class EditorManager : MonoBehaviour
{

    public enum EditMode
    {
        OFF = 0,
        MOVE,
        ADD,
        DELETE
    }

    public enum EdgePaint
    {
        OFF = 0,
        ERASER,
        RIVER,
        ROAD,
        WALL
    }

    public enum CellPaint
    {
        OFF = 0,
        ERASER,
        FOREST,
        FARM,
        RURAL,
        URBAN
    }

    public EditMode editMode { get { return _editMode; } }
    EditMode _editMode = EditMode.OFF;
    EdgePaint edgePaint = EdgePaint.OFF;
    CellPaint cellPaint = CellPaint.OFF;
    Transform selected = null;
    public static EditorManager Instance;

    [SerializeField]
    private Color activeModeButton;
    [SerializeField]
    private Color inactiveModeButton;

    [SerializeField]
    private Image moveButton;
    [SerializeField]
    private Image addButton;
    [SerializeField]
    private Image deleteButton;
    [SerializeField]
    private TextMeshProUGUI edgePaintText;
    [SerializeField]
    private TextMeshProUGUI cellPaintText;
    string defaultEdgeText;
    string defaultCellText;

    [SerializeField]
    private VoronoiMap map;
    [SerializeField]
    private GameObject cellPrefab;

    [SerializeField]
    private Color riverColor;
    [SerializeField]
    private Color roadColor;
    [SerializeField]
    private Color wallColor;



    [SerializeField]
    private LayerMask bgMask;

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        defaultCellText = cellPaintText.text;
        defaultEdgeText = edgePaintText.text;
    }

    void Update()
    {
        Vector3 localPoint;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (selected != null)
        {          
            if(Physics.Raycast(ray, out hit, 1000f, bgMask))
            {
                localPoint = map.transform.worldToLocalMatrix * new Vector3(hit.point.x, hit.point.y, 0);
                selected.localPosition = localPoint;
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                selected.GetComponent<PictureCell>().cell.pos = new Vector2(selected.localPosition.x, selected.localPosition.y);
                selected = null;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Background"))
                {
                    if(_editMode == EditMode.ADD)
                    {
                        localPoint = map.transform.worldToLocalMatrix * new Vector3(hit.point.x, hit.point.y, 0);
                        Transform newCell = Instantiate(cellPrefab, map.cellsContainer).transform;
                        newCell.localPosition = localPoint;
                        Cell site = new Cell{ pos = new Vector2(localPoint.x, localPoint.y) };
                        newCell.GetComponent<PictureCell>().cell = site;
                        map.GetDiagram().cells.Add(site);
                    }
                }
                else if (hit.collider.CompareTag("Site"))
                {
                    if(_editMode == EditMode.DELETE)
                    {
                        map.GetDiagram().cells.Remove(hit.collider.gameObject.GetComponent<PictureCell>().cell);
                        Destroy(hit.collider.gameObject);
                    }
                    else if(_editMode == EditMode.MOVE)
                    {
                        selected = hit.collider.transform;
                    }
                }
                else if (hit.collider.CompareTag("Edge"))
                {
                    if(edgePaint != EdgePaint.OFF)
                    {
                        Transform parent = hit.collider.transform.parent;
                        LineRenderer rend = parent.GetComponent<LineRenderer>();
                        PictureEdge edge = parent.GetComponent<PictureEdge>();

                        switch (edgePaint)
                        {
                            case EdgePaint.RIVER:
                                rend.startColor = riverColor;
                                rend.endColor = riverColor;
                                edge.painted = true;
                                break;
                            case EdgePaint.ROAD:
                                rend.startColor = roadColor;
                                rend.endColor = roadColor;
                                edge.painted = true;
                                break;
                            case EdgePaint.WALL:
                                rend.startColor = wallColor;
                                rend.endColor = wallColor;
                                edge.painted = true;
                                break;
                            case EdgePaint.ERASER:
                                rend.startColor = StyleManager.Instance.GetForeground();
                                rend.endColor = StyleManager.Instance.GetForeground();
                                edge.painted = false;
                                break;
                        }
                    }
                    
                }
                
            }
        }
    }

    public void SetEditMode(string value)
    {
        switch (value.ToUpper())
        {
            case "MOVE":
                SetEditMode(EditMode.MOVE);
                break;
            case "ADD":
                SetEditMode(EditMode.ADD);
                break;
            case "DELETE":
                SetEditMode(EditMode.DELETE);
                break;
        }
    }

    void SetEditMode(EditMode value)
    {
        _editMode = _editMode == value ? EditMode.OFF : value;
        edgePaint = EdgePaint.OFF;
        cellPaint = CellPaint.OFF;
        UpdateButtons();
        UpdatePaintText();
    }

    private void UpdateButtons()
    {
        if (_editMode == EditMode.MOVE)
            moveButton.color = activeModeButton;
        else
            moveButton.color = inactiveModeButton;

        if (_editMode == EditMode.ADD)
            addButton.color = activeModeButton;
        else
            addButton.color = inactiveModeButton;

        if (_editMode == EditMode.DELETE)
            deleteButton.color = activeModeButton;
        else
            deleteButton.color = inactiveModeButton;
    }

    public void SetEdgePaint(string value)
    {
        switch (value.ToUpper())
        {
            case "RIVER":
                SetEdgePaint(EdgePaint.RIVER);
                break;
            case "ROAD":
                SetEdgePaint(EdgePaint.ROAD);
                break;
            case "WALL":
                SetEdgePaint(EdgePaint.WALL);
                break;
            case "ERASER":
                SetEdgePaint(EdgePaint.ERASER);
                break;
        }
    }

    void SetEdgePaint(EdgePaint paint)
    {
        edgePaint = edgePaint == paint ? EdgePaint.OFF : paint;
        cellPaint = CellPaint.OFF;
        _editMode = EditMode.OFF;
        UpdatePaintText();
        UpdateButtons();
    }

    public void SetCellPaint(string value)
    {
        switch (value.ToUpper())
        {
            case "FOREST":
                SetCellPaint(CellPaint.FOREST);
                break;
            case "FARM":
                SetCellPaint(CellPaint.FARM);
                break;
            case "RURAL":
                SetCellPaint(CellPaint.RURAL);
                break;
            case "URBAN":
                SetCellPaint(CellPaint.URBAN);
                break;
            case "ERASER":
                SetCellPaint(CellPaint.ERASER);
                break;
        }
    }

    void SetCellPaint(CellPaint paint)
    {
        cellPaint = cellPaint == paint ? CellPaint.OFF : paint;
        edgePaint = EdgePaint.OFF;
        _editMode = EditMode.OFF;
        UpdatePaintText();
        UpdateButtons();
    }

    void UpdatePaintText()
    {
        if (edgePaint == EdgePaint.OFF)
            edgePaintText.text = defaultEdgeText;
        else
            edgePaintText.text = defaultEdgeText + " (" + edgePaint.ToString() + ")";
        if (cellPaint == CellPaint.OFF)
            cellPaintText.text = defaultCellText;
        else
            cellPaintText.text = defaultCellText + " (" + cellPaint.ToString() + ")";
    }
}
