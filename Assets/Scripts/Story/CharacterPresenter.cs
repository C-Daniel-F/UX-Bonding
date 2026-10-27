
using UnityEngine;
using UnityEngine.UI;

public class CharacterPresenter : MonoBehaviour
{
    [Header("Character Slots")]
    [SerializeField] private Image leftCharacter;
    [SerializeField] private Image rightCharacter;

    [Header("Speaker Highlight")]
    [SerializeField] private Color speakingColor = Color.white;
    [SerializeField]
    private Color listeningColor =
        new Color(0.65f, 0.65f, 0.65f, 1f);

    [SerializeField, Min(0f)]
    private float fadeDuration = 0.15f;

    private Character leftCharacterData;
    private Character rightCharacterData;
    private Character activeSpeaker;

    private void Start()
    {
        HideLeft();
        HideRight();
    }

    public void ShowCharacter(
        Character character,
        CharacterSide side)
    {
        if (character == null || character.Sprite == null)
            return;

        Image target = GetImage(side);

        if (side == CharacterSide.Left)
            leftCharacterData = character;
        else
            rightCharacterData = character;

        target.sprite = character.Sprite;
        target.preserveAspect = true;
        target.gameObject.SetActive(true);

        UpdateHighlight(target, character);
    }

    public void SetSpeakingCharacter(Character character)
    {
        activeSpeaker = character;

        if (leftCharacterData != null && leftCharacter != null)
            UpdateHighlight(leftCharacter, leftCharacterData);

        if (rightCharacterData != null && rightCharacter != null)
            UpdateHighlight(rightCharacter, rightCharacterData);
    }

    private void UpdateHighlight(
        Image image,
        Character character)
    {
        // When nobody is speaking, keep all visible characters bright.
        bool isSpeaking =
            activeSpeaker == null || character == activeSpeaker;

        Color targetColor = isSpeaking
            ? speakingColor
            : listeningColor;

        image.CrossFadeColor(
            targetColor,
            fadeDuration,
            true,
            true
        );
    }

    public void HideLeft()
    {
        leftCharacterData = null;

        if (leftCharacter == null)
            return;

        leftCharacter.CrossFadeColor(
            speakingColor, 0f, true, true
        );

        leftCharacter.sprite = null;
        leftCharacter.gameObject.SetActive(false);
    }

    public void HideRight()
    {
        rightCharacterData = null;

        if (rightCharacter == null)
            return;

        rightCharacter.CrossFadeColor(
            speakingColor, 0f, true, true
        );

        rightCharacter.sprite = null;
        rightCharacter.gameObject.SetActive(false);
    }

    private Image GetImage(CharacterSide side)
    {
        return side == CharacterSide.Left
            ? leftCharacter
            : rightCharacter;
    }
}
