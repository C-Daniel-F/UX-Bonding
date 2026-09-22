using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StoryDestination))]
public class StoryDestinationDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight * 2f) + Spacing;
    }

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        SerializedProperty sceneId =
            property.FindPropertyRelative("sceneId");

        SerializedProperty nodeId =
            property.FindPropertyRelative("nodeId");

        SerializedProperty scenes =
            property.serializedObject.FindProperty("scenes");

        if (scenes == null)
        {
            EditorGUI.HelpBox(
                position,
                "StoryDestination must be used inside a Story asset.",
                MessageType.Warning
            );

            return;
        }

        Rect sceneRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight
        );

        Rect nodeRect = new Rect(
            position.x,
            position.y + EditorGUIUtility.singleLineHeight + Spacing,
            position.width,
            EditorGUIUtility.singleLineHeight
        );

        // =========================
        // SCENE
        // =========================

        List<string> sceneOptions = new List<string>
        {
            "<None>"
        };

        List<string> sceneIds = new List<string>
        {
            string.Empty
        };

        int selectedSceneIndex = 0;

        for (int i = 0; i < scenes.arraySize; i++)
        {
            SerializedProperty scene =
                scenes.GetArrayElementAtIndex(i);

            SerializedProperty currentSceneId =
                scene.FindPropertyRelative("sceneId");

            SerializedProperty currentSceneName =
                scene.FindPropertyRelative("sceneName");

            string id = currentSceneId.stringValue;
            string sceneName = currentSceneName.stringValue;

            string displayName = string.IsNullOrWhiteSpace(sceneName)
                ? id
                : $"{sceneName} ({id})";

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "(Unnamed Scene)";
            }

            sceneOptions.Add(displayName);
            sceneIds.Add(id);

            if (sceneId.stringValue == id && !string.IsNullOrWhiteSpace(id))
            {
                selectedSceneIndex = i + 1;
            }
        }

        EditorGUI.BeginChangeCheck();

        int newSceneIndex = EditorGUI.Popup(
            sceneRect,
            "Scene",
            selectedSceneIndex,
            sceneOptions.ToArray()
        );

        if (EditorGUI.EndChangeCheck())
        {
            sceneId.stringValue = sceneIds[newSceneIndex];

            // Changing the scene invalidates the old node.
            nodeId.stringValue = string.Empty;
        }

        // =========================
        // NODE
        // =========================

        if (newSceneIndex == 0)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.Popup(
                    nodeRect,
                    "Node",
                    0,
                    new[] { "<Select a scene first>" }
                );
            }

            return;
        }

        int sceneArrayIndex = newSceneIndex - 1;

        SerializedProperty selectedScene =
            scenes.GetArrayElementAtIndex(sceneArrayIndex);

        SerializedProperty nodes =
            selectedScene.FindPropertyRelative("dialogueNodes");

        if (nodes == null || nodes.arraySize == 0)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.Popup(
                    nodeRect,
                    "Node",
                    0,
                    new[] { "<No dialogue nodes>" }
                );
            }

            return;
        }

        List<string> nodeOptions = new List<string>
        {
            "<None>"
        };

        List<string> nodeIds = new List<string>
        {
            string.Empty
        };

        int selectedNodeIndex = 0;

        for (int i = 0; i < nodes.arraySize; i++)
        {
            SerializedProperty node =
                nodes.GetArrayElementAtIndex(i);

            SerializedProperty currentNodeId =
                node.FindPropertyRelative("nodeId");

            SerializedProperty currentText =
                node.FindPropertyRelative("text");

            string id = currentNodeId.stringValue;
            string text = currentText.stringValue;

            string preview = text
                .Replace("\n", " ")
                .Trim();

            if (preview.Length > 45)
            {
                preview = preview.Substring(0, 45) + "...";
            }

            string displayName;

            if (string.IsNullOrWhiteSpace(id))
            {
                displayName = string.IsNullOrWhiteSpace(preview)
                    ? "(Unnamed Node)"
                    : $"(Unnamed Node) — {preview}";
            }
            else
            {
                displayName = string.IsNullOrWhiteSpace(preview)
                    ? id
                    : $"{id} — {preview}";
            }

            nodeOptions.Add(displayName);
            nodeIds.Add(id);

            if (nodeId.stringValue == id && !string.IsNullOrWhiteSpace(id))
            {
                selectedNodeIndex = i + 1;
            }
        }

        int newNodeIndex = EditorGUI.Popup(
            nodeRect,
            "Node",
            selectedNodeIndex,
            nodeOptions.ToArray()
        );

        nodeId.stringValue = nodeIds[newNodeIndex];
    }
}