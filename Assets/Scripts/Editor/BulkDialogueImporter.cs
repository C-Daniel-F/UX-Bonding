#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BulkDialogueImporter : EditorWindow
{
    [SerializeField] private Story targetStory;
    [SerializeField] private TextAsset dialogueFile;

    [MenuItem("Tools/UX Bonding/Bulk Dialogue Importer")]
    private static void OpenWindow()
    {
        GetWindow<BulkDialogueImporter>("Bulk Dialogue Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Bulk Dialogue Importer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Imports a pipe-separated text file into your existing Story. StoryScene, DialogueNode, DialogueChoice, and StoryDestination are written as inline serialized data; no node assets are created.",
            MessageType.Info);

        targetStory = (Story)EditorGUILayout.ObjectField("Target Story", targetStory, typeof(Story), false);
        dialogueFile = (TextAsset)EditorGUILayout.ObjectField("Dialogue Text File", dialogueFile, typeof(TextAsset), false);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Format", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "SCENE|sceneId|Scene Name\n" +
            "NODE|nodeId|Speaker DisplayName or -|Left/Right/Center/None|Dialogue text|nextNodeId or sceneId/nodeId or END\n" +
            "CHOICE|sourceNodeId|choiceId|Choice text|destinationNodeId or sceneId/nodeId\n\n" +
            "Use - for narration. Use \\n inside text for a line break. Escape a literal pipe as \\|. Choice records belong to the most recently declared scene.",
            MessageType.None);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(targetStory == null || dialogueFile == null))
        {
            if (GUILayout.Button("Import Dialogue", GUILayout.Height(32)))
                ImportDialogue();
        }
    }

    private void ImportDialogue()
    {
        bool storyUpdated = false;

        try
        {
            if (targetStory == null || dialogueFile == null)
                throw new InvalidOperationException("Assign a Story asset and a dialogue text file first.");

            ParseFile(dialogueFile.text, out List<SceneData> scenes, out List<PendingChoice> pendingChoices);
            AttachChoices(scenes, pendingChoices);

            Dictionary<string, Character> characters = LoadCharacters();
            GetExistingStoryData(targetStory, out HashSet<string> existingSceneIds, out HashSet<string> existingNodeKeys);
            ValidateData(scenes, characters, existingSceneIds, existingNodeKeys);

            // All writes happen through one SerializedObject and are applied only after
            // every scene/node/choice has been populated successfully.
            AppendScenesToStory(targetStory, scenes, characters);
            storyUpdated = true;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Dialogue Import Complete",
                $"Imported {scenes.Count} scene(s) and {scenes.Sum(s => s.Nodes.Count)} dialogue node(s) into '{targetStory.name}'.",
                "Great");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "Dialogue Import Failed",
                exception.Message + (storyUpdated
                    ? "\n\nThe Story asset was modified. Inspect it before retrying."
                    : "\n\nThe import did not complete. Check the error and try again."),
                "OK");
        }
    }

    private static void ParseFile(string source, out List<SceneData> scenes, out List<PendingChoice> pendingChoices)
    {
        scenes = new List<SceneData>();
        pendingChoices = new List<PendingChoice>();
        SceneData currentScene = null;
        HashSet<string> sceneIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string[] lines = (source ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                continue;

            List<string> fields = SplitFields(line);
            string record = fields[0].Trim().ToUpperInvariant();

            if (record == "SCENE")
            {
                RequireFields(fields, 3, i + 1);
                string id = fields[1].Trim();
                if (string.IsNullOrWhiteSpace(id) || id.Contains("/"))
                    throw new FormatException($"Line {i + 1}: sceneId must be non-empty and cannot contain '/'.");
                if (!sceneIds.Add(id))
                    throw new FormatException($"Line {i + 1}: duplicate sceneId '{id}' in import file.");

                currentScene = new SceneData { Id = id, Name = DecodeText(fields[2].Trim()) };
                scenes.Add(currentScene);
            }
            else if (record == "NODE")
            {
                if (currentScene == null)
                    throw new FormatException($"Line {i + 1}: add a SCENE record before NODE records.");
                RequireFields(fields, 6, i + 1);

                string id = fields[1].Trim();
                if (string.IsNullOrWhiteSpace(id) || id.Contains("/"))
                    throw new FormatException($"Line {i + 1}: nodeId must be non-empty and cannot contain '/'.");
                if (currentScene.Nodes.Any(n => Same(n.Id, id)))
                    throw new FormatException($"Line {i + 1}: duplicate nodeId '{id}' in scene '{currentScene.Id}'.");

                string speaker = fields[2].Trim();
                string side = fields[3].Trim();
                if (side == "-") side = "None";
                if (!Enum.TryParse(side, true, out CharacterSide parsedSide) ||
                    !Enum.IsDefined(typeof(CharacterSide), parsedSide))
                    throw new FormatException($"Line {i + 1}: unknown character side '{side}'. Use Left, Right, Center, or None.");

                currentScene.Nodes.Add(new NodeData
                {
                    Id = id,
                    SpeakerName = speaker,
                    SideName = parsedSide.ToString(),
                    Text = DecodeText(fields[4]),
                    Next = ParseDestination(fields[5].Trim(), currentScene.Id, true),
                    Line = i + 1
                });
            }
            else if (record == "CHOICE")
            {
                if (currentScene == null)
                    throw new FormatException($"Line {i + 1}: add a SCENE record before CHOICE records.");
                RequireFields(fields, 5, i + 1);
                pendingChoices.Add(new PendingChoice
                {
                    SceneId = currentScene.Id,
                    SourceNodeId = fields[1].Trim(),
                    ChoiceId = fields[2].Trim(),
                    Text = DecodeText(fields[3]),
                    Destination = ParseDestination(fields[4].Trim(), currentScene.Id, false),
                    Line = i + 1
                });
            }
            else
            {
                throw new FormatException($"Line {i + 1}: unknown record '{fields[0]}'. Use SCENE, NODE, CHOICE, or a comment starting with #.");
            }
        }

        if (scenes.Count == 0)
            throw new FormatException("The file contains no SCENE records.");
    }

    private static void AttachChoices(List<SceneData> scenes, List<PendingChoice> pending)
    {
        foreach (PendingChoice p in pending)
        {
            SceneData scene = scenes.FirstOrDefault(s => Same(s.Id, p.SceneId));
            NodeData node = scene?.Nodes.FirstOrDefault(n => Same(n.Id, p.SourceNodeId));
            if (node == null)
                throw new FormatException($"Line {p.Line}: choice source node '{p.SourceNodeId}' was not found in scene '{p.SceneId}'.");
            if (string.IsNullOrWhiteSpace(p.ChoiceId))
                throw new FormatException($"Line {p.Line}: choiceId cannot be empty.");
            if (string.IsNullOrWhiteSpace(p.Text))
                throw new FormatException($"Line {p.Line}: choice text cannot be empty.");
            if (p.Destination.IsEmpty)
                throw new FormatException($"Line {p.Line}: choices must point to a node; use a terminal node for the end of a branch.");
            if (node.Choices.Any(c => Same(c.Id, p.ChoiceId)))
                throw new FormatException($"Line {p.Line}: duplicate choiceId '{p.ChoiceId}' on node '{node.Id}'.");

            node.Choices.Add(new ChoiceData { Id = p.ChoiceId, Text = p.Text, Destination = p.Destination, Line = p.Line });
        }
    }

    private static void GetExistingStoryData(Story story, out HashSet<string> sceneIds, out HashSet<string> nodeKeys)
    {
        sceneIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        nodeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        SerializedObject so = new SerializedObject(story);
        SerializedProperty list = FindProperty(so, "scenes", "Scenes");
        if (list == null || !list.isArray)
            throw new InvalidOperationException("Story must contain a serialized List<StoryScene> field named 'scenes'.");

        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty scene = list.GetArrayElementAtIndex(i);
            if (scene.propertyType == SerializedPropertyType.ObjectReference)
            {
                UnityEngine.Object sceneObject = scene.objectReferenceValue;
                if (sceneObject == null) continue;
                SerializedObject sceneSo = new SerializedObject(sceneObject);
                string sceneId = GetString(sceneSo, "sceneId", "id");
                if (string.IsNullOrWhiteSpace(sceneId)) continue;
                sceneIds.Add(sceneId);
                SerializedProperty nodes = FindProperty(sceneSo, "dialogueNodes", "nodes");
                AddExistingNodeKeys(nodes, sceneId, nodeKeys);
            }
            else
            {
                string sceneId = GetStringRelative(scene, "sceneId", "id");
                if (string.IsNullOrWhiteSpace(sceneId)) continue;
                sceneIds.Add(sceneId);
                SerializedProperty nodes = FindRelative(scene, "dialogueNodes", "nodes");
                AddExistingNodeKeys(nodes, sceneId, nodeKeys);
            }
        }
    }

    private static void AddExistingNodeKeys(SerializedProperty nodes, string sceneId, HashSet<string> keys)
    {
        if (nodes == null || !nodes.isArray) return;
        for (int i = 0; i < nodes.arraySize; i++)
        {
            SerializedProperty item = nodes.GetArrayElementAtIndex(i);
            string id = string.Empty;
            if (item.propertyType == SerializedPropertyType.ObjectReference)
            {
                UnityEngine.Object obj = item.objectReferenceValue;
                if (obj != null)
                {
                    SerializedObject nodeSo = new SerializedObject(obj);
                    id = GetString(nodeSo, "nodeId", "id");
                    if (string.IsNullOrWhiteSpace(id)) id = obj.name;
                }
            }
            else
            {
                id = GetStringRelative(item, "nodeId", "id");
            }
            if (!string.IsNullOrWhiteSpace(id)) keys.Add(Key(sceneId, id));
        }
    }

    private static void ValidateData(
        List<SceneData> scenes,
        Dictionary<string, Character> characters,
        HashSet<string> existingSceneIds,
        HashSet<string> existingNodeKeys)
    {
        HashSet<string> allSceneIds = new HashSet<string>(existingSceneIds, StringComparer.OrdinalIgnoreCase);
        HashSet<string> allNodeKeys = new HashSet<string>(existingNodeKeys, StringComparer.OrdinalIgnoreCase);

        foreach (SceneData scene in scenes)
        {
            if (!allSceneIds.Add(scene.Id))
                throw new FormatException($"Story already contains a scene with ID '{scene.Id}'. Use a new sceneId.");
            if (scene.Nodes.Count == 0)
                throw new FormatException($"Scene '{scene.Id}' has no NODE records.");
            foreach (NodeData node in scene.Nodes)
            {
                if (!allNodeKeys.Add(Key(scene.Id, node.Id)))
                    throw new FormatException($"Duplicate node ID '{scene.Id}/{node.Id}'.");
                if (!string.IsNullOrWhiteSpace(node.SpeakerName) && node.SpeakerName != "-" &&
                    !characters.ContainsKey(node.SpeakerName))
                    throw new FormatException($"Line {node.Line}: speaker '{node.SpeakerName}' was not found among Character assets. Use its exact DisplayName or '-' for narration.");
            }
        }

        foreach (SceneData scene in scenes)
        {
            foreach (NodeData node in scene.Nodes)
            {
                ValidateDestination(node.Next, allSceneIds, allNodeKeys, $"next destination of {scene.Id}/{node.Id}");
                foreach (ChoiceData choice in node.Choices)
                    ValidateDestination(choice.Destination, allSceneIds, allNodeKeys, $"destination of choice '{choice.Id}' on {scene.Id}/{node.Id}");
            }
        }
    }

    private static void ValidateDestination(DestinationData d, HashSet<string> sceneIds, HashSet<string> nodeKeys, string context)
    {
        if (d.IsEmpty) return;
        if (!sceneIds.Contains(d.SceneId))
            throw new FormatException($"{context} points to unknown scene '{d.SceneId}'. Add that SCENE to the Story first or fix the destination.");
        if (!nodeKeys.Contains(Key(d.SceneId, d.NodeId)))
            throw new FormatException($"{context} points to missing node '{d.SceneId}/{d.NodeId}'.");
    }

    private static Dictionary<string, Character> LoadCharacters()
    {
        var result = new Dictionary<string, Character>(StringComparer.OrdinalIgnoreCase);
        foreach (string guid in AssetDatabase.FindAssets("t:Character"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Character character = AssetDatabase.LoadAssetAtPath<Character>(path);
            if (character == null || string.IsNullOrWhiteSpace(character.DisplayName)) continue;
            if (!result.ContainsKey(character.DisplayName)) result.Add(character.DisplayName, character);
        }
        return result;
    }

    private static void PopulateNode(
        SerializedProperty node,
        NodeData data,
        Dictionary<string, Character> characters)
    {
        if (node.propertyType != SerializedPropertyType.Generic)
            throw new InvalidOperationException(
                $"Node '{data.Id}' is not an inline serializable DialogueNode. Check StoryScene.dialogueNodes.");

        SetStringRelative(node, data.Id, "nodeId", "id");
        SetStringRelative(node, data.Text, "text", "dialogueText");

        SerializedProperty speaker = FindRelative(node, "characterSpeaker", "speaker");
        if (speaker == null || speaker.propertyType != SerializedPropertyType.ObjectReference)
            throw new InvalidOperationException(
                $"Node '{data.Id}': expected a Character asset reference field named 'characterSpeaker' or 'speaker'.");

        speaker.objectReferenceValue = data.SpeakerName == "-" || string.IsNullOrWhiteSpace(data.SpeakerName)
            ? null
            : characters[data.SpeakerName];

        SerializedProperty side = FindRelative(node, "characterSpeakerSide", "speakerSide", "side");
        if (side == null || side.propertyType != SerializedPropertyType.Enum)
            throw new InvalidOperationException(
                $"Node '{data.Id}': could not find a serialized CharacterSide enum field.");

        int enumIndex = Array.IndexOf(side.enumNames, data.SideName);
        if (enumIndex < 0)
            throw new InvalidOperationException(
                $"Node '{data.Id}': CharacterSide enum does not contain '{data.SideName}'.");
        side.enumValueIndex = enumIndex;

        SerializedProperty next = FindRelative(node, "next", "nextDestination", "destination");
        if (next == null)
            throw new InvalidOperationException(
                $"Node '{data.Id}': could not find a serialized StoryDestination field named 'next'.");
        SetDestination(next, data.Next);

        SerializedProperty choices = FindRelative(node, "choices", "Choices");
        if (choices == null || !choices.isArray)
        {
            if (data.Choices.Count > 0)
                throw new InvalidOperationException(
                    $"Node '{data.Id}' has choices but no serialized List<DialogueChoice> field named 'choices'.");
            return;
        }

        choices.arraySize = data.Choices.Count;
        for (int i = 0; i < data.Choices.Count; i++)
        {
            SerializedProperty choice = choices.GetArrayElementAtIndex(i);
            ChoiceData choiceData = data.Choices[i];

            if (choice.propertyType != SerializedPropertyType.Generic)
                throw new InvalidOperationException(
                    $"Choice '{choiceData.Id}' is not an inline serializable DialogueChoice. Check DialogueNode.choices.");

            SetStringRelative(choice, choiceData.Id, "choiceId", "id");
            SetStringRelative(choice, choiceData.Text, "text", "choiceText");

            SerializedProperty destination = FindRelative(choice, "destination", "target");
            if (destination == null)
                throw new InvalidOperationException(
                    $"Choice '{choiceData.Id}' must contain an inline StoryDestination field named 'destination'.");
            SetDestination(destination, choiceData.Destination);
        }
    }

    private static void AppendScenesToStory(
        Story story,
        List<SceneData> scenes,
        Dictionary<string, Character> characters)
    {
        Undo.RecordObject(story, "Import Dialogue");
        SerializedObject so = new SerializedObject(story);
        so.Update();

        SerializedProperty list = FindProperty(so, "scenes", "Scenes");
        if (list == null || !list.isArray)
            throw new InvalidOperationException("Story must contain a serialized List<StoryScene> field named 'scenes'.");

        int originalSize = list.arraySize;
        list.arraySize = originalSize + scenes.Count;

        for (int i = 0; i < scenes.Count; i++)
        {
            SceneData data = scenes[i];
            SerializedProperty scene = list.GetArrayElementAtIndex(originalSize + i);
            if (scene.propertyType != SerializedPropertyType.Generic)
                throw new InvalidOperationException(
                    "Story.scenes must be a List<StoryScene> of inline [Serializable] entries, not Unity object references.");

            SetStringRelative(scene, data.Id, "sceneId", "id");
            SetStringRelative(scene, data.Name, "sceneName", "displayName", "name");

            SerializedProperty nodes = FindRelative(scene, "dialogueNodes", "nodes");
            if (nodes == null || !nodes.isArray)
                throw new InvalidOperationException(
                    "StoryScene must contain a serialized List<DialogueNode> field named 'dialogueNodes'.");

            nodes.arraySize = data.Nodes.Count;
            for (int n = 0; n < data.Nodes.Count; n++)
            {
                SerializedProperty node = nodes.GetArrayElementAtIndex(n);
                PopulateNode(node, data.Nodes[n], characters);
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(story);
    }

    private static void SetDestination(SerializedProperty property, DestinationData destination)
    {
        if (property.propertyType != SerializedPropertyType.Generic)
            throw new InvalidOperationException(
                $"Destination field '{property.displayName}' must be an inline serializable StoryDestination class.");

        SerializedProperty sceneId = FindRelative(property, "sceneId", "sceneID");
        SerializedProperty nodeId = FindRelative(property, "nodeId", "nodeID");
        if (sceneId == null || nodeId == null ||
            sceneId.propertyType != SerializedPropertyType.String ||
            nodeId.propertyType != SerializedPropertyType.String)
        {
            throw new InvalidOperationException(
                $"Destination field '{property.displayName}' needs serialized string fields 'sceneId' and 'nodeId'.");
        }

        sceneId.stringValue = destination.IsEmpty ? string.Empty : destination.SceneId;
        nodeId.stringValue = destination.IsEmpty ? string.Empty : destination.NodeId;
    }

    private static DestinationData ParseDestination(string raw, string currentSceneId, bool allowEnd)
    {
        raw = (raw ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw) || raw == "-" || Same(raw, "END"))
        {
            if (!allowEnd)
                throw new FormatException("A choice must point to a node; it cannot point directly to END.");
            return new DestinationData { IsEmpty = true };
        }

        int slash = raw.IndexOf('/');
        string sceneId = slash < 0 ? currentSceneId : raw.Substring(0, slash).Trim();
        string nodeId = slash < 0 ? raw : raw.Substring(slash + 1).Trim();
        if (string.IsNullOrWhiteSpace(sceneId) || string.IsNullOrWhiteSpace(nodeId) || nodeId.Contains("/") || sceneId.Contains("/"))
            throw new FormatException($"Invalid destination '{raw}'. Use nodeId or sceneId/nodeId.");
        return new DestinationData { SceneId = sceneId, NodeId = nodeId, IsEmpty = false };
    }

    private static List<string> SplitFields(string line)
    {
        List<string> fields = new List<string>();
        StringBuilder current = new StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\\' && i + 1 < line.Length && (line[i + 1] == '|' || line[i + 1] == '\\'))
            {
                current.Append(line[i + 1]);
                i++;
            }
            else if (c == '|')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else current.Append(c);
        }
        fields.Add(current.ToString());
        return fields;
    }

    private static string DecodeText(string value) => (value ?? string.Empty).Replace("\\n", "\n").Replace("\\r", "\r");

    private static void RequireFields(List<string> fields, int count, int line)
    {
        if (fields.Count != count)
            throw new FormatException($"Line {line}: expected exactly {count} fields but found {fields.Count}. If text contains a pipe character, escape it as \\|.");
    }

    private static SerializedProperty FindProperty(SerializedObject so, params string[] names)
    {
        foreach (string name in names)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p != null) return p;
        }
        return null;
    }

    private static SerializedProperty FindRelative(SerializedProperty parent, params string[] names)
    {
        if (parent == null) return null;
        foreach (string name in names)
        {
            SerializedProperty p = parent.FindPropertyRelative(name);
            if (p != null) return p;
        }
        return null;
    }

    private static void SetString(SerializedObject so, string value, params string[] names)
    {
        SerializedProperty p = FindProperty(so, names);
        if (p == null || p.propertyType != SerializedPropertyType.String)
            throw new InvalidOperationException($"Missing serialized string field '{names[0]}' on {so.targetObject.GetType().Name}.");
        p.stringValue = value ?? string.Empty;
    }

    private static void SetStringRelative(SerializedProperty parent, string value, params string[] names)
    {
        SerializedProperty p = FindRelative(parent, names);
        if (p == null || p.propertyType != SerializedPropertyType.String)
            throw new InvalidOperationException($"Missing serialized string field '{names[0]}' inside '{parent.displayName}'.");
        p.stringValue = value ?? string.Empty;
    }

    private static string GetString(SerializedObject so, params string[] names)
    {
        SerializedProperty p = FindProperty(so, names);
        return p != null && p.propertyType == SerializedPropertyType.String ? p.stringValue : string.Empty;
    }

    private static string GetStringRelative(SerializedProperty parent, params string[] names)
    {
        SerializedProperty p = FindRelative(parent, names);
        return p != null && p.propertyType == SerializedPropertyType.String ? p.stringValue : string.Empty;
    }

    private static string Key(string sceneId, string nodeId) => sceneId + "/" + nodeId;
    private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private sealed class SceneData
    {
        public string Id;
        public string Name;
        public readonly List<NodeData> Nodes = new List<NodeData>();
    }

    private sealed class NodeData
    {
        public string Id;
        public string SpeakerName;
        public string SideName;
        public string Text;
        public int Line;
        public DestinationData Next;
        public readonly List<ChoiceData> Choices = new List<ChoiceData>();
    }

    private sealed class ChoiceData
    {
        public string Id;
        public string Text;
        public int Line;
        public DestinationData Destination;
    }

    private sealed class PendingChoice
    {
        public string SceneId;
        public string SourceNodeId;
        public string ChoiceId;
        public string Text;
        public DestinationData Destination;
        public int Line;
    }

    private struct DestinationData
    {
        public string SceneId;
        public string NodeId;
        public bool IsEmpty;
    }
}
#endif
