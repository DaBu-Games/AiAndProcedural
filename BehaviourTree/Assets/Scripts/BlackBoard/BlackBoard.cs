using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public readonly struct BlackBoardKey : IEquatable<BlackBoardKey>
{
    private readonly string name;
    private readonly int hashedKey;

    public BlackBoardKey(string name)
    {
        this.name = name;
        hashedKey = name.ComputeFNV1aHash();
    }
    
    public bool Equals(BlackBoardKey other) => hashedKey == other.hashedKey;
    
    public override bool Equals(object obj) => obj is BlackBoardKey other && Equals(other);
    public override int GetHashCode() => hashedKey;
    public override string ToString() => name;
    
    public static bool operator ==(BlackBoardKey lhs, BlackBoardKey rhs) => lhs.hashedKey == rhs.hashedKey;
    public static bool operator !=(BlackBoardKey lhs, BlackBoardKey rhs) => !(lhs == rhs);
}

[Serializable]
public class BlackBoardEntry<T>
{
    public BlackBoardKey Key { get; }
    public T Value { get; }
    public Type ValueType { get; }

    public BlackBoardEntry(BlackBoardKey key, T value)
    {
        Key = key;
        Value = value;
        ValueType = typeof(T);
    }
    
    public override bool Equals(object obj) => obj is BlackBoardEntry<T> other && other.Key == Key;
    public override int GetHashCode() => Key.GetHashCode();
}

[Serializable]
public class BlackBoard
{
    private Dictionary<string, BlackBoardKey> keyRegistry = new();
    private Dictionary<BlackBoardKey, object> entries = new();

    public void Debug()
    {
        foreach (var entry in entries)
        {
            var entryType = entry.Value.GetType();

            if (entryType.IsGenericType && entryType.GetGenericTypeDefinition() == typeof(BlackBoardEntry<>))
            {
                var valueProperty = entryType.GetProperty("Value");
                if(valueProperty == null) continue;
                
                var value = valueProperty.GetValue(entry.Value);
                UnityEngine.Debug.Log($"Key: {entry.Key}, Value: {value}");
            }
        }
    }

    public bool TryGetValue<T>(BlackBoardKey key, out T value)
    {
        if (entries.TryGetValue(key, out var entry) && entry is BlackBoardEntry<T> castedEntry)
        {
            value = castedEntry.Value;
            return true;
        }
        
        value = default;
        return false;
    }

    public void SetValue<T>(BlackBoardKey key, T value)
    {
        entries[key] = new BlackBoardEntry<T>(key, value);
    }

    public BlackBoardKey GetOrRegister(string keyName)
    {
        if(keyName == null) throw new ArgumentNullException(nameof(keyName));

        if (!keyRegistry.TryGetValue(keyName, out BlackBoardKey key))
        {
            key = new BlackBoardKey(keyName);
            keyRegistry[keyName] = key;
        }
        
        return key;
    }
    
    public bool ContainsKey(BlackBoardKey key) => entries.ContainsKey(key);
    
    public void Remove(BlackBoardKey key) => entries.Remove(key);
}
