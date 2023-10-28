using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class RedisConnection
{
    private static readonly Lazy<RedisConnection> lazy =
        new Lazy<RedisConnection>(() => new RedisConnection());

    public static RedisConnection Conn { get { return lazy.Value; } }

    public IDatabase DB { get; }

    private RedisConnection()
    {
        var connection = ConnectionMultiplexer.Connect("localhost");
        DB = connection.GetDatabase();
    }
}
