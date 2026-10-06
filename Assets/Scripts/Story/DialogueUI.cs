using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Typing")]
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Pagination")]
    [SerializeField] private int minimumWordsOnFinalPage = 3;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    public event Action ContinuePressed;
    public event Action<int> ChoiceSelected;

    private DialogueNode currentNode;

    private readonly List<string> pages = new List<string>();

    private int currentPage;

    private Coroutine typingCoroutine;

    private bool isTyping;
    private bool textFinished;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDestroy()
    {
        continueButton.onClick.RemoveListener(OnContinueClicked);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
    }

    public void ShowNode(DialogueNode node)
    {
        if (node == null)
        {
            Clear();
            return;
        }

        currentNode = node;
        currentPage = 0;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        textFinished = false;

        ClearChoices();

        ShowSpeaker(node);

        BuildPages(node.Text);

        ShowCurrentPage();
    }

    private void ShowSpeaker(DialogueNode node)
    {
        if (node.Speaker == null)
        {
            speakerText.gameObject.SetActive(false);
            speakerText.text = string.Empty;
            return;
        }

        speakerText.gameObject.SetActive(true);
        speakerText.text = node.Speaker.DisplayName;
    }

    private void BuildPages(string text)
    {
        pages.Clear();

        if (string.IsNullOrWhiteSpace(text))
        {
            pages.Add(string.Empty);
            return;
        }

        List<string> words = new List<string>(
            text.Split(
                new[] { ' ', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries
            )
        );

        int startIndex = 0;

        while (startIndex < words.Count)
        {
            int remainingWords =
                words.Count - startIndex;

            if (remainingWords <= minimumWordsOnFinalPage)
            {
                pages.Add(
                    JoinWords(
                        words,
                        startIndex,
                        words.Count
                    )
                );

                break;
            }

            int maxFitCount =
                FindMaximumFittingWords(
                    words,
                    startIndex,
                    remainingWords
                );

            if (maxFitCount <= 0)
            {
                maxFitCount = 1;
            }

            int wordsAfterPage =
                remainingWords - maxFitCount;

            if (wordsAfterPage > 0 &&
                wordsAfterPage < minimumWordsOnFinalPage)
            {
                int balancedCount =
                    Mathf.CeilToInt(
                        remainingWords / 2f
                    );

                balancedCount =
                    Mathf.Clamp(
                        balancedCount,
                        1,
                        maxFitCount
                    );

                while (balancedCount > 1 &&
                       !FitsOnPage(
                           JoinWords(
                               words,
                               startIndex,
                               startIndex + balancedCount
                           )
                       ))
                {
                    balancedCount--;
                }

                maxFitCount = balancedCount;
            }

            pages.Add(
                JoinWords(
                    words,
                    startIndex,
                    startIndex + maxFitCount
                )
            );

            startIndex += maxFitCount;
        }

        if (pages.Count == 0)
        {
            pages.Add(text.Trim());
        }
    }

    private int FindMaximumFittingWords(
        List<string> words,
        int startIndex,
        int count)
    {
        int low = 1;
        int high = count;
        int best = 0;

        while (low <= high)
        {
            int middle =
                (low + high) / 2;

            string candidate =
                JoinWords(
                    words,
                    startIndex,
                    startIndex + middle
                );

            if (FitsOnPage(candidate))
            {
                best = middle;
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return best;
    }

    private bool FitsOnPage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        dialogueText.overflowMode =
            TextOverflowModes.Page;

        dialogueText.pageToDisplay = 1;
        dialogueText.text = text;

        dialogueText.ForceMeshUpdate();

        return dialogueText.textInfo.pageCount <= 1;
    }

    private string JoinWords(
        List<string> words,
        int startIndex,
        int endIndex)
    {
        int count =
            endIndex - startIndex;

        return string.Join(
            " ",
            words.GetRange(
                startIndex,
                count
            )
        );
    }

    private void ShowCurrentPage()
    {
        if (pages.Count == 0)
        {
            return;
        }

        currentPage =
            Mathf.Clamp(
                currentPage,
                0,
                pages.Count - 1
            );

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        textFinished = false;

        dialogueText.overflowMode =
            TextOverflowModes.Overflow;

        dialogueText.text =
            pages[currentPage];

        dialogueText.maxVisibleCharacters = 0;

        dialogueText.ForceMeshUpdate();

        continueButton.gameObject.SetActive(true);

        typingCoroutine =
            StartCoroutine(TypeDialogue());
    }

    private IEnumerator TypeDialogue()
    {
        isTyping = true;
        textFinished = false;

        int characterCount =
            dialogueText.textInfo.characterCount;

        if (characterCount == 0)
        {
            FinishTyping();
            yield break;
        }

        float delay =
            1f /
            Mathf.Max(
                1f,
                charactersPerSecond
            );

        for (int i = 1;
             i <= characterCount;
             i++)
        {
            dialogueText.maxVisibleCharacters = i;

            if (useUnscaledTime)
            {
                yield return new WaitForSecondsRealtime(
                    delay
                );
            }
            else
            {
                yield return new WaitForSeconds(
                    delay
                );
            }
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.maxVisibleCharacters =
            dialogueText.textInfo.characterCount;

        isTyping = false;
        textFinished = true;

        bool isLastPage =
            currentPage >= pages.Count - 1;

        if (currentNode != null &&
            currentNode.HasChoices &&
            isLastPage)
        {
            ShowChoices(currentNode);
            continueButton.gameObject.SetActive(false);
        }
        else
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    private void OnContinueClicked()
    {
        if (currentNode == null)
        {
            return;
        }

        if (isTyping)
        {
            FinishTyping();
            return;
        }

        if (!textFinished)
        {
            return;
        }

        if (currentPage < pages.Count - 1)
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

        choicesContainer.gameObject.SetActive(true);

        for (int i = 0;
             i < node.Choices.Count;
             i++)
        {
            int choiceIndex = i;

            DialogueChoice choice =
                node.Choices[i];

            Button button =
                Instantiate(
                    choiceButtonPrefab,
                    choicesContainer
                );

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
        if (choicesContainer == null)
        {
            return;
        }

        for (int i =
                 choicesContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                choicesContainer.GetChild(i).gameObject
            );
        }

        choicesContainer.gameObject.SetActive(false);
    }

    private void OnChoiceClicked(int choiceIndex)
    {
        ChoiceSelected?.Invoke(choiceIndex);
    }

    public void Clear()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentNode = null;
        currentPage = 0;

        pages.Clear();

        isTyping = false;
        textFinished = false;

        speakerText.text = string.Empty;
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;

        speakerText.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        ClearChoices();
    }
}