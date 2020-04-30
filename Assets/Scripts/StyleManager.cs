using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StyleManager : MonoBehaviour {

    public static StyleManager Instance;

    [SerializeField]
    private TextMeshProUGUI titleLabel;
    [SerializeField]
    private Camera screenshotCamera;
    [SerializeField]
    private GameObject edgePrefab;
    [SerializeField]
    private GameObject cellPrefab;
    [SerializeField]
    private VoronoiMap voronoiMap;
    [SerializeField]
    private Color initialForeground;
    [SerializeField]
    private Color initialBackground;

    private void OnEnable() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetBackground(initialBackground);
            SetForeground(initialForeground);
        } else {
            Destroy(gameObject);
        }
    }

    public void SetBackground(string bg) {
        if (ColorUtility.TryParseHtmlString(bg, out Color color))
            SetBackground(color);
    }

    public void SetForeground(string fg) {
        if (ColorUtility.TryParseHtmlString(fg, out Color color))
            SetForeground(color);
    }

    private void SetBackground(Color bg) {
        Camera.main.backgroundColor = bg;
        screenshotCamera.backgroundColor = bg;
    }

    private void SetForeground(Color fg) {
        titleLabel.color = fg;
        foreach (TextMeshPro tmp in voronoiMap.cellsContainer.GetComponentsInChildren<TextMeshPro>())
            tmp.color = fg;
        cellPrefab.GetComponentInChildren<TextMeshPro>().color = fg;
        foreach (LineRenderer l in voronoiMap.lineRenderersContainer.GetComponentsInChildren<LineRenderer>()) {
            l.startColor = fg;
            l.endColor = fg;
        }
        LineRenderer lr = edgePrefab.GetComponentInChildren<LineRenderer>();
        lr.startColor = fg;
        lr.endColor = fg;
    }
}
