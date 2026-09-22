using UnityEngine;

[CreateAssetMenu(fileName = "NewBackground", menuName = "VN/Background")]
public class Background : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string backgroundId;
    [SerializeField] private string displayName;

    [Header("Visual")]
    [SerializeField] private Sprite image;

    public string BackgroundId => backgroundId;
    public string DisplayName => displayName;
    public Sprite Image => image;
}