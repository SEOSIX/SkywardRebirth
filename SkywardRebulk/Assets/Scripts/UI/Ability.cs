using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CooldownButton : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject cooldownImageObject;
    [SerializeField] private float cooldownDuration = 3f;

    private Button _button;
    private bool _isOnCooldown = false;

    private void Awake()
    {
        _button = GetComponent<Button>();
        
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_isOnCooldown) return;

        _isOnCooldown = true;
        _button.interactable = false;

        if (GameUIManager.Instance != null && cooldownImageObject != null)
        {
            GameUIManager.Instance.StartCooldownUI(cooldownImageObject, cooldownDuration, () =>
            {
                _isOnCooldown = false;
                _button.interactable = true; 
            });
        }
    }
}