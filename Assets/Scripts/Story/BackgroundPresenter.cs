using UnityEngine;
using UnityEngine.UI;

public class BackgroundPresenter : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;


    public void ShowBackground(Background background)
    {
        if (background == null || background.Image == null)
        {
            return;
        }

        backgroundImage.sprite = background.Image;
        backgroundImage.gameObject.SetActive(true);
    }


    public void Clear()
    {
        backgroundImage.sprite = null;
        backgroundImage.gameObject.SetActive(false);
    }
}