using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Collectable : MonoBehaviour
{
    public int PointValue => _pointValue;
    public CollectablesEnum CollectableID => _collectableID;
    
    /// <summary>
    /// buffer to prevent overlap with other collectable if being spawned next to eachother
    /// </summary>
    public float RadiusBuffer => _collider.radius + 0.1f;
    
    [SerializeField] private int _pointValue = 10;
    [SerializeField] private AudioClip _collectSound; //only need clip, no need for individual audio source
    [SerializeField] private CollectablesEnum _collectableID; //only really used for pool identification, plenty of other ways to do this
    
    //could also do this with a UnityEvent as it can be used just like delegate events
    // public UnityEvent<Collectable, bool> OnDisabled;
    public delegate void CollectableDisabled(Collectable collectable, bool collected);
    public event CollectableDisabled OnDisabled;
    
    
    private float _killYBound; //y threshold to disable, calculated based off camera bounds
    private static int playerLayer; //for caching the layer number
    protected CircleCollider2D _collider;
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
        //simple way to determine when to kill a missed collectable, fires when outside screen bounds (works for any resolution)
        if(transform.position.y < _killYBound - _collider.radius * 2)
        {
            Disable(false);
        }
    }
    
    /// <summary>
    /// Called when the collectable is disabled, either by being collected or missed, preps for recycling in the pool
    /// </summary>
    /// <param name="collected"></param>
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
        gameObject.SetActive(false);
        OnDisabled?.Invoke(this, collected);
    }
}
