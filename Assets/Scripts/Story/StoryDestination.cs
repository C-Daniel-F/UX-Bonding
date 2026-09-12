using System;
using UnityEngine;

[Serializable]
public class StoryDestination
{
    [SerializeField] private string sceneId;
    [SerializeField] private string nodeId;

    public string SceneId => sceneId;
    public string NodeId => nodeId;

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(sceneId) &&
        !string.IsNullOrWhiteSpace(nodeId);
}