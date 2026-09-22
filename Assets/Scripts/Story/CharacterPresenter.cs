using UnityEngine;
using UnityEngine.UI;

public class CharacterPresenter : MonoBehaviour
{
    [Header("Character Slots")]
    [SerializeField] private Image leftCharacter;
    [SerializeField] private Image rightCharacter;


    private void Start()
    {
        HideLeft();
        HideRight();
    }


    // =========================
    // SHOW CHARACTER
    // =========================

    public void ShowCharacter(
        Character character,
        CharacterSide side)
    {
        if (character == null || character.Sprite == null)
        {
            return;
        }

        Image target = GetImage(side);

        target.sprite = character.Sprite;
        target.gameObject.SetActive(true);
        target.preserveAspect = true;
    }


    // =========================
    // HIDE
    // =========================

    public void HideLeft()
    {
        leftCharacter.sprite = null;
        leftCharacter.gameObject.SetActive(false);
    }


    public void HideRight()
    {
        rightCharacter.sprite = null;
        rightCharacter.gameObject.SetActive(false);
    }


    // =========================
    // HELPERS
    // =========================

    private Image GetImage(CharacterSide side)
    {
        return side == CharacterSide.Left
            ? leftCharacter
            : rightCharacter;
    }
}