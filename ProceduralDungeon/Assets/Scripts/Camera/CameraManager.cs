using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeedScale = 0.5f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeedScale = 0.1f;
    [SerializeField] private float maxZoomScale = 0.4f;
    
    private float _moveSpeed;
    private float _zoomSpeed;
    private float _minZoom = 1f;
    private float _maxZoom;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    public void CenterOnGrid(Vector2Int gridSize)
    {
        Vector3 center = new Vector3(gridSize.x / 2f, gridSize.y / 2f, -10f);
        transform.position = center;
        float average = (gridSize.x + gridSize.y) / 2f;
        
        _maxZoom = average * maxZoomScale;
        _zoomSpeed = _maxZoom * zoomSpeedScale;
        _moveSpeed = _maxZoom * moveSpeedScale;
        _camera.orthographicSize = _maxZoom;
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        Vector3 move = new Vector3(h, v, 0f) * (_moveSpeed * Time.deltaTime);
        transform.position += move;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll == 0) return;

        _camera.orthographicSize -= scroll * _zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _minZoom, _maxZoom);
        _moveSpeed = moveSpeedScale * _camera.orthographicSize;
    }
}
