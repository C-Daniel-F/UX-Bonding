using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryScene
{
    [SerializeField] private string sceneId;
    [SerializeField] private string sceneName;

    [SerializeField]
    private List<DialogueNode> dialogueNodes =
        new List<DialogueNode>();

    public string SceneId => sceneId;
    public string SceneName => sceneName;

    public IReadOnlyList<DialogueNode> DialogueNodes => dialogueNodes;
}