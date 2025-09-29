using StackExchange.Redis;

#if !NETSTANDARD2_0
namespace Jim.RedisManagement;
#else
namespace Jim.RedisManagement {
#endif
public interface IRedisManage
{

    #region clear
    /// <summary>
    /// 清空reids
    /// </summary>
    void Clear();
    /// <summary>
    /// 清空reids
    /// </summary>
    /// <returns></returns>
    Task ClearAsync();
    /// <summary>
    /// 清空reids
    /// </summary>
    /// <param name="dbNum"></param>
    void Clear(int dbNum);
    /// <summary>
    /// 清空reids
    /// </summary>
    /// <param name="dbNum"></param>
    /// <returns></returns>
    Task ClearAsync(int dbNum);

    #endregion

    #region String

    /// <summary>
    /// 保存单个key value
    /// </summary>
    /// <param name="key">Redis Key</param>
    /// <param name="value">保存的值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns></returns>
    bool StringSet(string key, string value, TimeSpan? expiry = default(TimeSpan?));
    /// <summary>
    /// 保存单个key value (选择库)
    /// </summary>
    /// <param name="dbNum">库1-15</param>
    /// <param name="key">Redis Key</param>
    /// <param name="value">保存的值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns></returns>
    bool StringSet(int dbNum, string key, string value, TimeSpan? expiry = default(TimeSpan?));
    /// <summary>
    /// 保存多个key value
    /// </summary>
    /// <param name="keyValues">键值对</param>
    /// <returns></returns>
    bool StringSet(List<KeyValuePair<RedisKey, RedisValue>> keyValues);
    /// <summary>
    /// 保存多个key value
    /// </summary>
    /// <param name="dbNum">库1-15</param>
    /// <param name="keyValues">键值对</param>
    /// <returns></returns>
    bool StringSet(int dbNum, List<KeyValuePair<RedisKey, RedisValue>> keyValues);
    /// <summary>
    /// 保存一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    bool StringSet<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?));
    /// <summary>
    /// 保存一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    bool StringSet<T>(int dbNum, string key, T obj, TimeSpan? expiry = default(TimeSpan?));
    /// <summary>
    /// 获取单个key的值
    /// </summary>
    /// <param name="key">Redis Key</param>
    /// <returns></returns>
    string StringGet(string key);
    /// <summary>
    /// 获取单个key的值
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    string StringGet(int dbNum, string key);
    /// <summary>
    /// 获取多个Key
    /// </summary>
    /// <param name="listKey">Redis Key集合</param>
    /// <returns></returns>
    RedisValue[] StringGet(List<string> listKey);
    /// <summary>
    /// 获取多个Key
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="listKey"></param>
    /// <returns></returns>
    RedisValue[] StringGet(int dbNum, List<string> listKey);
    /// <summary>
    /// 获取一个key的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T StringGet<T>(string key);
    /// <summary>
    /// 获取一个key的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    T StringGet<T>(int dbNum, string key);
    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    double StringIncrement(string key, double val = 1);
    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    double StringIncrement(int dbNum, string key, double val = 1);
    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    double StringDecrement(string key, double val = 1);
    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    double StringDecrement(int dbNum, string key, double val = 1);

    /// <summary>
    /// 保存单个key value
    /// </summary>
    /// <param name="key">Redis Key</param>
    /// <param name="value">保存的值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns></returns>
    Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = default(TimeSpan?));
    /// <summary>
    /// 保存单个key value
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key">Redis Key</param>
    /// <param name="value">保存的值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns></returns>
    Task<bool> StringSetAsync(int dbNum, string key, string value, TimeSpan? expiry = default(TimeSpan?));
    /// <summary>
    /// 保存多个key value
    /// </summary>
    /// <param name="keyValues">键值对</param>
    /// <returns></returns>
    Task<bool> StringSetAsync(List<KeyValuePair<RedisKey, RedisValue>> keyValues);
    /// <summary>
    /// 保存多个key value
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="keyValues">键值对</param>
    /// <returns></returns>
    Task<bool> StringSetAsync(int dbNum, List<KeyValuePair<RedisKey, RedisValue>> keyValues);
    /// <summary>
    /// 保存一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    Task<bool> StringSetAsync<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?));
    /// <summary>
    /// 保存一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    Task<bool> StringSetAsync<T>(int dbNum, string key, T obj, TimeSpan? expiry = default(TimeSpan?));
    /// <summary>
    /// 获取单个key的值
    /// </summary>
    /// <param name="key">Redis Key</param>
    /// <returns></returns>
    Task<string> StringGetAsync(string key);
    /// <summary>
    /// 获取单个key的值
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key">Redis Key</param>
    /// <returns></returns>
    Task<string> StringGetAsync(int dbNum, string key);
    /// <summary>
    /// 获取多个Key
    /// </summary>
    /// <param name="listKey">Redis Key集合</param>
    /// <returns></returns>
    Task<RedisValue[]> StringGetAsync(List<string> listKey);
    /// <summary>
    /// 获取多个Key
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="listKey">Redis Key集合</param>
    /// <returns></returns>
    Task<RedisValue[]> StringGetAsync(int dbNum, List<string> listKey);
    /// <summary>
    /// 获取一个key的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T> StringGetAsync<T>(string key);
    /// <summary>
    /// 获取一个key的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T> StringGetAsync<T>(int dbNum, string key);
    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    Task<double> StringIncrementAsync(string key, double val = 1);
    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    Task<double> StringIncrementAsync(int dbNum, string key, double val = 1);
    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    Task<double> StringDecrementAsync(string key, double val = 1);
    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    Task<double> StringDecrementAsync(int dbNum, string key, double val = 1);

