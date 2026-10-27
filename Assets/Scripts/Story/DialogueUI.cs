using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Font")]
    [SerializeField] private TMP_FontAsset pixelifySansFont;

    [Header("Typing")]
    [SerializeField, Min(1f)] private float charactersPerSecond = 40f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Punctuation Pauses")]
    [SerializeField, Min(0f)] private float commaPause = 0.12f;
    [SerializeField, Min(0f)] private float sentencePause = 0.25f;
    [SerializeField, Min(0f)] private float ellipsisPause = 0.5f;
    [SerializeField, Min(0f)] private float newlinePause = 0.2f;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    public event Action ContinuePressed;
    public event Action<int> ChoiceSelected;

    private readonly List<Button> choiceButtons = new List<Button>();
    private int selectedChoiceIndex;

    private DialogueNode currentNode;
    private Coroutine typingCoroutine;

    private int currentPage;
    private int pageCount = 1;

    private bool isTyping;
    private bool textFinished;

    private void Awake()
    {
        ApplyFont(speakerText);
        ApplyFont(dialogueText);

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


    private void Update()
    {
        if (currentNode == null)
            return;

        bool choicesVisible =
            choicesContainer != null &&
            choicesContainer.gameObject.activeInHierarchy &&
            choiceButtons.Count > 0;

        if (choicesVisible)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SelectChoice(selectedChoiceIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) ||
                     Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectChoice(selectedChoiceIndex + 1);
            }

            if ((EventSystem.current == null ||
                 EventSystem.current.currentInputModule == null) &&
                (Input.GetKeyDown(KeyCode.Return) ||
                 Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                choiceButtons[selectedChoiceIndex].onClick.Invoke();
            }

            return;
        }

        if ((EventSystem.current == null ||
             EventSystem.current.currentInputModule == null) &&
            (Input.GetKeyDown(KeyCode.Return) ||
             Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            OnContinueClicked();
        }
    }

    private void SelectChoice(int index)
    {
        if (choiceButtons.Count == 0)
            return;

        selectedChoiceIndex =
            (index % choiceButtons.Count + choiceButtons.Count)
            % choiceButtons.Count;

        Button selectedButton = choiceButtons[selectedChoiceIndex];

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                selectedButton.gameObject
            );
        }
    }


    private void ApplyFont(TMP_Text textComponent)
    {
        if (textComponent != null && pixelifySansFont != null)
            textComponent.font = pixelifySansFont;
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

        if (currentNode == null ||
            !currentNode.HasChoices ||
            pageCount < 2)
        {
            return;
        }

        int originalPageCount = pageCount;
        int previousPageIndex = originalPageCount - 2;

        List<int> wordStarts = GetWordStartsOnPage(
            dialogueText.textInfo,
            previousPageIndex
        );

        // Try moving three or more words onto the final page.
        for (int wordsToMove = 3;
             wordsToMove <= wordStarts.Count;
             wordsToMove++)
        {
            int insertIndex =
                wordStarts[wordStarts.Count - wordsToMove];

            if (insertIndex < 0 || insertIndex > text.Length)
                continue;

            dialogueText.text = text.Insert(insertIndex, "<page>");

            Canvas.ForceUpdateCanvases();
            dialogueText.ForceMeshUpdate(true, true);

            int candidatePageCount =
                Mathf.Max(1, dialogueText.textInfo.pageCount);

            // Never accept a solution that adds another page.
            if (candidatePageCount > originalPageCount)
                continue;

            int finalPageWords = CountWordsOnPage(
                dialogueText.textInfo,
                candidatePageCount - 1
            );

            if (finalPageWords >= 4)
            {
                pageCount = candidatePageCount;
                return;
            }
        }

        // No suitable adjustment found: restore the original text.
        dialogueText.text = text;

        Canvas.ForceUpdateCanvases();
        dialogueText.ForceMeshUpdate(true, true);

        pageCount = Mathf.Max(1, dialogueText.textInfo.pageCount);
    }

    private List<int> GetWordStartsOnPage(
        TMP_TextInfo info,
        int targetPage)
    {
        List<int> wordStarts = new List<int>();
        bool insideWord = false;

        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo character = info.characterInfo[i];

            if (character.pageNumber != targetPage)
            {
                insideWord = false;
                continue;
            }

            if (char.IsWhiteSpace(character.character))
            {
                insideWord = false;
            }
            else if (!insideWord)
            {
                wordStarts.Add(character.index);
                insideWord = true;
            }
        }

        return wordStarts;
    }

    private int CountWordsOnPage(
        TMP_TextInfo info,
        int targetPage)
    {
        int count = 0;
        bool insideWord = false;

        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo character = info.characterInfo[i];

            if (character.pageNumber != targetPage)
            {
                insideWord = false;
                continue;
            }

            if (char.IsWhiteSpace(character.character))
            {
                insideWord = false;
            }
            else if (!insideWord)
            {
                count++;
                insideWord = true;
            }
        }

        return count;
    }


    private int CountWordsInPage(TMP_TextInfo info, TMP_PageInfo page)
    {
        int start = Mathf.Max(0, page.firstCharacterIndex);
        int end = Mathf.Min(page.lastCharacterIndex, info.characterCount - 1);

        int words = 0;
        bool insideWord = false;

        for (int i = start; i <= end; i++)
        {
            char c = info.characterInfo[i].character;

            if (char.IsWhiteSpace(c))
            {
                insideWord = false;
            }
            else if (!insideWord)
            {
                words++;
                insideWord = true;
            }
        }

        return words;
    }

    private int GetWordStartStringIndex(
        TMP_TextInfo info,
        TMP_PageInfo page,
        int wordsFromEnd)
    {
        int start = Mathf.Max(0, page.firstCharacterIndex);
        int end = Mathf.Min(page.lastCharacterIndex, info.characterCount - 1);

        List<int> wordStarts = new List<int>();
        bool insideWord = false;

        for (int i = start; i <= end; i++)
        {
            char c = info.characterInfo[i].character;

            if (char.IsWhiteSpace(c))
            {
                insideWord = false;
            }
            else if (!insideWord)
            {
                wordStarts.Add(info.characterInfo[i].index);
                insideWord = true;
            }
        }

        if (wordStarts.Count < wordsFromEnd)
            return -1;

        return wordStarts[wordStarts.Count - wordsFromEnd];
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
        {
            continueButton.gameObject.SetActive(true);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(
                    continueButton.gameObject
                );
            }
        }

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

            char currentChar = info.characterInfo[i].character;
            float pause = 0f;

            if (currentChar == ',')
            {
                pause = commaPause;
            }
            else if (currentChar == '.' &&
                     i + 2 < info.characterCount &&
                     info.characterInfo[i + 1].character == '.' &&
                     info.characterInfo[i + 2].character == '.')
            {
                pause = ellipsisPause;
            }
            else if (currentChar == '.' &&
         i > 0 &&
         info.characterInfo[i - 1].character == '.')
            {
                pause = 0f;
            }
            else if (currentChar == '.' ||
                     currentChar == '!' ||
                     currentChar == '?' )
            {
                pause = sentencePause;
            }
            else if (currentChar == '\n')
            {
                pause = newlinePause;
            }

            float waitTime = delay + pause;

            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(waitTime);
            else
                yield return new WaitForSeconds(waitTime);
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

            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            choiceButtons.Add(button);

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                ApplyFont(buttonText);
                buttonText.text = choice.Text;
            }

            button.onClick.AddListener(
                () => ChoiceSelected?.Invoke(index)
            );
        }

        if (choiceButtons.Count > 0)
            SelectChoice(0);
    }

    private void ClearChoices()
    {
        if (choicesContainer == null)
            return;

        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);

        choiceButtons.Clear();
        selectedChoiceIndex = 0;

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
