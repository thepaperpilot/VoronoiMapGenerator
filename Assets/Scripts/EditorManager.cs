using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
