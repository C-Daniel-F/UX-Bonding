using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "VN/Character")]
public class Character : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string characterId;
    [SerializeField] private string displayName;

    [Header("Visual")]
    [SerializeField] private Sprite sprite;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
    public Sprite Sprite => sprite;
}