using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNode
{
    [SerializeField] private string nodeId;

    [SerializeField] private Character speaker;

    [SerializeField] private Background background;

    [Header("Audio")]
    [SerializeField] private AudioClip soundEffect;

    public AudioClip SoundEffect => soundEffect;

    [TextArea(3, 8)]
    [SerializeField] private string text;

    [SerializeField] private StoryDestination next;

    [SerializeField] private List<DialogueChoice> choices =
        new List<DialogueChoice>();

    [SerializeField]
    private CharacterSide speakerSide =
    CharacterSide.Left;

    [SerializeField] private bool hideLeftCharacter;
    [SerializeField] private bool hideRightCharacter;

    public string NodeId => nodeId;
    public Character Speaker => speaker;
    public Background Background => background;
    public string Text => text;

    public StoryDestination Next => next;

    public IReadOnlyList<DialogueChoice> Choices => choices;

    public bool HasChoices => choices != null && choices.Count > 0;

    public CharacterSide SpeakerSide => speakerSide;

    public bool HideLeftCharacter => hideLeftCharacter;
    public bool HideRightCharacter => hideRightCharacter;
}