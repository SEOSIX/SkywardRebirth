using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Transform container;

    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite threeQuarterHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite quarterHeart;
    [SerializeField] private Sprite emptyHeart;

    private Image[] _heartVisuals;

    public void Init(int maxHealth)
    {
        _heartVisuals = new Image[maxHealth];

        for (int i = 0; i < maxHealth; i++)
        {
            Image parentHeart = Instantiate(heartPrefab, container);
            
            _heartVisuals[i] = parentHeart.transform.GetChild(0).GetComponent<Image>();
        }
    }

    public void UpdateHealth(float health)
    {
        for (int i = 0; i < _heartVisuals.Length; i++)
        {
            float heartValue = Mathf.Clamp(health - i, 0f, 1f);
            
            _heartVisuals[i].sprite = heartValue switch
            {
                >= 1f => fullHeart,
                >= 0.75f => threeQuarterHeart,
                >= 0.5f => halfHeart,
                >= 0.25f => quarterHeart,
                _ => emptyHeart
            };
        }
    }
}