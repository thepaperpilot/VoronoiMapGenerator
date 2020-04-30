using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SetMode(EditMode value)
    {
        _mode = _mode == value ? EditMode.OFF : value;
    }

    private void UpdateButtons()
    {

    }
}
