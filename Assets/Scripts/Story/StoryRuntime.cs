using System;

public class StoryRuntime
{
    private Story story;

    private StoryScene currentScene;
    private DialogueNode currentNode;


    // =========================
    // EVENTS
    // =========================

    public event Action<DialogueNode> NodeChanged;
    public event Action StoryStopped;


    // =========================
    // CURRENT STATE
    // =========================

    public Story Story => story;
    public StoryScene CurrentScene => currentScene;
    public DialogueNode CurrentNode => currentNode;

    public bool IsRunning =>
        story != null &&
        currentScene != null &&
        currentNode != null;

    public bool HasChoices =>
        IsRunning && currentNode.HasChoices;


    // =========================
    // START
    // =========================

    public void Start(Story story)
    {
        if (story == null)
        {
            throw new ArgumentNullException(nameof(story));
        }

        if (story.Scenes == null || story.Scenes.Count == 0)
        {
            throw new InvalidOperationException(
                "The story does not contain any scenes."
            );
        }

        this.story = story;

        currentScene = story.Scenes[0];

        if (currentScene.DialogueNodes == null ||
            currentScene.DialogueNodes.Count == 0)
        {
            throw new InvalidOperationException(
                "The starting scene does not contain any dialogue nodes."
            );
        }

        currentNode = currentScene.DialogueNodes[0];

        NotifyNodeChanged();
    }


    // =========================
    // ADVANCE
    // =========================

    public void Advance()
    {
        if (!IsRunning)
        {
            return;
        }

        if (currentNode.HasChoices)
        {
            return;
        }

        MoveTo(currentNode.Next);
    }


    // =========================
    // CHOICES
    // =========================

    public void Choose(int choiceIndex)
    {
        if (!IsRunning)
        {
            return;
        }

        if (!currentNode.HasChoices)
        {
            return;
        }

        if (choiceIndex < 0 ||
            choiceIndex >= currentNode.Choices.Count)
        {
            return;
        }

        DialogueChoice choice =
            currentNode.Choices[choiceIndex];

        MoveTo(choice.Destination);
    }


    // =========================
    // NAVIGATION
    // =========================

    private void MoveTo(StoryDestination destination)
    {
        if (destination == null || !destination.IsValid)
        {
            Stop();
            return;
        }

        StoryScene destinationScene =
            FindScene(destination.SceneId);

        if (destinationScene == null)
        {
            Stop();
            return;
        }

        DialogueNode destinationNode =
            FindNode(destinationScene, destination.NodeId);

        if (destinationNode == null)
        {
            Stop();
            return;
        }

        currentScene = destinationScene;
        currentNode = destinationNode;

        NotifyNodeChanged();
    }


    private StoryScene FindScene(string sceneId)
    {
        if (story == null || story.Scenes == null)
        {
            return null;
        }

        foreach (StoryScene scene in story.Scenes)
        {
            if (scene.SceneId == sceneId)
            {
                return scene;
            }
        }

        return null;
    }


    private DialogueNode FindNode(
        StoryScene scene,
        string nodeId)
    {
        if (scene == null ||
            scene.DialogueNodes == null)
        {
            return null;
        }

        foreach (DialogueNode node in scene.DialogueNodes)
        {
            if (node.NodeId == nodeId)
            {
                return node;
            }
        }

        return null;
    }


    // =========================
    // NOTIFICATION
    // =========================

    private void NotifyNodeChanged()
    {
        NodeChanged?.Invoke(currentNode);
    }


    // =========================
    // STOP
    // =========================

    public void Stop()
    {
        currentScene = null;
        currentNode = null;

        StoryStopped?.Invoke();
    }
}