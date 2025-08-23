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
    
    private GameManager gameManager;
    private Color cubeColor;
    private bool isInitialized = false;
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
        
        if (transform.position.y < -6f)
        {
            if (gameManager != null)
            {
                gameManager.OnCubeMissed(this);
            }
            Destroy(gameObject);
            return;
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
        
        // Ensure effect spawns within screen bounds
        spawnPos.x = Mathf.Clamp(spawnPos.x, -4f, 4f);
        spawnPos.y = Mathf.Clamp(spawnPos.y, -4f, 4f);
        
        var effect = Instantiate(effectPrefab, spawnPos, Quaternion.identity);
        
        // Scale down the effect to match cube size
        effect.transform.localScale = Vector3.one * 0.5f;
        
        var main = effect.main;
        main.startColor = tint;
        main.startSize = 0.3f; // Reduce particle size
        
        effect.Play();
        Destroy(effect.gameObject, 2f);
    }
    
    void SpawnEffectAt(ParticleSystem effectPrefab, Vector3 position, Color tint)
    {
        if (effectPrefab == null) return;
        
        // Ensure effect spawns within screen bounds
        position.x = Mathf.Clamp(position.x, -4f, 4f);
        position.y = Mathf.Clamp(position.y, -4f, 4f);
        
        var effect = Instantiate(effectPrefab, position, Quaternion.identity);
        
        // Scale down the effect to match cube size
        effect.transform.localScale = Vector3.one * 0.5f;
        
        var main = effect.main;
        main.startColor = tint;
        main.startSize = 0.3f; // Reduce particle size
        
        effect.Play();
        Destroy(effect.gameObject, 2f);
    }
    
    public void PlayDestroyEffect()
    {
        if (destroyEffect != null)
        {
            Vector3 spawnPos = transform.position;
            
            // Ensure effect spawns within screen bounds
            spawnPos.x = Mathf.Clamp(spawnPos.x, -4f, 4f);
            spawnPos.y = Mathf.Clamp(spawnPos.y, -4f, 4f);
            
            ParticleSystem effect = Instantiate(destroyEffect, spawnPos, Quaternion.identity);
            
            // Scale down the effect to match cube size
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