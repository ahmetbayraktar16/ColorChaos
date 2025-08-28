using UnityEngine;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    [Header("Cube Properties")]
    public float fallSpeed = 2f;
    public bool isGoldenCube = false;
    
    [Header("Visual Effects")]
    public ParticleSystem destroyEffect;
    public Material goldenMaterial;
    public ParticleSystem correctClickEffect;
    public ParticleSystem wrongClickEffect;
    
    [Header("Touch Detection")]
    public float touchRadius = 1.5f; // Increased for better touch detection
    
    private GameManager gameManager;
    private Color cubeColor;
    private bool isInitialized = false;
    private Renderer cubeRenderer;
    private Camera mainCamera;
    
    public Color CubeColor => cubeColor;
    
    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer == null)
        {
            cubeRenderer = GetComponentInChildren<Renderer>();
        }
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        
        if (transform.position.y < -6f)
        {
            if (gameManager != null)
            {
                gameManager.OnCubeMissed(this);
            }
            Destroy(gameObject);
            return;
        }
        
        // Check for mouse input (PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckMouseClick();
        }
        
        // Check for touch input (Mobile)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckTouchInput();
        }
    }
    
    void CheckMouseClick()
    {
        if (mainCamera == null) return;
        
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
        
        float distance = Vector3.Distance(transform.position, worldPosition);
        Debug.Log($"Mouse click at {mousePosition}, world pos: {worldPosition}, cube pos: {transform.position}, distance: {distance}, touchRadius: {touchRadius}");
        
        if (distance < touchRadius)
        {
            Debug.Log("Cube clicked via mouse!");
            OnCubeClicked();
        }
    }
    
    void CheckTouchInput()
    {
        if (mainCamera == null) return;
        
        Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 10f));
        
        float distance = Vector3.Distance(transform.position, worldPosition);
        Debug.Log($"Touch detected at {touchPosition}, world pos: {worldPosition}, cube pos: {transform.position}, distance: {distance}, touchRadius: {touchRadius}");
        
        if (distance < touchRadius)
        {
            Debug.Log("Cube clicked via touch!");
            OnCubeClicked();
        }
    }
    
    void OnCubeClicked()
    {
        if (gameManager != null)
        {
            gameManager.OnCubeClicked(this);
        }
    }
    
    // Legacy mouse input for compatibility
    void OnMouseDown()
    {
        OnCubeClicked();
    }
    
    public void Initialize(GameManager manager, Color color)
    {
        gameManager = manager;
        cubeColor = color;
        isInitialized = true;
        isGoldenCube = false;
        
        SetupRenderer();
    }
    
    public void InitializeAsGolden(GameManager manager, Color color)
    {
        gameManager = manager;
        cubeColor = color;
        isInitialized = true;
        isGoldenCube = true;
        
        SetupRenderer();
        MakeGoldenCube();
    }
    
    void SetupRenderer()
    {
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
    }
    
    void MakeGoldenCube()
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        if (cubeRenderer != null && goldenMaterial != null)
        {
            cubeRenderer.material = goldenMaterial;
        }
        else if (cubeRenderer != null)
        {
            cubeRenderer.material.color = new Color(1f, 0.8f, 0f, 1f);
            
            if (cubeRenderer.material.HasProperty("_EmissionColor"))
            {
                cubeRenderer.material.EnableKeyword("_EMISSION");
                cubeRenderer.material.SetColor("_EmissionColor", new Color(1f, 0.8f, 0f, 1f) * 0.5f);
            }
        }
    }
    
    public void SetFallSpeed(float newSpeed)
    {
        fallSpeed = newSpeed;
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
    
    public void PlayEffectCorrect()
    {
        SpawnEffect(correctClickEffect, cubeColor);
    }
    
    public void PlayEffectWrong()
    {
        SpawnEffect(wrongClickEffect, Color.red);
    }
    
    void SpawnEffect(ParticleSystem effectPrefab, Color tint)
    {
        if (effectPrefab == null) return;
        
        Vector3 spawnPos = transform.position;
        
        spawnPos.x = Mathf.Clamp(spawnPos.x, -4f, 4f);
        spawnPos.y = Mathf.Clamp(spawnPos.y, -4f, 4f);
        
        var effect = Instantiate(effectPrefab, spawnPos, Quaternion.identity);
        
        effect.transform.localScale = Vector3.one * 0.5f;
        
        var main = effect.main;
        main.startColor = tint;
        main.startSize = 0.3f;
        
        effect.Play();
        Destroy(effect.gameObject, 2f);
    }
    
    void SpawnEffectAt(ParticleSystem effectPrefab, Vector3 position, Color tint)
    {
        if (effectPrefab == null) return;
        
        position.x = Mathf.Clamp(position.x, -4f, 4f);
        position.y = Mathf.Clamp(position.y, -4f, 4f);
        
        var effect = Instantiate(effectPrefab, position, Quaternion.identity);
        
        effect.transform.localScale = Vector3.one * 0.5f;
        
        var main = effect.main;
        main.startColor = tint;
        main.startSize = 0.3f;
        
        effect.Play();
        Destroy(effect.gameObject, 2f);
    }
    
    public void PlayDestroyEffect()
    {
        if (destroyEffect != null)
        {
            Vector3 spawnPos = transform.position;
            
            spawnPos.x = Mathf.Clamp(spawnPos.x, -4f, 4f);
            spawnPos.y = Mathf.Clamp(spawnPos.y, -4f, 4f);
            
            ParticleSystem effect = Instantiate(destroyEffect, spawnPos, Quaternion.identity);
            
            effect.transform.localScale = Vector3.one * 0.5f;
            
            var main = effect.main;
            main.startSize = 0.3f;
            
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }
    }

    public Color GetCubeColor()
    {
        return cubeColor;
    }
}