using UnityEngine;

public class DialoguePresenter : MonoBehaviour
{
    [Header("Presentation")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private BackgroundPresenter backgroundPresenter;
    [SerializeField] private CharacterPresenter characterPresenter;
    [SerializeField] private AudioPresenter audioPresenter;


    private StoryRuntime runtime;


    private void Awake()
    {
        dialogueUI.ContinuePressed += OnContinuePressed;
        dialogueUI.ChoiceSelected += OnChoiceSelected;
    }


    private void OnDestroy()
    {
        dialogueUI.ContinuePressed -= OnContinuePressed;
        dialogueUI.ChoiceSelected -= OnChoiceSelected;

        UnbindRuntime();
    }


    public void Bind(StoryRuntime storyRuntime)
    {
        UnbindRuntime();

        runtime = storyRuntime;

        if (runtime == null)
        {
            dialogueUI.Clear();
            backgroundPresenter.Clear();
            return;
        }

        runtime.NodeChanged += OnNodeChanged;
        runtime.StoryStopped += OnStoryStopped;

        if (runtime.IsRunning)
        {
            OnNodeChanged(runtime.CurrentNode);
        }
    }


    private void UnbindRuntime()
    {
        if (runtime == null)
        {
            return;
        }

        runtime.NodeChanged -= OnNodeChanged;
        runtime.StoryStopped -= OnStoryStopped;

        runtime = null;
    }


    private void OnNodeChanged(DialogueNode node)
    {
        if (node == null)
        {
            return;
        }

        dialogueUI.ShowNode(node);

        if (node.Background != null)
        {
            backgroundPresenter.ShowBackground(node.Background);
        }

        if (node.HideLeftCharacter)
        {
            characterPresenter.HideLeft();
        }

        if (node.HideRightCharacter)
        {
            characterPresenter.HideRight();
        }

        if (node.Speaker != null)
        {
            characterPresenter.ShowCharacter(
                node.Speaker,
                node.SpeakerSide
            );
        }

        if (runtime != null)
        {
            audioPresenter.UpdateMusic(runtime.CurrentScene);
        }

        audioPresenter.PlaySoundEffect(node.SoundEffect);
    }


    private void OnStoryStopped()
    {
        dialogueUI.Clear();
        backgroundPresenter.Clear();
        audioPresenter.StopAll();
    }

    private void OnContinuePressed()
    {
        if (runtime == null)
        {
            return;
        }

        runtime.Advance();
    }


    private void OnChoiceSelected(int choiceIndex)
    {
        if (runtime == null)
        {
            return;
        }

        runtime.Choose(choiceIndex);
    }
}