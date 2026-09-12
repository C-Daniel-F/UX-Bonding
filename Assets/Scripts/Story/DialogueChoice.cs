using System;
using UnityEngine;

[Serializable]
public class DialogueChoice
{
    [SerializeField] private string choiceId;

    [TextArea(2, 4)]
    [SerializeField] private string text;

    [SerializeField] private StoryDestination destination;

    public string ChoiceId => choiceId;
    public string Text => text;
    public StoryDestination Destination => destination;
}