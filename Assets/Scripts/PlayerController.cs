using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 17f;
    private float _xBounds = 24f;

    private Vector2 _moveInput;
    private Rigidbody2D _rigidbody;
    private readonly string _moveActionName = "Move";
    

    private void Awake()
    {
        //get bounds based off camera view
        Camera cam = Camera.main;
        Vector3 bounds = cam.ScreenToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        _rigidbody = GetComponent<Rigidbody2D>();
        
        _xBounds = Mathf.Abs(bounds.x) - collider.size.x / 2f;
        playerInput.onActionTriggered += OnActionTriggered;
        
        _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation & RigidbodyConstraints2D.FreezePositionY;
    }
    
   

    private void OnDisable()
    {
        if (TryGetComponent(out PlayerInput playerInput))
        {
            playerInput.onActionTriggered -= OnActionTriggered;
        }
    }

    /// <summary>
    /// Unity Input System callback, using to get the move input for arrow keys and A/S (gamepad should also work)
    /// </summary>
    /// <param name="context"></param>
    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.name.Equals(_moveActionName))
        {
            _moveInput = context.ReadValue<Vector2>();
        }
    }

    /// <summary>
    /// Movement handling
    /// </summary>
    private void FixedUpdate()
    {
        Vector2 movement = new Vector2(_moveInput.x, 0) * _moveSpeed * Time.fixedDeltaTime;
        float xPos = _rigidbody.position.x + movement.x;
        if((_moveInput.x > 0 && xPos > _xBounds) || (_moveInput.x < 0 && xPos < -_xBounds))
        {
            return;
        }
        _rigidbody.MovePosition(_rigidbody.position + movement);
    }
}