using StackExchange.Redis;

namespace Services
{
    public abstract class BaseService
    {
        protected static IDatabase Redis = RedisConnection.Conn.DB;
    }
}
