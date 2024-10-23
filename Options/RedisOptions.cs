using StackExchange.Redis;

#if !NETSTANDARD2_0
namespace Jim.RedisManagement;
#else
namespace Jim.RedisManagement {
#endif

public class RedisOptions
{
    /// <summary>
    /// reids ip address
    /// </summary>
    public string Server { get; set; }
    /// <summary>
    /// redis prot
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// redis userName
    /// </summary>
    public string User { get; set; }
    /// <summary>
    /// redis password
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// redis cache milliseconds time
    /// </summary>
    public int Timeout { get; set; }
    /// <summary>
    /// reids connection name
    /// </summary>
    public string ClientName { get; set; }
    /// <summary>
    /// redis connection deafault db
    /// </summary>
    public int DefaultDb { get; set; }
}
#if NETSTANDARD2_0
}
#endif