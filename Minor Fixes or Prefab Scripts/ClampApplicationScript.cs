using UnityEngine;

public class ClampToScreen : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        ClampToScreenBounds();
    }

    void ClampToScreenBounds()
    {
        Vector3[] canvasCorners = new Vector3[4];
        canvas.GetComponent<RectTransform>().GetWorldCorners(canvasCorners);

        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        Vector3 offset = Vector3.zero;

        // Left
        if (objectCorners[0].x < canvasCorners[0].x)
            offset.x = canvasCorners[0].x - objectCorners[0].x;

        // Right
        if (objectCorners[2].x > canvasCorners[2].x)
            offset.x = canvasCorners[2].x - objectCorners[2].x;

        // Bottom
        if (objectCorners[0].y < canvasCorners[0].y)
            offset.y = canvasCorners[0].y - objectCorners[0].y;

        // Top
        if (objectCorners[1].y > canvasCorners[1].y)
            offset.y = canvasCorners[1].y - objectCorners[1].y;

        rectTransform.position += offset;
    }
}
