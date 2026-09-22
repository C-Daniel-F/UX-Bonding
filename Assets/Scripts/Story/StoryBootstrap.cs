using UnityEngine;

public class StoryBootstrap : MonoBehaviour
{
    [Header("Story")]
    [SerializeField] private Story story;

    [Header("Presentation")]
    [SerializeField] private DialoguePresenter dialoguePresenter;


    private StoryRuntime runtime;


    private void Start()
    {
        runtime = new StoryRuntime();

        dialoguePresenter.Bind(runtime);

        runtime.Start(story);
    }
}