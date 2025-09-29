using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

#if !NETSTANDARD2_0
namespace Jim.RedisManagement;
#else
namespace Jim.RedisManagement {
#endif

internal sealed class RedisManage : IRedisManage
{
    private static readonly string SysCustomKey = "Jim_";
    private readonly IConfiguration _configuration;
    public volatile ConnectionMultiplexer _redisConnection;
    private readonly object _redisConnectionLock = new object();
    private readonly ConfigurationOptions _configOptions;
    private readonly ILogger<RedisManage> _logger;
    private readonly RedisOptions _redisOptions;
    public RedisManage(ILogger<RedisManage> logger, IConfiguration configuration, IOptions<RedisOptions> redisOptions)
    {
        _logger = logger;
        _configuration = configuration;
        _redisOptions = redisOptions.Value;
        _configOptions = ReadRedisOptions();
        _redisConnection = ConnectionRedis();
    }

    private ConfigurationOptions ReadRedisOptions()
    {
        var ip = _redisOptions.Server;
        var port = _redisOptions.Port;

        ConfigurationOptions options = new ConfigurationOptions
        {
            ClientName = _redisOptions.ClientName,
            Password = _redisOptions.Password,
            ConnectTimeout = _redisOptions.Timeout,
            DefaultDatabase = _redisOptions.DefaultDb,
            AbortOnConnectFail = false,
        };
        options.EndPoints.Add(ip, port);

        if (options == null)
        {
            _logger.LogError("Redis数据库配置有误");
        }
        return options;
    }

    private ConnectionMultiplexer ConnectionRedis()
    {
        if (_redisConnection != null && _redisConnection.IsConnected)
        {
            return _redisConnection; // 已有连接，直接使用
        }
        lock (_redisConnectionLock)
        {
            if (_redisConnection != null)
            {
                _redisConnection.Dispose(); // 释放，重连
            }
            try
            {
                _redisConnection = ConnectionMultiplexer.Connect(_configOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis服务启动失败：{ex.Message}");
            }
        }
        return _redisConnection;
    }

    private string AddSysCustomKey(string oldKey)
    {
        return SysCustomKey + oldKey;
    }


    #region 公共方法

    private T DbDo<T>(int dbNum, Func<IDatabase, T> func)
    {
        var database = _redisConnection.GetDatabase(dbNum);
        return func(database);
    }

    private T Do<T>(Func<IDatabase, T> func)
    {
        var database = _redisConnection.GetDatabase(_redisOptions.DefaultDb);
        return func(database);
    }

    private string ConvertJson<T>(T value)
    {
        return value is string ? value.ToString() : JsonConvert.SerializeObject(value);
    }

    private T ConvertObj<T>(RedisValue value)
    {
        return JsonConvert.DeserializeObject<T>(value);
    }

    private List<T> ConvetList<T>(RedisValue[] values)
    {
        List<T> result = new List<T>();
        foreach (var item in values)
        {
            var model = ConvertObj<T>(item);
            result.Add(model);
        }
        return result;
    }

    private RedisKey[] ConvertRedisKeys(List<string> redisKeys)
    {
        return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
    }

    #endregion


    #region clear

    public void Clear()
    {
        foreach (var endPoint in _redisConnection.GetEndPoints())
        {
            var server = _redisConnection.GetServer(endPoint);
            foreach (var key in server.Keys())
            {
                _ = Do(db => db.KeyDelete(key));
            }
        }
    }

    public async Task ClearAsync()
    {
        foreach (var endPoint in _redisConnection.GetEndPoints())
        {
            var server = _redisConnection.GetServer(endPoint);
            foreach (var key in server.Keys())
            {
                _ = await Do(db => db.KeyDeleteAsync(key));
            }
        }
    }
    
    public void Clear(int dbNum)
    {
        foreach (var endPoint in _redisConnection.GetEndPoints())
        {
            var server = _redisConnection.GetServer(endPoint);
            foreach (var key in server.Keys())
            {
                _ = DbDo(dbNum, db => db.KeyDelete(key));
            }
        }
    }

    public async Task ClearAsync(int dbNum)
    {
        foreach (var endPoint in _redisConnection.GetEndPoints())
        {
            var server = _redisConnection.GetServer(endPoint);
            foreach (var key in server.Keys())
            {
                _ = await DbDo(dbNum, db => db.KeyDeleteAsync(key));
            }
        }
    }

    #endregion

    #region String

    #region 同步方法

    /// <summary>
    /// 保存单个key value
    /// </summary>
    /// <param name="key">Redis Key</param>
    /// <param name="value">保存的值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns></returns>
    public bool StringSet(string key, string value, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        return Do(db => db.StringSet(key, value, expiry));
    }
    /// <summary>
    /// 保存单个key value (选择库)
    /// </summary>
    /// <param name="dbNum">库1-15</param>
    /// <param name="key">Redis Key</param>
    /// <param name="value">保存的值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns></returns>
    public bool StringSet(int dbNum, string key, string value, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, db => db.StringSet(key, value, expiry));
    }

