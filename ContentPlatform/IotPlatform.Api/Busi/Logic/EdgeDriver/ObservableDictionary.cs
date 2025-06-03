using IotPlatform.Api.Busi.Logic.Common;
using Microsoft.Extensions.Caching.Hybrid;
using ZiggyCreatures.Caching.Fusion;

namespace IotPlatform.Api.Busi.Logic.EdgeDriver;

public class ObservableDictionary<TKey, TValue>(IFusionCache fusionCache,ILogger<ObservableDictionary<TKey, TValue>> logger)
{
    private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

    public event EventHandler<DictionaryChangedEventArgs<TKey, TValue>> ItemAdded;
    public event EventHandler<DictionaryChangedEventArgs<TKey, TValue>> ItemRemoved;

    public void Add(TKey key, TValue value)
    {
        // var val= fusionCache.TryGet<string>(Constants.EdgedriversPrefix + key);
        // if (val.HasValue)
        // {
        //     logger.LogInformation($"The key {key} is already in the server dictionary."); 
        // }
        if (!_dictionary.ContainsKey(key))
        {
            _dictionary.Add(key, value);
            OnItemAdded(new DictionaryChangedEventArgs<TKey, TValue>(key, value));
        }
        // else
        // {
        //     logger.LogInformation($"The key {key} is already in the client dictionary.");
        // }

      
    }

    public bool Remove(TKey key)
    {
        if (_dictionary.Remove(key))
        {
            OnItemRemoved(new DictionaryChangedEventArgs<TKey, TValue>(key, _dictionary[key])); // 获取移除前的值
            return true;
        }

        return false;
    }

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            if (_dictionary.ContainsKey(key))
            {
                TValue oldValue = _dictionary[key];
                _dictionary[key] = value;
                OnItemAdded(new DictionaryChangedEventArgs<TKey, TValue>(key, value, oldValue)); // 可以添加更新事件
            }
            else
            {
                _dictionary[key] = value;
                OnItemAdded(new DictionaryChangedEventArgs<TKey, TValue>(key, value));
            }
        }
    }

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
    public int Count => _dictionary.Count;
    public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;
    public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

    protected virtual void OnItemAdded(DictionaryChangedEventArgs<TKey, TValue> e)
    {
        ItemAdded?.Invoke(this, e);
    }

    protected virtual void OnItemRemoved(DictionaryChangedEventArgs<TKey, TValue> e)
    {
        ItemRemoved?.Invoke(this, e);
    }
}

public class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
{
    public TKey Key { get; }
    public TValue NewValue { get; }
    public TValue OldValue { get; } // 可选，用于更新事件

    public DictionaryChangedEventArgs(TKey key, TValue newValue, TValue oldValue = default)
    {
        Key = key;
        NewValue = newValue;
        OldValue = oldValue;
    }
}