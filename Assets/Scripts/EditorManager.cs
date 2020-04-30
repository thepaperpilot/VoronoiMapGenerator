using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public EditMode mode { get { return _mode; } }
    EditMode _mode = EditMode.OFF;
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
    private VoronoiMap map;
    [SerializeField]
    private GameObject cellPrefab;



    Transform selected = null;

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
                    if(_mode == EditMode.ADD)
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
                    if(_mode == EditMode.DELETE)
                    {
                        map.GetDiagram().cells.Remove(hit.collider.gameObject.GetComponent<PictureCell>().cell);
                        Destroy(hit.collider.gameObject);
                    }
                    else if(_mode == EditMode.MOVE)
                    {
                        selected = hit.collider.transform;
                    }
                }
                
            }
        }
    }

    public void SetMode(string value)
    {
        switch (value.ToUpper())
        {
            case "MOVE":
                SetMode(EditMode.MOVE);
                break;
            case "ADD":
                SetMode(EditMode.ADD);
                break;
            case "DELETE":
                SetMode(EditMode.DELETE);
                break;
        }
    }

    void SetMode(EditMode value)
    {
        _mode = _mode == value ? EditMode.OFF : value;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (_mode == EditMode.MOVE)
            moveButton.color = activeModeButton;
        else
            moveButton.color = inactiveModeButton;

        if (_mode == EditMode.ADD)
            addButton.color = activeModeButton;
        else
            addButton.color = inactiveModeButton;

        if (_mode == EditMode.DELETE)
            deleteButton.color = activeModeButton;
        else
            deleteButton.color = inactiveModeButton;
    }
}
