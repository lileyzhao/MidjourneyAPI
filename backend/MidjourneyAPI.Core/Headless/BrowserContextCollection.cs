using System.Collections.Concurrent;
using PuppeteerSharp;

namespace MidjourneyAPI.Core.Headless;

/// <summary>
/// BrowserContext 实例的集合。
/// </summary>
public class BrowserContextCollection
{
    private readonly ConcurrentDictionary<string, IBrowserContext> _dictionary;
    private readonly object _lock = new();
    private readonly Random _random;
    private IEnumerator<KeyValuePair<string, IBrowserContext>> _enumerator;

    /// <summary>
    /// 初始化 BrowserContextCollection 类的新实例。
    /// </summary>
    public BrowserContextCollection()
    {
        _dictionary = new ConcurrentDictionary<string, IBrowserContext>();
        _enumerator = _dictionary.GetEnumerator();
        _random = new Random();
    }

    /// <summary>
    /// 获取当前集合中的上下文数量。
    /// </summary>
    public int Count => _dictionary.Count;

    /// <summary>
    /// 集合中所有的键。
    /// </summary>
    public ICollection<string> Keys => _dictionary.Keys;

    /// <summary>
    /// 集合中所有的值。
    /// </summary>
    public ICollection<IBrowserContext> Values => _dictionary.Values;

    /// <summary>
    /// 向集合中添加一个新的 IBrowserContext 实例。
    /// </summary>
    /// <param name="key">用于标识 IBrowserContext 实例的键。</param>
    /// <param name="context">要添加的 IBrowserContext 实例。</param>
    /// <returns>如果添加成功则返回 true，否则返回 false。</returns>
    public bool Add(string key, IBrowserContext context)
    {
        if (!_dictionary.TryAdd(key, context)) return false;

        lock (_lock)
        {
            ResetEnumerator();
        }

        return true;
    }

    /// <summary>
    /// 根据键获取 IBrowserContext 实例。
    /// </summary>
    /// <param name="key">用于标识 IBrowserContext 实例的键。</param>
    /// <returns>如果找到则返回 IBrowserContext 实例，否则返回 null。</returns>
    public IBrowserContext? Get(string key)
    {
        return _dictionary.TryGetValue(key, out var context) ? context : null;
    }

    /// <summary>
    /// 获取集合中的下一个 IBrowserContext 实例（轮询）。
    /// </summary>
    /// <returns>下一个 IBrowserContext 实例，如果集合为空则返回 null。</returns>
    public IBrowserContext? GetNext()
    {
        lock (_lock)
        {
            if (_enumerator.MoveNext()) return _enumerator.Current.Value;
            ResetEnumerator();
            return _enumerator.MoveNext() ? _enumerator.Current.Value : null;
        }
    }

    /// <summary>
    /// 随机获取集合中的一个 IBrowserContext 实例。
    /// </summary>
    /// <returns>随机选择的 IBrowserContext 实例，如果集合为空则返回 null。</returns>
    public IBrowserContext? GetRandom()
    {
        var values = _dictionary.Values.ToList();
        if (values.Count == 0) return null;

        lock (_lock)
        {
            var index = _random.Next(values.Count);
            return values[index];
        }
    }

    /// <summary>
    /// 从集合中移除指定键的 IBrowserContext 实例。
    /// </summary>
    /// <param name="key">用于标识要移除的 IBrowserContext 实例的键。</param>
    /// <param name="context">从集合移除的 IBrowserContext 实例</param>
    /// <returns>如果移除成功则返回 true，否则返回 false。</returns>
    public bool Remove(string key, out IBrowserContext? context)
    {
        var success = _dictionary.TryRemove(key, out var removedContext);
        context = removedContext;

        if (!success) return false;

        lock (_lock)
        {
            ResetEnumerator();
        }

        return true;
    }

    /// <summary>
    /// 清空集合中的所有 IBrowserContext 实例。
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _dictionary.Clear();
            ResetEnumerator();
        }
    }

    /// <summary>
    /// 重置枚举器，使其指向集合的起始位置。
    /// </summary>
    private void ResetEnumerator()
    {
        _enumerator = _dictionary.GetEnumerator();
    }
}