    /// <summary>
    /// 保存多个key value
    /// </summary>
    /// <param name="keyValues">键值对</param>
    /// <returns></returns>
    public bool StringSet(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
    {
        List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
            keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
        return Do(db => db.StringSet(newkeyValues.ToArray()));
    }

    /// <summary>
    /// 保存多个key value
    /// </summary>
    /// <param name="dbNum">库1-15</param>
    /// <param name="keyValues">键值对</param>
    /// <returns></returns>
    public bool StringSet(int dbNum, List<KeyValuePair<RedisKey, RedisValue>> keyValues)
    {
        List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
            keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
        return DbDo(dbNum,db => db.StringSet(newkeyValues.ToArray()));
    }

    /// <summary>
    /// 保存一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public bool StringSet<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        string json = ConvertJson(obj);
        return Do(db => db.StringSet(key, json, expiry));
    }

    /// <summary>
    /// 保存一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public bool StringSet<T>(int dbNum,string key, T obj, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        string json = ConvertJson(obj);
        return DbDo(dbNum, db => db.StringSet(key, json, expiry));
    }

    /// <summary>
    /// 获取单个key的值
    /// </summary>
    /// <param name="key">Redis Key</param>
    /// <returns></returns>
    public string StringGet(string key)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.StringGet(key));
    }

    /// <summary>
    /// 获取单个key的值
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public string StringGet(int dbNum, string key)
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, db => db.StringGet(key));
    }

    /// <summary>
    /// 获取多个Key
    /// </summary>
    /// <param name="listKey">Redis Key集合</param>
    /// <returns></returns>
    public RedisValue[] StringGet(List<string> listKey)
    {
        List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
        return Do(db => db.StringGet(ConvertRedisKeys(newKeys)));
    }

    /// <summary>
    /// 获取多个Key
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="listKey"></param>
    /// <returns></returns>
    public RedisValue[] StringGet(int dbNum,List<string> listKey)
    {
        List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
        return DbDo(dbNum, db => db.StringGet(ConvertRedisKeys(newKeys)));
    }

    /// <summary>
    /// 获取一个key的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T StringGet<T>(string key)
    {
        key = AddSysCustomKey(key);
        return Do(db => ConvertObj<T>(db.StringGet(key)));
    }

    /// <summary>
    /// 获取一个key的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public T StringGet<T>(int dbNum, string key)
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, db => ConvertObj<T>(db.StringGet(key)));
    }

    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    public double StringIncrement(string key, double val = 1)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.StringIncrement(key, val));
    }

    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    public double StringIncrement(int dbNum,string key, double val = 1)
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, db => db.StringIncrement(key, val));
    }

    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    public double StringDecrement(string key, double val = 1)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.StringDecrement(key, val));
    }

    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    public double StringDecrement(int dbNum,string key, double val = 1)
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, db => db.StringDecrement(key, val));
    }

    #endregion 同步方法

    #region 异步方法

    /// <summary>
    /// 保存单个key value
    /// </summary>
    /// <param name="key">Redis Key</param>
    /// <param name="value">保存的值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns></returns>
    public async Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.StringSetAsync(key, value, expiry));
    }

    /// <summary>
    /// 保存单个key value
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key">Redis Key</param>
    /// <param name="value">保存的值</param>
    /// <param name="expiry">过期时间</param>
    /// <returns></returns>
    public async Task<bool> StringSetAsync(int dbNum, string key, string value, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        return await DbDo(dbNum, db => db.StringSetAsync(key, value, expiry));
    }

    /// <summary>
    /// 保存多个key value
    /// </summary>
    /// <param name="keyValues">键值对</param>
    /// <returns></returns>
    public async Task<bool> StringSetAsync(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
    {
        List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
            keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
        return await Do(db => db.StringSetAsync(newkeyValues.ToArray()));
    }

    /// <summary>
    /// 保存多个key value
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="keyValues">键值对</param>
    /// <returns></returns>
    public async Task<bool> StringSetAsync(int dbNum, List<KeyValuePair<RedisKey, RedisValue>> keyValues)
    {
        List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
            keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
        return await DbDo(dbNum, db => db.StringSetAsync(newkeyValues.ToArray()));
    }

    /// <summary>
    /// 保存一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public async Task<bool> StringSetAsync<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        string json = ConvertJson(obj);
        return await Do(db => db.StringSetAsync(key, json, expiry));
    }

    /// <summary>
    /// 保存一个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public async Task<bool> StringSetAsync<T>(int dbNum,string key, T obj, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        string json = ConvertJson(obj);
        return await DbDo(dbNum, db => db.StringSetAsync(key, json, expiry));
    }

    /// <summary>
    /// 获取单个key的值
    /// </summary>
    /// <param name="key">Redis Key</param>
    /// <returns></returns>
    public async Task<string> StringGetAsync(string key)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.StringGetAsync(key));
    }

    /// <summary>
    /// 获取单个key的值
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key">Redis Key</param>
    /// <returns></returns>
    public async Task<string> StringGetAsync(int dbNum,string key)
    {
        key = AddSysCustomKey(key);
        return await DbDo(dbNum, db => db.StringGetAsync(key));
    }

    /// <summary>
    /// 获取多个Key
    /// </summary>
    /// <param name="listKey">Redis Key集合</param>
    /// <returns></returns>
    public async Task<RedisValue[]> StringGetAsync(List<string> listKey)
    {
        List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
        return await Do(db => db.StringGetAsync(ConvertRedisKeys(newKeys)));
    }

    /// <summary>
    /// 获取多个Key
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="listKey">Redis Key集合</param>
    /// <returns></returns>
    public async Task<RedisValue[]> StringGetAsync(int dbNum ,List<string> listKey)
    {
        List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
        return await DbDo(dbNum, db => db.StringGetAsync(ConvertRedisKeys(newKeys)));
    }

    /// <summary>
    /// 获取一个key的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> StringGetAsync<T>(string key)
    {
        key = AddSysCustomKey(key);
        string result = await Do(db => db.StringGetAsync(key));
        if (string.IsNullOrEmpty(result))
            return ConvertObj<T>(result);
        return default;
    }

    /// <summary>
    /// 获取一个key的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> StringGetAsync<T>(int dbNum, string key)
    {
        key = AddSysCustomKey(key);
        string result = await DbDo(dbNum, db => db.StringGetAsync(key));
        if (string.IsNullOrEmpty(result))
            return ConvertObj<T>(result);
        return default;
    }

    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    public async Task<double> StringIncrementAsync(string key, double val = 1)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.StringIncrementAsync(key, val));
    }

    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    public async Task<double> StringIncrementAsync(int dbNum ,string key, double val = 1)
    {
        key = AddSysCustomKey(key);
        return await DbDo(dbNum, db => db.StringIncrementAsync(key, val));
    }

    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    public async Task<double> StringDecrementAsync(string key, double val = 1)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.StringDecrementAsync(key, val));
    }

    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    public async Task<double> StringDecrementAsync(int dbNum,string key, double val = 1)
    {
        key = AddSysCustomKey(key);
        return await DbDo(dbNum, db => db.StringDecrementAsync(key, val));
    }

    #endregion 异步方法

    #endregion

    #region List

    #region 同步方法

    /// <summary>
    /// 移除指定ListId的内部List的值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void ListRemove<T>(string key, T value)
    {
        key = AddSysCustomKey(key);
        Do(db => db.ListRemove(key, ConvertJson(value)));
    }

    /// <summary>
    /// 移除指定ListId的内部List的值
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void ListRemove<T>(int dbNum,string key, T value)
    {
        key = AddSysCustomKey(key);
        DbDo(dbNum, db => db.ListRemove(key, ConvertJson(value)));
    }

    /// <summary>
    /// 获取指定key的List
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public List<T> ListRange<T>(string key)
    {
        key = AddSysCustomKey(key);
        return Do(redis =>
        {
            var values = redis.ListRange(key);
            return ConvetList<T>(values);
        });
    }

    /// <summary>
    /// 获取指定key的List
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public List<T> ListRange<T>(int dbNum,string key)
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, redis =>
        {
            var values = redis.ListRange(key);
            return ConvetList<T>(values);
        });
    }

    /// <summary>
    /// 入队(右侧)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void ListRightPush<T>(string key, T value)
    {
        key = AddSysCustomKey(key);
        Do(db => db.ListRightPush(key, ConvertJson(value)));
    }

    /// <summary>
    /// 入队(右侧)
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void ListRightPush<T>(int dbNum,string key, T value)
    {
        key = AddSysCustomKey(key);
        DbDo(dbNum, db => db.ListRightPush(key, ConvertJson(value)));
    }

    /// <summary>
    /// 出队右侧
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T ListRightPop<T>(string key)
    {
        key = AddSysCustomKey(key);
        return Do(db =>
        {
            var value = db.ListRightPop(key);
            return ConvertObj<T>(value);
        });
    }

    /// <summary>
    /// 出队右侧
    /// </summary>
    /// <param name="dbNum"></param>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T ListRightPop<T>(int dbNum,string key)
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, db =>
        {
            var value = db.ListRightPop(key);
            return ConvertObj<T>(value);
        });
    }

    /// <summary>
    /// 入栈（左侧）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void ListLeftPush<T>(string key, T value)
    {
        key = AddSysCustomKey(key);
        Do(db => db.ListLeftPush(key, ConvertJson(value)));
    }

    /// <summary>
    /// 入栈（左侧）
    /// </summary>
    /// <param name="dbNum"></param>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void ListLeftPush<T>(int dbNum,string key, T value)
    {
        key = AddSysCustomKey(key);
        DbDo(dbNum, db => db.ListLeftPush(key, ConvertJson(value)));
    }

    /// <summary>
    /// 出栈（左侧）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T ListLeftPop<T>(string key)
    {
        key = AddSysCustomKey(key);
        return Do(db =>
        {
            var value = db.ListLeftPop(key);
            return ConvertObj<T>(value);
        });
    }

    /// <summary>
    /// 出栈（左侧）
    /// </summary>
    /// <param name="dbNum"></param>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T ListLeftPop<T>(int dbNum ,string key)
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, db =>
        {
            var value = db.ListLeftPop(key);
            return ConvertObj<T>(value);
        });
    }

    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public long ListLength(string key)
    {
        key = AddSysCustomKey(key);
        return Do(redis => redis.ListLength(key));
    }

    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public long ListLength(int dbNum,string key)
    {
        key = AddSysCustomKey(key);
        return DbDo(dbNum, redis => redis.ListLength(key));
    }

    #endregion 同步方法

    #region 异步方法

    /// <summary>
    /// 移除指定ListId的内部List的值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task<long> ListRemoveAsync<T>(string key, T value)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.ListRemoveAsync(key, ConvertJson(value)));
    }

    /// <summary>
    /// 移除指定ListId的内部List的值
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task<long> ListRemoveAsync<T>(int dbNum, string key, T value)
    {
        key = AddSysCustomKey(key);
        return await DbDo(dbNum, db => db.ListRemoveAsync(key, ConvertJson(value)));
    }

    /// <summary>
    /// 获取指定key的List
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<List<T>> ListRangeAsync<T>(string key)
    {
        key = AddSysCustomKey(key);
        var values = await Do(redis => redis.ListRangeAsync(key));
        return ConvetList<T>(values);
    }

    /// <summary>
    /// 获取指定key的List
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<List<T>> ListRangeAsync<T>(int dbNum ,string key)
    {
        key = AddSysCustomKey(key);
        var values = await DbDo(dbNum ,redis => redis.ListRangeAsync(key));
        return ConvetList<T>(values);
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task<long> ListRightPushAsync<T>(string key, T value)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.ListRightPushAsync(key, ConvertJson(value)));
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task<long> ListRightPushAsync<T>(int dbNum ,string key, T value)
    {
        key = AddSysCustomKey(key);
        return await DbDo(dbNum ,db => db.ListRightPushAsync(key, ConvertJson(value)));
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> ListRightPopAsync<T>(string key)
    {
        key = AddSysCustomKey(key);
        var value = await Do(db => db.ListRightPopAsync(key));
        return ConvertObj<T>(value);
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> ListRightPopAsync<T>(int dbNum,string key)
    {
        key = AddSysCustomKey(key);
        var value = await DbDo(dbNum ,db => db.ListRightPopAsync(key));
        return ConvertObj<T>(value);
    }

    /// <summary>
    /// 入栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task<long> ListLeftPushAsync<T>(string key, T value)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.ListLeftPushAsync(key, ConvertJson(value)));
    }

    /// <summary>
    /// 入栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task<long> ListLeftPushAsync<T>(int dbNum ,string key, T value)
    {
        key = AddSysCustomKey(key);
        return await DbDo(dbNum ,db => db.ListLeftPushAsync(key, ConvertJson(value)));
    }

    /// <summary>
    /// 出栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> ListLeftPopAsync<T>(string key)
    {
        key = AddSysCustomKey(key);
        var value = await Do(db => db.ListLeftPopAsync(key));
        return ConvertObj<T>(value);
    }

    /// <summary>
    /// 出栈
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> ListLeftPopAsync<T>(int dbNum,string key)
    {
        key = AddSysCustomKey(key);
        var value = await DbDo(dbNum ,db => db.ListLeftPopAsync(key));
        return ConvertObj<T>(value);
    }

    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<long> ListLengthAsync(string key)
    {
        key = AddSysCustomKey(key);
        return await Do(redis => redis.ListLengthAsync(key));
    }

    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="dbNum"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<long> ListLengthAsync(int dbNum ,string key)
    {
        key = AddSysCustomKey(key);
        return await DbDo(dbNum, redis => redis.ListLengthAsync(key));
    }

    #endregion 异步方法

    #endregion

    #region Hash

    #region 同步方法

    /// <summary>
    /// 判断某个数据是否已经被缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    public bool HashExists(string key, string dataKey)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.HashExists(key, dataKey));
    }

    /// <summary>
    /// 存储数据到hash表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool HashSet<T>(string key, string dataKey, T t)
    {
        key = AddSysCustomKey(key);
        return Do(db =>
        {
            string json = ConvertJson(t);
            return db.HashSet(key, dataKey, json);
        });
    }

    /// <summary>
    /// 移除hash中的某值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    public bool HashDelete(string key, string dataKey)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.HashDelete(key, dataKey));
    }

    /// <summary>
    /// 移除hash中的多个值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKeys"></param>
    /// <returns></returns>
    public long HashDelete(string key, List<RedisValue> dataKeys)
    {
        key = AddSysCustomKey(key);
        //List<RedisValue> dataKeys1 = new List<RedisValue>() {"1","2"};
        return Do(db => db.HashDelete(key, dataKeys.ToArray()));
    }

    /// <summary>
    /// 从hash表获取数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    public T HashGet<T>(string key, string dataKey)
    {
        key = AddSysCustomKey(key);
        return Do(db =>
        {
            string value = db.HashGet(key, dataKey);
            return ConvertObj<T>(value);
        });
    }

    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    public double HashIncrement(string key, string dataKey, double val = 1)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.HashIncrement(key, dataKey, val));
    }

    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    public double HashDecrement(string key, string dataKey, double val = 1)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.HashDecrement(key, dataKey, val));
    }

    /// <summary>
    /// 获取hashkey所有Redis key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public List<T> HashKeys<T>(string key)
    {
        key = AddSysCustomKey(key);
        return Do(db =>
        {
            RedisValue[] values = db.HashKeys(key);
            return ConvetList<T>(values);
        });
    }

    #endregion 同步方法

    #region 异步方法

    /// <summary>
    /// 判断某个数据是否已经被缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    public async Task<bool> HashExistsAsync(string key, string dataKey)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.HashExistsAsync(key, dataKey));
    }

    /// <summary>
    /// 存储数据到hash表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public async Task<bool> HashSetAsync<T>(string key, string dataKey, T t)
    {
        key = AddSysCustomKey(key);
        return await Do(db =>
        {
            string json = ConvertJson(t);
            return db.HashSetAsync(key, dataKey, json);
        });
    }

    /// <summary>
    /// 移除hash中的某值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    public async Task<bool> HashDeleteAsync(string key, string dataKey)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.HashDeleteAsync(key, dataKey));
    }

    /// <summary>
    /// 移除hash中的多个值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKeys"></param>
    /// <returns></returns>
    public async Task<long> HashDeleteAsync(string key, List<RedisValue> dataKeys)
    {
        key = AddSysCustomKey(key);
        //List<RedisValue> dataKeys1 = new List<RedisValue>() {"1","2"};
        return await Do(db => db.HashDeleteAsync(key, dataKeys.ToArray()));
    }

    /// <summary>
    /// 从hash表获取数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    public async Task<T> HashGeAsync<T>(string key, string dataKey) where T : class, new()
    {
        key = AddSysCustomKey(key);
        string value = await Do(db => db.HashGetAsync(key, dataKey));
        return ConvertObj<T>(value);
    }

    /// <summary>
    /// 从hash表获取数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <returns></returns>
    public async Task<string> HashGeAsync(string key, string dataKey)
    {
        key = AddSysCustomKey(key);
        string value = await Do(db => db.HashGetAsync(key, dataKey));
        return value;
    }

    /// <summary>
    /// 为数字增长val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="val">可以为负</param>
    /// <returns>增长后的值</returns>
    public async Task<double> HashIncrementAsync(string key, string dataKey, double val = 1)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.HashIncrementAsync(key, dataKey, val));
    }

    /// <summary>
    /// 为数字减少val
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataKey"></param>
    /// <param name="val">可以为负</param>
    /// <returns>减少后的值</returns>
    public async Task<double> HashDecrementAsync(string key, string dataKey, double val = 1)
    {
        key = AddSysCustomKey(key);
        return await Do(db => db.HashDecrementAsync(key, dataKey, val));
    }

    /// <summary>
    /// 获取hashkey所有Redis key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<List<T>> HashKeysAsync<T>(string key)
    {
        key = AddSysCustomKey(key);
        RedisValue[] values = await Do(db => db.HashKeysAsync(key));
        return ConvetList<T>(values);
    }

    #endregion 异步方法


    #endregion Hash

    #region SortedSet 有序集合

    #region 同步方法

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="score"></param>
    public bool SortedSetAdd<T>(string key, T value, double score)
    {
        key = AddSysCustomKey(key);
        return Do(redis => redis.SortedSetAdd(key, ConvertJson<T>(value), score));
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public bool SortedSetRemove<T>(string key, T value)
    {
        key = AddSysCustomKey(key);
        return Do(redis => redis.SortedSetRemove(key, ConvertJson(value)));
    }

    /// <summary>
    /// 获取全部
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public List<T> SortedSetRangeByRank<T>(string key)
    {
        key = AddSysCustomKey(key);
        return Do(redis =>
        {
            var values = redis.SortedSetRangeByRank(key);
            return ConvetList<T>(values);
        });
    }

    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public long SortedSetLength(string key)
    {
        key = AddSysCustomKey(key);
        return Do(redis => redis.SortedSetLength(key));
    }

    #endregion 同步方法

    #region 异步方法

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="score"></param>
    public async Task<bool> SortedSetAddAsync<T>(string key, T value, double score)
    {
        key = AddSysCustomKey(key);
        return await Do(redis => redis.SortedSetAddAsync(key, ConvertJson<T>(value), score));
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task<bool> SortedSetRemoveAsync<T>(string key, T value)
    {
        key = AddSysCustomKey(key);
        return await Do(redis => redis.SortedSetRemoveAsync(key, ConvertJson(value)));
    }

    /// <summary>
    /// 获取全部
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<List<T>> SortedSetRangeByRankAsync<T>(string key)
    {
        key = AddSysCustomKey(key);
        var values = await Do(redis => redis.SortedSetRangeByRankAsync(key));
        return ConvetList<T>(values);
    }

    /// <summary>
    /// 获取集合中的数量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<long> SortedSetLengthAsync(string key)
    {
        key = AddSysCustomKey(key);
        return await Do(redis => redis.SortedSetLengthAsync(key));
    }

    #endregion 异步方法

    #endregion SortedSet 有序集合

    #region key

    /// <summary>
    /// 删除单个key
    /// </summary>
    /// <param name="key">redis key</param>
    /// <returns>是否删除成功</returns>
    public bool KeyDelete(string key)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.KeyDelete(key));
    }

    /// <summary>
    /// 删除多个key
    /// </summary>
    /// <param name="keys">rediskey</param>
    /// <returns>成功删除的个数</returns>
    public long KeyDelete(List<string> keys)
    {
        List<string> newKeys = keys.Select(AddSysCustomKey).ToList();
        return Do(db => db.KeyDelete(ConvertRedisKeys(newKeys)));
    }

    /// <summary>
    /// 判断key是否存储
    /// </summary>
    /// <param name="key">redis key</param>
    /// <returns></returns>
    public bool KeyExists(string key)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.KeyExists(key));
    }

    /// <summary>
    /// 重新命名key
    /// </summary>
    /// <param name="key">就的redis key</param>
    /// <param name="newKey">新的redis key</param>
    /// <returns></returns>
    public bool KeyRename(string key, string newKey)
    {
        key = AddSysCustomKey(key);
        return Do(db => db.KeyRename(key, newKey));
    }

    /// <summary>
    /// 设置Key的时间
    /// </summary>
    /// <param name="key">redis key</param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
    {
        key = AddSysCustomKey(key);
        return Do(db => db.KeyExpire(key, expiry));
    }

    #endregion key

    #region 发布订阅

    /// <summary>
    /// 订阅(返回msg)
    /// </summary>
    /// <param name="topticName"></param>
    /// <param name="handler"></param>
    public string SubScriper(string topticName, Action<RedisChannel, RedisValue> handler = null)
    {
        ISubscriber subscriber = _redisConnection.GetSubscriber();
        var channelMessageQueue = subscriber.Subscribe(topticName);
        var restulStr = "";
        channelMessageQueue.OnMessage(channelMessage =>
        {
            if (handler != null)
            {
                var redisChannel = channelMessage.Channel;
                var msg = channelMessage.Message;
                restulStr = ConvertJson(msg);
                handler.Invoke(redisChannel, msg);
            }
            else
            {
                restulStr = ConvertJson(channelMessage.Message);
            }
        });
        return restulStr;
    }

    /// <summary>
    /// Redis发布订阅  订阅
    /// </summary>
    /// <param name="subChannel"></param>
    /// <param name="handler"></param>
    public void Subscribe(string subChannel, Action<RedisChannel, RedisValue> handler = null)
    {
        ISubscriber sub = _redisConnection.GetSubscriber();
        sub.Subscribe(subChannel, (channel, message) =>
        {
            if (handler == null)
            {
                Console.WriteLine(subChannel + " 订阅收到消息：" + message);
            }
            else
            {
                handler(channel, message);
            }
        });
    }

    /// <summary>
    /// Redis发布订阅  发布
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="channel"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public long Publish<T>(string channel, T msg)
    {
        ISubscriber sub = _redisConnection.GetSubscriber();
        return sub.Publish(channel, ConvertJson(msg));
    }

    /// <summary>
    /// Redis发布订阅  取消订阅
    /// </summary>
    /// <param name="channel"></param>
    public void Unsubscribe(string channel)
    {
        ISubscriber sub = _redisConnection.GetSubscriber();
        sub.Unsubscribe(channel);
    }

    /// <summary>
    /// Redis发布订阅  取消全部订阅
    /// </summary>
    public void UnsubscribeAll()
    {
        ISubscriber sub = _redisConnection.GetSubscriber();
        sub.UnsubscribeAll();
    }

    #endregion 发布订阅

    #region 其他

    public ITransaction CreateTransaction()
    {
        return GetDatabase().CreateTransaction();
    }

    public IDatabase GetDatabase()
    {
        return _redisConnection.GetDatabase(_redisOptions.DefaultDb);
    }

    public IServer GetServer(string hostAndPort)
    {
        return _redisConnection.GetServer(hostAndPort);
    }
    public List<RedisKey> GetKeys()
    {
        var keys = new List<RedisKey>();
        foreach (var endPoint in _redisConnection.GetEndPoints())
        {
            var server = _redisConnection.GetServer(endPoint);
            keys = server.Keys().ToList();
        }

        return keys;
    }
    /// <summary>
    /// 获取redis所有key
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public List<RedisKey> GetDbKeys()
    {
        var keys = new List<RedisKey>();
        if (_redisConnection != null && _redisConnection.IsConnected)
        {
            foreach (var endPoint in _redisConnection.GetEndPoints())
            {
                var server = _redisConnection.GetServer(endPoint);
                keys = server.Keys().ToList();
            }
        }
        else
        {
            lock (_redisConnectionLock)
            {
                if (_redisConnection != null)
                    _redisConnection.Dispose(); // 释放，重连

                try
                {
                    _redisConnection = ConnectionMultiplexer.Connect(_configOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"获取 key  Redis服务启动失败：{ex.Message}");
                }
                foreach (var endPoint in _redisConnection.GetEndPoints())
                {
                    var server = _redisConnection.GetServer(endPoint);
                    keys = server.Keys().ToList();
                }
            }
        }
        return keys;
    }

    /// <summary>
    /// 根据 key 模糊查询删除
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveByKey(string key)
    {
        var db = GetDatabase();
        var redisResult = await db.ScriptEvaluateAsync(LuaScript.Prepare(
            //模糊查询：
            " local res = redis.call('KEYS', @keypattern) " +
            " return res "), new { @keypattern = key });

        if (!redisResult.IsNull)
        {
            var keys = (string[])redisResult;
            foreach (var k in keys)
                db.KeyDelete(k);
        }
    }

    /// <summary>
    /// 执行Lua脚本
    /// </summary>
    public async Task<T> ExecuteLuaScriptAsync<T>(string script, RedisKey[] keys, RedisValue[] values)
    {
        var db = GetDatabase();
        var result = await db.ScriptEvaluateAsync(script, keys, values);

        if (typeof(T) == typeof(long))
            return (T)(object)(long)result;
        if (typeof(T) == typeof(string))
            return (T)(object)(string)result;
        if (typeof(T) == typeof(bool))
            return (T)(object)(bool)result;
        if (typeof(T) == typeof(double))
            return (T)(object)(double)result;

        return (T)(object)result;
    }

    #endregion 其他

    #region 分布式锁
    //static string token = Environment.MachineName;

    /// <summary>
    /// 加锁，如果锁定成功，就去执行方法
    /// </summary>
    public bool LockTake(string key, string value, int seconds)
    {
        //string key = "lock";
        // key：用key来当锁，因为key是唯一的。
        // value：很多童鞋可能不明白，有key作为锁不就够了吗，为什么还要用到value?原因就是我们在上面讲到可靠性时，
        // 分布式锁要满足第四个条件解铃还须系铃人，通过给value赋值为Guid.NewGuid().ToString()，我们就知道这把锁是哪个请求加的了，在解锁的时候就可以有依据。
        //return _db.LockTake(key, data, (DateTime.Now.AddSeconds(seconds) - DateTime.Now));
        key = AddSysCustomKey(key);
        return Do(db => db.LockTake(key, value, (DateTime.Now.AddSeconds(seconds) - DateTime.Now)));
    }


    /// <summary>
    /// 解锁
    /// </summary>
    public bool LockRelease(string key, string value)
    {
        //string key = "lock";
        return Do(db => db.LockRelease(key, value));
    }
    #endregion

}
#if NETSTANDARD2_0
}
#endif