    #endregion

    #region List

    /// <summary>
    /// 移除指定ListId的内部List的值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void ListRemove<T>(string key, T value);
    /// <summary>
    /// 移除指定ListId的内部List的值
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void ListRemove<T>(int dbNum, string key, T value);
    /// <summary>
    /// 获取指定key的List
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    List<T> ListRange<T>(string key);
    /// <summary>
    /// 获取指定key的List
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    List<T> ListRange<T>(int dbNum, string key);
    /// <summary>
    /// 入队(右侧)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void ListRightPush<T>(string key, T value);
    /// <summary>
    /// 入队(右侧)
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void ListRightPush<T>(int dbNum, string key, T value);
    /// <summary>
    /// 出队右侧
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T ListRightPop<T>(string key);
    /// <summary>
    /// 出队右侧
    /// </summary>
    /// <param name="dbNum"></param>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T ListRightPop<T>(int dbNum, string key);
    /// <summary>
    /// 入栈（左侧）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void ListLeftPush<T>(string key, T value);
    /// <summary>
    /// 入栈（左侧）
    /// </summary>
    /// <param name="dbNum"></param>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void ListLeftPush<T>(int dbNum, string key, T value);
    /// <summary>
    /// 出栈（左侧）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T ListLeftPop<T>(string key);
    /// <summary>
    /// 出栈（左侧）
    /// </summary>
    /// <param name="dbNum"></param>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T ListLeftPop<T>(int dbNum, string key);
    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    long ListLength(string key);
    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    long ListLength(int dbNum, string key);

    /// <summary>
    /// 移除指定ListId的内部List的值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    Task<long> ListRemoveAsync<T>(string key, T value);
    /// <summary>
    /// 移除指定ListId的内部List的值
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    Task<long> ListRemoveAsync<T>(int dbNum, string key, T value);
    /// <summary>
    /// 获取指定key的List
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<List<T>> ListRangeAsync<T>(string key);
    /// <summary>
    /// 获取指定key的List
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<List<T>> ListRangeAsync<T>(int dbNum, string key);
    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    Task<long> ListRightPushAsync<T>(string key, T value);
    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    Task<long> ListRightPushAsync<T>(int dbNum, string key, T value);
    /// <summary>
    /// 出队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T> ListRightPopAsync<T>(string key);
    /// <summary>
    /// 出队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T> ListRightPopAsync<T>(int dbNum, string key);
    /// <summary>
    /// 入栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    Task<long> ListLeftPushAsync<T>(string key, T value);
    /// <summary>
    /// 入栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    Task<long> ListLeftPushAsync<T>(int dbNum, string key, T value);
    /// <summary>
    /// 出栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T> ListLeftPopAsync<T>(string key);
    /// <summary>
    /// 出栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T> ListLeftPopAsync<T>(int dbNum, string key);
    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<long> ListLengthAsync(string key);
    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<long> ListLengthAsync(int dbNum, string key);

    #endregion

    #region Hash

    /// <summary>
    /// 判断某个数据是否已经被缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    bool HashExists(string key, string dataKey);
    /// <summary>
    /// 存储数据到hash表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    bool HashSet<T>(string key, string dataKey, T t);
    /// <summary>
    /// 移除hash中的某值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    bool HashDelete(string key, string dataKey);
    /// <summary>
    /// 移除hash中的多个值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKeys"></param>
    /// <returns></returns>
    long HashDelete(string key, List<RedisValue> dataKeys);
    /// <summary>
    /// 从hash表获取数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    T HashGet<T>(string key, string dataKey);
    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    double HashIncrement(string key, string dataKey, double val = 1);
    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    double HashDecrement(string key, string dataKey, double val = 1);
    /// <summary>
    /// 获取hashkey所有Redis key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    List<T> HashKeys<T>(string key);

