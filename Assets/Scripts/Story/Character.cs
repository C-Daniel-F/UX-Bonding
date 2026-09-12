using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "VN/Character")]
public class Character : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string characterId;
    [SerializeField] private string displayName;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
}