# jim.redisMamagement



## 🎨 使用方法 🎨

### 🔧 Installation 🔧

```bash
$> dotnet add package Jim.RedisManagement
```

### Config	
`appsettings.json`
```json
"Redis": {
    "Server": "127.0.0.1",
    "Port": "6379",
    "Password": "ABC.1234",
    "ClientName": "Baby",
    "DefaultDb": 1,
    "Timeout": "5000"
  }
```

### Server
```cs
server.AddRedis() 

```

### 在构造函数里面使用 
```cs
private readonly IRedisManage _redis;
public TestController(IRedisManage redis)
{
    _redis = redis;
}
```
