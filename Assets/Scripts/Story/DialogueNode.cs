using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNode
{
    [SerializeField] private string nodeId;

    [SerializeField] private Character speaker;

    [TextArea(3, 8)]
    [SerializeField] private string text;

    [SerializeField] private StoryDestination next;

    [SerializeField]
    private List<DialogueChoice> choices =
        new List<DialogueChoice>();

    public string NodeId => nodeId;
    public Character Speaker => speaker;
    public string Text => text;

    public StoryDestination Next => next;

    public IReadOnlyList<DialogueChoice> Choices => choices;

    public bool HasChoices => choices != null && choices.Count > 0;
}