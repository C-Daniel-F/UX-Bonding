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

    [SerializeField]
    private SceneMusicAction musicAction =
    SceneMusicAction.KeepPrevious;

    [SerializeField] private AudioClip sceneSoundEffect;

    public SceneMusicAction MusicAction => musicAction;
    public AudioClip SceneSoundEffect => sceneSoundEffect;

    public string SceneId => sceneId;
    public string SceneName => sceneName;

    public AudioClip Music => music;

    public IReadOnlyList<DialogueNode> DialogueNodes => dialogueNodes;
}

public enum SceneMusicAction
{
    KeepPrevious,
    ChangeMusic,    
    StopMusic
}