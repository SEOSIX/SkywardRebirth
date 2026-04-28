using UnityEngine;

[CreateAssetMenu(fileName = "Input", menuName = "Scriptable Objects/Input")]
public class InputData : ScriptableObject
{
    public Sprite buttonSprite;
    public string tooltipText;
    public bool ui;
}