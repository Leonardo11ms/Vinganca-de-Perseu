using UnityEngine;

public class CameraFollow : MonoBehaviour
{
     public Transform target;
    public BoxCollider2D cameraBounds;
    public float smoothSpeed = 5f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null || cameraBounds == null)
            return;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        Bounds bounds = cameraBounds.bounds;

        float minX = bounds.min.x + width;
        float maxX = bounds.max.x - width;

        float minY = bounds.min.y + height;
        float maxY = bounds.max.y - height;

        float x = minX > maxX
            ? bounds.center.x
            : Mathf.Clamp(target.position.x, minX, maxX);

        float y = minY > maxY
            ? bounds.center.y
            : Mathf.Clamp(target.position.y, minY, maxY);

        Vector3 targetPosition = new Vector3(x, y, transform.position.z);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            Mathf.Clamp(transform.position.y, minY, maxY),
            transform.position.z
        );
    }
}
