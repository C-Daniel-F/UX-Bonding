using System.Collections.Generic;

public class GameState
{
    private readonly Dictionary<string, bool> flags =
        new Dictionary<string, bool>();

    private readonly Dictionary<string, int> relationships =
        new Dictionary<string, int>();



    public void SetFlag(string flagId, bool value)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            return;
        }

        flags[flagId] = value;
    }

    public bool GetFlag(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            return false;
        }

        return flags.TryGetValue(flagId, out bool value) && value;
    }

    public bool HasFlag(string flagId)
    {
        return GetFlag(flagId);
    }

    public void RemoveFlag(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            return;
        }

        flags.Remove(flagId);
    }


    public void SetRelationship(string characterId, int value)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        relationships[characterId] = value;
    }

    public int GetRelationship(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return 0;
        }

        return relationships.TryGetValue(characterId, out int value)
            ? value
            : 0;
    }

    public void ModifyRelationship(string characterId, int amount)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        int currentValue = GetRelationship(characterId);
        relationships[characterId] = currentValue + amount;
    }

    public void ResetRelationship(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        relationships[characterId] = 0;
    }


    public void Clear()
    {
        flags.Clear();
        relationships.Clear();
    }
}