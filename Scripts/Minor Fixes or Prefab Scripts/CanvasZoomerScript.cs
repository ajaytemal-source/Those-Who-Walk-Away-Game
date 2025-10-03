using UnityEngine;
using System.Collections;

public class CanvasZoomerScript : MonoBehaviour
{
    public RectTransform targetUI;
    public float zoomedSize = 2f;
    public float zoomDuration = 20f;
    public float waitAtZoom = 1f;

    private Camera cam;
    private float originalSize;
    private Vector3 originalPos;

    void Start()
    {
        cam = Camera.main;
        originalSize = cam.orthographicSize;
        originalPos = cam.transform.position;

        //StartCoroutine(ZoomSequence());
    }

    IEnumerator ZoomSequence()
    {
        Vector3 targetCenter = new Vector3(targetUI.position.x, targetUI.position.y, originalPos.z);
        float targetSize = CalculateOrthographicSizeToFit(targetUI, cam);

        yield return StartCoroutine(ZoomTo(targetCenter, targetSize));
        yield return new WaitForSeconds(waitAtZoom);
        yield return StartCoroutine(ZoomTo(originalPos, originalSize));
    }

    IEnumerator ZoomTo(Vector3 targetPosition, float targetSize)
    {
        float elapsed = 0f;
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;

        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cam.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = targetPosition;
        cam.orthographicSize = targetSize;
    }

        float CalculateOrthographicSizeToFit(RectTransform target, Camera cam)
        {
        // Get world corners of the RectTransform
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        float height = Vector3.Distance(corners[1], corners[0]); // top-left to bottom-left
        float width = Vector3.Distance(corners[3], corners[0]);  // bottom-left to bottom-right

        float aspect = cam.aspect;

        // Figure out what size the orthographic camera should be
        float sizeByHeight = height / 2f;
        float sizeByWidth = (width / aspect) / 2f;

        return Mathf.Max(sizeByHeight, sizeByWidth); // zoom just enough to fit the bigger side
        }
}