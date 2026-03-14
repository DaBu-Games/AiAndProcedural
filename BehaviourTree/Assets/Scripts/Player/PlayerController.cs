using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    
    private Camera _camera;
    private Rigidbody _rb;
    private Vector2 _moveInput;
    private Vector3 _velocity;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _camera = Camera.main;
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
        _velocity = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * moveSpeed;
    }
    
    void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            
            Vector3 worldPosition = _camera.ScreenToWorldPoint(new Vector3(
                mouseScreenPosition.x, 
                mouseScreenPosition.y, 
                -_camera.transform.position.z
            ));
            
            transform.LookAt(new Vector3(worldPosition.x, transform.position.y, worldPosition.z));
        }
    }
    
    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
    }
}
