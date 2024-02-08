using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Collectable : MonoBehaviour
{
    public int PointValue => _pointValue;
    public CollectablesEnum CollectableID => _collectableID;
    
    [SerializeField] private int _pointValue = 10;
    [SerializeField] private AudioClip _collectSound;
    [SerializeField] private CollectablesEnum _collectableID;
    
    //could also do this with a UnityEvent as it can be used just like delegate events
    // public UnityEvent<Collectable, bool> OnDisabled;
    public delegate void CollectableDisabled(Collectable collectable, bool collected);
    public event CollectableDisabled OnDisabled;
    
    private float _killYBound; //y threshold to disable, calculated based off camera bounds
    private static int playerLayer; //for caching the layer number
    private CircleCollider2D _collider;
    private Rigidbody2D _rigidbody;
    
    private void Awake()
    {
        Camera cam = Camera.main;
        Vector3 bounds = cam.ScreenToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
        _killYBound = bounds.y;
        Debug.Log(_killYBound);
        
        playerLayer = LayerMask.NameToLayer("Player");
        _collider = GetComponent<CircleCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            Disable(true);
        }
    }

    private void FixedUpdate()
    {
        if(transform.position.y < _killYBound - _collider.radius * 2)
        {
            Disable(false);
        }
    }

    public void Disable(bool collected)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        
        if (collected)
        {
            AudioSource.PlayClipAtPoint(_collectSound, Vector3.zero); //not necessary to have positional audio
        }
        _rigidbody.velocity = Vector2.zero;
        OnDisabled?.Invoke(this, collected);
        gameObject.SetActive(false);
    }
    
}