    /// <summary>
    /// 判断某个数据是否已经被缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    Task<bool> HashExistsAsync(string key, string dataKey);
    /// <summary>
    /// 存储数据到hash表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    Task<bool> HashSetAsync<T>(string key, string dataKey, T t);
    /// <summary>
    /// 移除hash中的某值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    Task<bool> HashDeleteAsync(string key, string dataKey);
    /// <summary>
    /// 移除hash中的多个值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKeys"></param>
    /// <returns></returns>
    Task<long> HashDeleteAsync(string key, List<RedisValue> dataKeys);
    /// <summary>
    /// 从hash表获取数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    Task<T> HashGeAsync<T>(string key, string dataKey) where T : class, new();
    /// <summary>
    /// 从hash表获取数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    Task<string> HashGeAsync(string key, string dataKey);
    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    Task<double> HashIncrementAsync(string key, string dataKey, double val = 1);
    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    Task<double> HashDecrementAsync(string key, string dataKey, double val = 1);
    /// <summary>
    /// 获取hashkey所有Redis key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<List<T>> HashKeysAsync<T>(string key);

    #endregion Hash

    #region SortedSet 有序集合

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="score"></param>
    bool SortedSetAdd<T>(string key, T value, double score);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    bool SortedSetRemove<T>(string key, T value);

    /// <summary>
    /// 获取全部
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    List<T> SortedSetRangeByRank<T>(string key);
    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    long SortedSetLength(string key);

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="score"></param>
    Task<bool> SortedSetAddAsync<T>(string key, T value, double score);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    Task<bool> SortedSetRemoveAsync<T>(string key, T value);

    /// <summary>
    /// 获取全部
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<List<T>> SortedSetRangeByRankAsync<T>(string key);

    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<long> SortedSetLengthAsync(string key);

    #endregion SortedSet 有序集合

    #region key

    /// <summary>
    /// 删除单个key
    /// </summary>
    /// <param name="key">redis key</param>
    /// <returns>是否删除成功</returns>
    bool KeyDelete(string key);

    /// <summary>
    /// 删除多个key
    /// </summary>
    /// <param name="keys">rediskey</param>
    /// <returns>成功删除的个数</returns>
    long KeyDelete(List<string> keys);

    /// <summary>
    /// 判断key是否存储
    /// </summary>
    /// <param name="key">redis key</param>
    /// <returns></returns>
    bool KeyExists(string key);

    /// <summary>
    /// 重新命名key
    /// </summary>
    /// <param name="key">就的redis key</param>
    /// <param name="newKey">新的redis key</param>
    /// <returns></returns>
    bool KeyRename(string key, string newKey);

    /// <summary>
    /// 设置Key的时间
    /// </summary>
    /// <param name="key">redis key</param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?));

    #endregion key

    #region 发布订阅

    /// <summary>
    /// 订阅(返回msg)
    /// </summary>
    /// <param name="topticName"></param>
    /// <param name="handler"></param>
    string SubScriper(string topticName, Action<RedisChannel, RedisValue> handler = null);

    /// <summary>
    /// Redis发布订阅  订阅
    /// </summary>
    /// <param name="subChannel"></param>
    /// <param name="handler"></param>
    void Subscribe(string subChannel, Action<RedisChannel, RedisValue> handler = null);

    /// <summary>
    /// Redis发布订阅  发布
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="channel"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    long Publish<T>(string channel, T msg);

    /// <summary>
    /// Redis发布订阅  取消订阅
    /// </summary>
    /// <param name="channel"></param>
    void Unsubscribe(string channel);

    /// <summary>
    /// Redis发布订阅  取消全部订阅
    /// </summary>
    void UnsubscribeAll();

    #endregion 发布订阅

    #region 其他

    ITransaction CreateTransaction();

    IDatabase GetDatabase();

    IServer GetServer(string hostAndPort);
    List<RedisKey> GetKeys();
    /// <summary>
    /// 获取redis所有key
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    List<RedisKey> GetDbKeys();

    /// <summary>
    /// 根据 key 模糊查询删除
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task RemoveByKey(string key);

    /// <summary>
    /// 执行Lua脚本
    /// </summary>
    Task<T> ExecuteLuaScriptAsync<T>(string script, RedisKey[] keys, RedisValue[] values);

    #endregion 其他

    #region 分布式锁
    /// <summary>
    /// 加锁，如果锁定成功，就去执行方法
    /// </summary>
    bool LockTake(string key, string value, int seconds);

    /// <summary>
    /// 解锁
    /// </summary>
    bool LockRelease(string key, string value);
    #endregion

}
#if NETSTANDARD2_0
}
#endif