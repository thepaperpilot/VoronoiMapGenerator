using UnityEngine;
using System.Collections;

// Reference for panning and clamping camera: https://forum.unity.com/threads/click-drag-camera-movement.39513/
// Zooming: https://answers.unity.com/questions/384753/ortho-camera-zoom-to-mouse-point.html
[RequireComponent(typeof(BoxCollider2D))]
public class CameraController : MonoBehaviour {

    private Vector3 dragOrigin;

    private BoxCollider2D boxCollider;

    private void OnEnable() {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void LateUpdate() {
        Bounds areaBounds = boxCollider.bounds;

        // Panning camera
        if (Input.GetMouseButtonDown(1)) {
            dragOrigin = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        } else if (Input.GetMouseButton(1)) {
            Vector3 delta = (Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10))) - Camera.main.transform.position;
            Camera.main.transform.position = dragOrigin - delta;
        }

        // Zooming camera
        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            // Zooming was messing up when scrolling too fast and this limits it to 1 scroll/frame
            float amount = Input.GetAxis("Mouse ScrollWheel") > 0 ? 1 : -1;
            
            // Zoom in that amount
            float oldSize = Camera.main.orthographicSize;
            Camera.main.orthographicSize -= amount;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1, Mathf.Min(areaBounds.size.y, areaBounds.size.x * Screen.height / Screen.width) / 2);
            // Calculate the actual amount zoomed
            amount = oldSize - Camera.main.orthographicSize;
            if (amount == 0) return;

            // Pan the camera based on the amount zoomed
            float multiplier = 1 / (Camera.main.orthographicSize * amount);
            Vector3 zoomCenter = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            Vector3 delta = (zoomCenter - Camera.main.transform.position) * multiplier;
            delta.z = 0;
            Camera.main.transform.position += delta;
        }

        // Clamping camera
        Vector3 linkedCameraPos = Camera.main.transform.position;
        float vertExtent = Camera.main.orthographicSize;
        float horizExtent = vertExtent * Screen.width / Screen.height;

        Camera.main.transform.position = new Vector3(
            Mathf.Clamp(linkedCameraPos.x, areaBounds.min.x + horizExtent, areaBounds.max.x - horizExtent),
            Mathf.Clamp(linkedCameraPos.y, areaBounds.min.y + vertExtent, areaBounds.max.y - vertExtent),
            linkedCameraPos.z);
    }
}
