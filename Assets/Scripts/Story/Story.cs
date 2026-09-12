using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStory", menuName = "VN/Story")]
public class Story : ScriptableObject
{
    [Header("Story")]
    [SerializeField] private string storyId;
    [SerializeField] private string storyName;

    [Header("Content")]
    [SerializeField] private List<StoryScene> scenes = new List<StoryScene>();

    [Header("Characters")]
    [SerializeField] private List<Character> characters = new List<Character>();

    [Header("Backgrounds")]
    [SerializeField] private List<Background> backgrounds = new List<Background>();

    public string StoryId => storyId;
    public string StoryName => storyName;

    public IReadOnlyList<StoryScene> Scenes => scenes;
    public IReadOnlyList<Character> Characters => characters;
    public IReadOnlyList<Background> Backgrounds => backgrounds;
}