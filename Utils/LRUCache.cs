using System;
using System.Collections.Generic;

namespace NukeLib.Utils;

/// <summary>
/// Simple Least Recently Used cache
/// </summary>
/// <typeparam name="TKey">The cache key type</typeparam>
/// <typeparam name="TValue">The cache value type</typeparam>
public class LRUCache<TKey, TValue> where TKey : notnull {
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _map;
    private readonly LinkedList<CacheItem> _list;
    private readonly Action<TValue>? _onRemoved;
    private readonly object _lock = new();

    private struct CacheItem {
        public TKey Key;
        public TValue Value;

        public CacheItem(TKey key, TValue value) {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LRUCache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="capacity">Maximum capacity of the cache</param>
    /// <param name="onRemoved">Optional callback invoked when an item is removed</param>
    /// <exception cref="ArgumentOutOfRangeException">When capacity is negative</exception>
    public LRUCache(int capacity, Action<TValue>? onRemoved = null) {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        _capacity = capacity;
        _map = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
        _list = new LinkedList<CacheItem>();
        _onRemoved = onRemoved;
    }

    /// <summary>
    /// Try to get value from cache (might be null if it's not there)
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The output value (null if not in cache)</param>
    /// <returns>True if found, false otherwise</returns>
    public bool TryGetValue(TKey key, out TValue? value) {
        lock (_lock) {
            if (_map.TryGetValue(key, out var node)) {
                _list.Remove(node);
                _list.AddFirst(node);
                value = node.Value.Value;
                return true;
            }

            value = default;
            return false;
        }
    }

    /// <summary>
    /// Adds or updates an item in the cache
    /// </summary>
    public void Add(TKey key, TValue value) {
        lock (_lock) {
            if (_map.TryGetValue(key, out var existingNode)) {
                _list.Remove(existingNode);
                _onRemoved?.Invoke(existingNode.Value.Value);
                _map.Remove(key);
            } else if (_map.Count >= _capacity) {
                var last = _list.Last;
                if (last != null) {
                    _list.RemoveLast();
                    _map.Remove(last.Value.Key);
                    _onRemoved?.Invoke(last.Value.Value);
                }
            }

            var newItem = new CacheItem(key, value);
            var newNode = _list.AddFirst(newItem);
            _map[key] = newNode;
        }
    }

    /// <summary>
    /// Clears all entries from the cache
    /// </summary>
    public void Clear() {
        lock (_lock) {
            if (_onRemoved != null) {
                foreach (var item in _list) {
                    _onRemoved.Invoke(item.Value);
                }
            }

            _map.Clear();
            _list.Clear();
        }
    }
}
