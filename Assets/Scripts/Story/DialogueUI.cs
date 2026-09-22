using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;


    public event Action ContinuePressed;
    public event Action<int> ChoiceSelected;


    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
    }


    public void ShowNode(DialogueNode node)
    {
        if (node == null)
        {
            Clear();
            return;
        }

        ShowSpeaker(node);
        ShowDialogue(node);
        ShowChoices(node);
    }


    // =========================
    // SPEAKER
    // =========================

    private void ShowSpeaker(DialogueNode node)
    {
        if (node.Speaker == null)
        {
            speakerText.gameObject.SetActive(false);
            return;
        }

        speakerText.gameObject.SetActive(true);
        speakerText.text = node.Speaker.DisplayName;
    }



    private void ShowDialogue(DialogueNode node)
    {
        dialogueText.text = node.Text;
    }


    private void ShowChoices(DialogueNode node)
    {
        ClearChoices();

        if (!node.HasChoices)
        {
            continueButton.gameObject.SetActive(true);
            choicesContainer.gameObject.SetActive(false);
            return;
        }

        continueButton.gameObject.SetActive(false);
        choicesContainer.gameObject.SetActive(true);

        for (int i = 0; i < node.Choices.Count; i++)
        {
            int choiceIndex = i;

            DialogueChoice choice = node.Choices[i];

            Button button =
                Instantiate(choiceButtonPrefab, choicesContainer);

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text = choice.Text;
            }

            button.onClick.AddListener(
                () => OnChoiceClicked(choiceIndex)
            );
        }
    }


    private void ClearChoices()
    {
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(choicesContainer.GetChild(i).gameObject);
        }
    }



    private void OnContinueClicked()
    {
        ContinuePressed?.Invoke();
    }


    private void OnChoiceClicked(int choiceIndex)
    {
        ChoiceSelected?.Invoke(choiceIndex);
    }

    public void Clear()
    {
        speakerText.text = string.Empty;
        dialogueText.text = string.Empty;

        speakerText.gameObject.SetActive(false);

        continueButton.gameObject.SetActive(false);

        ClearChoices();
        choicesContainer.gameObject.SetActive(false);
    }
}