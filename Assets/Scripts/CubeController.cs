using UnityEngine;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    [Header("Cube Properties")]
    public float fallSpeed = 2f;
    public float rotationSpeed = 50f;
    public bool isGoldenCube = false;
    
    [Header("Visual Effects")]
    public ParticleSystem destroyEffect;
    public Material goldenMaterial;
    
    private GameManager gameManager;
    private Color cubeColor;
    private bool isInitialized = false;
    private bool isRotating = false;
    private Renderer cubeRenderer;
    
    public Color CubeColor => cubeColor;
    
    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer == null)
        {
            cubeRenderer = GetComponentInChildren<Renderer>();
        }
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Camera.main == null) return;
            
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
            
            if (Vector3.Distance(transform.position, worldPosition) < 1f)
            {
                OnMouseDown();
            }
        }
        
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (Camera.main == null) return;
            
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 10f));
            
            if (Vector3.Distance(transform.position, worldPosition) < 1f)
            {
                OnMouseDown();
            }
        }
    }
    
    public void Initialize(GameManager manager, Color color)
    {
        gameManager = manager;
        cubeColor = color;
        isInitialized = true;
        
        if (cubeRenderer == null)
        {
            cubeRenderer = GetComponent<Renderer>();
            if (cubeRenderer == null)
            {
                cubeRenderer = GetComponentInChildren<Renderer>();
            }
        }
        
        if (cubeRenderer != null)
        {
            if (cubeRenderer.material == null)
            {
                cubeRenderer.material = new Material(Shader.Find("Standard"));
            }
            
            cubeRenderer.material.color = cubeColor;
        }
        else
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube");
            }
            
            cubeRenderer = meshRenderer;
            if (cubeRenderer.material == null)
            {
                cubeRenderer.material = new Material(Shader.Find("Standard"));
            }
            cubeRenderer.material.color = cubeColor;
        }
        
        if (Random.Range(0f, 1f) < 0.05f)
        {
            MakeGoldenCube();
        }
        
        if (Random.Range(0f, 1f) < 0.1f)
        {
            isRotating = true;
        }
    }
    
    void MakeGoldenCube()
    {
        isGoldenCube = true;
        if (cubeRenderer != null && goldenMaterial != null)
        {
            cubeRenderer.material = goldenMaterial;
        }
        else if (cubeRenderer != null)
        {
            cubeRenderer.material.color = Color.yellow;
        }
    }
    
    void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.OnCubeClicked(this);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TouchArea"))
        {
            if (gameManager != null)
            {
                gameManager.OnCubeClicked(this);
            }
        }
    }
    
    public Color GetCubeColor()
    {
        return cubeColor;
    }
    
    void OnDestroy()
    {
        if (destroyEffect != null)
        {
            ParticleSystem effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }
    }
}