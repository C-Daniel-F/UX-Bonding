using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Typing")]
    [SerializeField, Min(1f)] private float charactersPerSecond = 40f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    public event Action ContinuePressed;
    public event Action<int> ChoiceSelected;

    private DialogueNode currentNode;
    private Coroutine typingCoroutine;

    private int currentPage;
    private int pageCount = 1;

    private bool isTyping;
    private bool textFinished;

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (dialogueText != null)
        {
            dialogueText.enableWordWrapping = true;
            dialogueText.overflowMode = TextOverflowModes.Page;
        }

        if (choicesContainer != null)
            choicesContainer.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        StopTyping();
    }

    public void ShowNode(DialogueNode node)
    {
        if (node == null)
        {
            Clear();
            return;
        }

        StopTyping();
        ClearChoices();

        currentNode = node;
        currentPage = 0;
        isTyping = false;
        textFinished = false;

        ShowSpeaker(node);
        PrepareDialogue(node.Text ?? string.Empty);
        ShowCurrentPage();
    }

    private void ShowSpeaker(DialogueNode node)
    {
        if (speakerText == null)
            return;

        if (node.Speaker == null)
        {
            speakerText.text = string.Empty;
            speakerText.gameObject.SetActive(false);
            return;
        }

        speakerText.gameObject.SetActive(true);
        speakerText.text = node.Speaker.DisplayName;
    }

    private void PrepareDialogue(string text)
    {
        dialogueText.overflowMode = TextOverflowModes.Page;
        dialogueText.enableWordWrapping = true;
        dialogueText.pageToDisplay = 1;
        dialogueText.maxVisibleCharacters = int.MaxValue;
        dialogueText.text = text;

        Canvas.ForceUpdateCanvases();
        dialogueText.ForceMeshUpdate(true, true);

        pageCount = Mathf.Max(1, dialogueText.textInfo.pageCount);
    }

    private void ShowCurrentPage()
    {
        if (dialogueText == null)
            return;

        currentPage = Mathf.Clamp(currentPage, 0, pageCount - 1);

        StopTyping();
        ClearChoices();

        isTyping = false;
        textFinished = false;

        dialogueText.overflowMode = TextOverflowModes.Page;
        dialogueText.pageToDisplay = currentPage + 1;
        dialogueText.maxVisibleCharacters = int.MaxValue;
        dialogueText.ForceMeshUpdate(true, true);

        int firstCharacter = GetCurrentPageFirstCharacter();
        dialogueText.maxVisibleCharacters = firstCharacter;

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        typingCoroutine = StartCoroutine(TypeCurrentPage());
    }

    private IEnumerator TypeCurrentPage()
    {
        isTyping = true;
        textFinished = false;

        dialogueText.ForceMeshUpdate(true, true);

        TMP_TextInfo info = dialogueText.textInfo;

        if (info.characterCount == 0 ||
            currentPage >= info.pageInfo.Length)
        {
            typingCoroutine = null;
            FinishTyping(false);
            yield break;
        }

        TMP_PageInfo page = info.pageInfo[currentPage];

        int first = Mathf.Max(0, page.firstCharacterIndex);
        int last = page.lastCharacterIndex;

        if (last < first)
        {
            typingCoroutine = null;
            FinishTyping(false);
            yield break;
        }

        float delay = 1f / Mathf.Max(1f, charactersPerSecond);

        dialogueText.maxVisibleCharacters = first;

        for (int i = first; i <= last; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;

            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return new WaitForSeconds(delay);
        }

        typingCoroutine = null;
        FinishTyping(false);
    }

    private int GetCurrentPageFirstCharacter()
    {
        TMP_TextInfo info = dialogueText.textInfo;

        if (currentPage < 0 || currentPage >= info.pageInfo.Length)
            return 0;

        return Mathf.Max(
            0,
            info.pageInfo[currentPage].firstCharacterIndex
        );
    }

    private void FinishTyping(bool stopCoroutine = true)
    {
        if (stopCoroutine && typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = null;

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
            dialogueText.ForceMeshUpdate();
        }

        isTyping = false;
        textFinished = true;

        bool isLastPage = currentPage >= pageCount - 1;

        if (currentNode != null &&
            currentNode.HasChoices &&
            isLastPage)
        {
            ShowChoices(currentNode);

            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
        }
        else if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    private void OnContinueClicked()
    {
        if (currentNode == null)
            return;

        if (isTyping)
        {
            FinishTyping();
            return;
        }

        if (!textFinished)
            return;

        if (currentPage < pageCount - 1)
        {
            currentPage++;
            ShowCurrentPage();
            return;
        }

        ContinuePressed?.Invoke();
    }

    private void ShowChoices(DialogueNode node)
    {
        ClearChoices();

        if (choicesContainer == null ||
            choiceButtonPrefab == null ||
            node.Choices == null)
        {
            return;
        }

        choicesContainer.gameObject.SetActive(true);

        for (int i = 0; i < node.Choices.Count; i++)
        {
            int index = i;
            DialogueChoice choice = node.Choices[i];

            Button button = Instantiate(
                choiceButtonPrefab,
                choicesContainer
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = choice.Text;

            button.onClick.AddListener(
                () => ChoiceSelected?.Invoke(index)
            );
        }
    }

    private void ClearChoices()
    {
        if (choicesContainer == null)
            return;

        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);

        choicesContainer.gameObject.SetActive(false);
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    public void Clear()
    {
        StopTyping();

        currentNode = null;
        currentPage = 0;
        pageCount = 1;
        textFinished = false;

        if (speakerText != null)
        {
            speakerText.text = string.Empty;
            speakerText.gameObject.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.pageToDisplay = 1;
        }

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        ClearChoices();
    }
}