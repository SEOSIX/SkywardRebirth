using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    
    [Header("Inventory")]
    [SerializeField] private GameObject inventoryPrefab;
    [SerializeField] private Transform inventorySpawnParent;
    
    [Header("Health UI")]
    [SerializeField] private int _maxHealth;
    [SerializeField] private float _currentHealth;
    
    private bool _inventoryOpen = false;
    private GameObject _currentInventoryInstance;
    private bool _hasBow = false; 
    
    private HealthManager _healthManager;
    private PlayerInput inputs;
    private Player player;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }
        
        _healthManager = GetComponent<HealthManager>();
    }

    void Start()
    {
        player = Player.instance; 
        inputs = player.GetComponent<PlayerInput>();

        _healthManager.Init(_maxHealth);
        _currentHealth = _maxHealth;
        
        _healthManager.UpdateHealth(_currentHealth);
    }
    
    public void TakeDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        
        _healthManager.UpdateHealth(_currentHealth);
        
    }
    
    public void StartCooldownUI(GameObject imageObject, float cooldownTime, Action onCooldownComplete = null)
    {
        Image cooldownImage = imageObject.GetComponent<Image>();

        if (cooldownImage != null && cooldownImage.type == Image.Type.Filled)
        {
            StartCoroutine(CooldownCoroutine(cooldownImage, cooldownTime, onCooldownComplete));
        }
        else
        {
        }
    }
    
    
    private IEnumerator CooldownCoroutine(Image cooldownImage, float cooldownTime, Action onCooldownComplete)
    {
        cooldownImage.fillAmount = 1f; 
        
        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            
            cooldownImage.fillAmount = 1f - (elapsedTime / cooldownTime);
            
            yield return null; 
        }

        cooldownImage.fillAmount = 0f; 
        onCooldownComplete?.Invoke(); 
    }
    
    

    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        _inventoryOpen = !_inventoryOpen;

        if (_inventoryOpen)
        {
            if (Player.instance != null && Player.instance.playerContoller != null)
            {
                Player.instance.playerContoller.enabled = false;
            }
            
            Transform parent = inventorySpawnParent != null ? inventorySpawnParent : transform;
            _currentInventoryInstance = Instantiate(inventoryPrefab, parent);

            Inventory invScript = _currentInventoryInstance.GetComponent<Inventory>();
            if (invScript != null && invScript.BowImage != null)
            {
                invScript.BowImage.SetActive(_hasBow); 
            }
        }
        else
        {
            if (Player.instance != null && Player.instance.playerContoller != null)
            {
                Player.instance.playerContoller.enabled = true;
            }
            
            if (_currentInventoryInstance != null)
            {
                Destroy(_currentInventoryInstance);
            }
        }
    }

    public void AddObjectToInventory(string itemName)
    {
        if (itemName == "Bow")
        {
            _hasBow = true;
            
            if (_inventoryOpen && _currentInventoryInstance != null)
            {
                Inventory invScript = _currentInventoryInstance.GetComponent<Inventory>();
                if (invScript != null && invScript.BowImage != null)
                {
                    invScript.BowImage.SetActive(true);
                }
            }
        }
    }
}