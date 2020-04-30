using UnityEngine;
using System.Collections;
using TMPro;

public class Screenshotter : MonoBehaviour {

    [SerializeField]
    new private Camera camera;
    [SerializeField]
    private Canvas citynameCanvas;
    [SerializeField]
    private int pixelsPerUnit = 100;

    private string filename = "screenshot";
    private bool renderCellNames = false;

    private void OnEnable() {
        camera.enabled = false;
    }

    public void SetFilename(string filename) {
        this.filename = filename;
    }

    public void SetRenderCellNames(bool renderCellNames) {
        this.renderCellNames = renderCellNames;
    }

    public void Screenshot() {
        temp();
        // The first render wasn't having the city name appear in it
        // but its too late to figure out the root cause so I just render twice
        temp();
    }

    private void temp() {
        // Setup render settings
        RectTransform cityNameRectTransform = citynameCanvas.GetComponentInChildren<TextMeshProUGUI>().GetComponent<RectTransform>();
        Vector2 cityNameAnchoredPosition = cityNameRectTransform.anchoredPosition;
        cityNameRectTransform.anchoredPosition = new Vector2(0, -50);
        citynameCanvas.worldCamera = camera;
        camera.enabled = true;
        if (renderCellNames)
            camera.cullingMask |= 1 << LayerMask.NameToLayer("Cell-Names");
        else
            camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Cell-Names"));

        // Render to camera
        RenderTexture rt = RenderTexture.GetTemporary((int)(ConfigurationManager.Instance.width * pixelsPerUnit), (int)(ConfigurationManager.Instance.height * pixelsPerUnit));
        camera.targetTexture = rt;
        camera.orthographicSize = ConfigurationManager.Instance.height / 2;
        camera.aspect = ConfigurationManager.Instance.width / ConfigurationManager.Instance.height;
        camera.transform.position = new Vector3(ConfigurationManager.Instance.width / 2, ConfigurationManager.Instance.height / 2, -10);
        camera.Render();
        camera.targetTexture = null;

        // Save to PNG
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        byte[] bytes = screenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/../" + filename + ".png", bytes);

        // Reset render settings
        cityNameRectTransform.anchoredPosition = cityNameAnchoredPosition;
        citynameCanvas.worldCamera = null;
        camera.enabled = false;
    }
}
