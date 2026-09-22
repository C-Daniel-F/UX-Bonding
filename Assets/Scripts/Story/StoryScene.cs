using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryScene
{
    [SerializeField] private string sceneId;
    [SerializeField] private string sceneName;

    [Header("Audio")]
    [SerializeField] private AudioClip music;

    [SerializeField]
    private List<DialogueNode> dialogueNodes =
        new List<DialogueNode>();

    public string SceneId => sceneId;
    public string SceneName => sceneName;

    public AudioClip Music => music;

    public IReadOnlyList<DialogueNode> DialogueNodes => dialogueNodes;
}