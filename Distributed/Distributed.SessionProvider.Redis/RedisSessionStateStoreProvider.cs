using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace Distributed.SessionProvider.Redis
{
    public class RedisSessionStateStoreProvider : SessionStateStoreProviderBase
    {
        #region Field

        private static readonly ConnectionMultiplexer ConnectionMultiplexer;

        #endregion Field

        #region Constructor

        static RedisSessionStateStoreProvider()
        {
            ConnectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { ConfigurationManager.AppSettings["RedisServer"] },
                ConnectTimeout = 30 * 1000
            });
        }

        #endregion Constructor

        #region Overrides of SessionStateStoreProviderBase

        /// <summary>
        /// Releases all resources used by the <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/> implementation.
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// Sets a reference to the <see cref="T:System.Web.SessionState.SessionStateItemExpireCallback"/> delegate for the Session_OnEnd event defined in the Global.asax file.
        /// </summary>
        /// <returns>
        /// true if the session-state store provider supports calling the Session_OnEnd event; otherwise, false.
        /// </returns>
        /// <param name="expireCallback">The <see cref="T:System.Web.SessionState.SessionStateItemExpireCallback"/>  delegate for the Session_OnEnd event defined in the Global.asax file.</param>
        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return true;
        }

        /// <summary>
        /// Called by the <see cref="T:System.Web.SessionState.SessionStateModule"/> object for per-request initialization.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param>
        public override void InitializeRequest(HttpContext context)
        {
        }

        /// <summary>
        /// Returns read-only session-state data from the session data store.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.SessionState.SessionStateStoreData"/> populated with session values and information from the session data store.
        /// </returns>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param><param name="id">The <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"/> for the current request.</param><param name="locked">When this method returns, contains a Boolean value that is set to true if the requested session item is locked at the session data store; otherwise, false.</param><param name="lockAge">When this method returns, contains a <see cref="T:System.TimeSpan"/> object that is set to the amount of time that an item in the session data store has been locked.</param><param name="lockId">When this method returns, contains an object that is set to the lock identifier for the current request. For details on the lock identifier, see "Locking Session-Store Data" in the <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/> class summary.</param><param name="actions">When this method returns, contains one of the <see cref="T:System.Web.SessionState.SessionStateActions"/> values, indicating whether the current session is an uninitialized, cookieless session.</param>
        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId,
            out SessionStateActions actions)
        {
            return DoGet(context, id, false, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// Returns read-only session-state data from the session data store.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.SessionState.SessionStateStoreData"/> populated with session values and information from the session data store.
        /// </returns>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param><param name="id">The <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"/> for the current request.</param><param name="locked">When this method returns, contains a Boolean value that is set to true if a lock is successfully obtained; otherwise, false.</param><param name="lockAge">When this method returns, contains a <see cref="T:System.TimeSpan"/> object that is set to the amount of time that an item in the session data store has been locked.</param><param name="lockId">When this method returns, contains an object that is set to the lock identifier for the current request. For details on the lock identifier, see "Locking Session-Store Data" in the <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/> class summary.</param><param name="actions">When this method returns, contains one of the <see cref="T:System.Web.SessionState.SessionStateActions"/> values, indicating whether the current session is an uninitialized, cookieless session.</param>
        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge,
            out object lockId, out SessionStateActions actions)
        {
            return DoGet(context, id, true, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// Releases a lock on an item in the session data store.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param><param name="id">The session identifier for the current request.</param><param name="lockId">The lock identifier for the current request. </param>
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
        }

        /// <summary>
        /// Updates the session-item information in the session-state data store with values from the current request, and clears the lock on the data.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param><param name="id">The session identifier for the current request.</param><param name="item">The <see cref="T:System.Web.SessionState.SessionStateStoreData"/> object that contains the current session values to be stored.</param><param name="lockId">The lock identifier for the current request. </param><param name="newItem">true to identify the session item as a new item; false to identify the session item as an existing item.</param>
        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            ISessionStateItemCollection sessionItems = null;

            if (item.Items.Count > 0)
                sessionItems = item.Items;

            var database = GetConnection().GetDatabase();
            RedisSessionStateStore.Create(database, id, item.Timeout).Set(sessionItems);
        }

        /// <summary>
        /// Deletes item data from the session data store.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param><param name="id">The session identifier for the current request.</param><param name="lockId">The lock identifier for the current request.</param><param name="item">The <see cref="T:System.Web.SessionState.SessionStateStoreData"/> that represents the item to delete from the data store.</param>
        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            var database = GetConnection().GetDatabase();
            RedisSessionStateStore.Delete(database, id);
        }

        /// <summary>
        /// Updates the expiration date and time of an item in the session data store.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param><param name="id">The session identifier for the current request.</param>
        public override void ResetItemTimeout(HttpContext context, string id)
        {
        }

        /// <summary>
        /// Creates a new <see cref="T:System.Web.SessionState.SessionStateStoreData"/> object to be used for the current request.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Web.SessionState.SessionStateStoreData"/> for the current request.
        /// </returns>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param><param name="timeout">The session-state <see cref="P:System.Web.SessionState.HttpSessionState.Timeout"/> value for the new <see cref="T:System.Web.SessionState.SessionStateStoreData"/>.</param>
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return CreateLegitStoreData(context, null, null, timeout);
        }

        /// <summary>
        /// Adds a new session-state item to the data store.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param><param name="id">The <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"/> for the current request.</param><param name="timeout">The session <see cref="P:System.Web.SessionState.HttpSessionState.Timeout"/> for the current request.</param>
        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            var database = GetConnection().GetDatabase();
            RedisSessionStateStore.Create(database, id, timeout);
        }

        /// <summary>
        /// Called by the <see cref="T:System.Web.SessionState.SessionStateModule"/> object at the end of a request.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Web.HttpContext"/> for the current request.</param>
        public override void EndRequest(HttpContext context)
        {
        }

        #endregion Overrides of SessionStateStoreProviderBase

        #region Private Method

        private static ConnectionMultiplexer GetConnection()
        {
            return ConnectionMultiplexer;
        }

        private static SessionStateStoreData CreateLegitStoreData(HttpContext context, ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout)
        {
            if (sessionItems == null)
                sessionItems = new SessionStateItemCollection();
            if (staticObjects == null && context != null)
                staticObjects = SessionStateUtility.GetSessionStaticObjects(context);
            return new SessionStateStoreData(sessionItems, staticObjects, timeout);
        }

        private SessionStateStoreData DoGet(HttpContext context, string id, bool exclusive, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            locked = false;
            lockId = null;
            lockAge = TimeSpan.Zero;
            actionFlags = SessionStateActions.None;
            var database = GetConnection().GetDatabase();
            var state = RedisSessionStateStore.Get(database, id);
            if (state == null)
                return null;
            state.UpdateExpire();
            return CreateLegitStoreData(context, state.GetSessionStateItemCollection(), null, (int)state.Timeout.TotalMinutes);
        }

        #endregion Private Method

        #region Help Class

        internal sealed class RedisSessionStateStore
        {
            #region Field

            private readonly IDatabase _database;
            private readonly string _id;

            #endregion Field

            #region Constructor

            private RedisSessionStateStore(IDatabase database, string id, TimeSpan timeout, string hashKey)
            {
                Timeout = timeout;
                HashKey = hashKey;
                _database = database;
                _id = id;
            }

            #endregion Constructor

            #region Property

            public TimeSpan Timeout { get; }
            public string HashKey { get; }

            #endregion Property

            #region Public Method

            public ISessionStateItemCollection GetSessionStateItemCollection()
            {
                var hash = _database.HashGetAll(HashKey);
                ISessionStateItemCollection collection = new SessionStateItemCollection();

                foreach (var entry in hash)
                {
                    var item = JsonConvert.DeserializeObject<DataItem>(entry.Value);
                    collection[entry.Name] = item?.Deserialize();
                }
                return collection;
            }

            public void Set(ISessionStateItemCollection sessionStateItemCollection)
            {
                _database.KeyDelete(HashKey);
                if (sessionStateItemCollection == null)
                    return;
                foreach (string key in sessionStateItemCollection.Keys)
                {
                    var item = sessionStateItemCollection[key];
                    _database.HashSet(HashKey, key, item == null ? null : JsonConvert.SerializeObject(new DataItem(item)));
                }

                UpdateExpire();
            }

            public void UpdateExpire()
            {
                UpdateExpire(_database, _id, Timeout);
            }

            #endregion Public Method

            #region Public Static Method

            public static RedisSessionStateStore Create(IDatabase database, string id, int timeout)
            {
                var timeoutSpan = TimeSpan.FromMinutes(timeout);
                database.StringSet(id, JsonConvert.SerializeObject(new { Timeout = timeout }), timeoutSpan);

                var hashKey = GetHashKey(id);
                var entrys = database.HashGetAll(hashKey);
                foreach (var entry in entrys)
                    database.HashDelete(hashKey, entry.Name);

                return new RedisSessionStateStore(database, id, TimeSpan.FromMinutes(timeout), hashKey);
            }

            public static RedisSessionStateStore Get(IDatabase database, string id)
            {
                var json = database.StringGet(id);

                int timeout;

                try
                {
                    var jobject = JObject.Parse(json);
                    timeout = jobject.Value<int>("Timeout");
                }
                catch (Exception)
                {
                    timeout = 20;
                }

                return new RedisSessionStateStore(database, id, TimeSpan.FromMinutes(timeout), GetHashKey(id));
            }

            public static void Delete(IDatabase database, string id)
            {
                database.KeyDelete(new RedisKey[] { id, GetHashKey(id) });
            }

            public static void UpdateExpire(IDatabase database, string id, TimeSpan timeout)
            {
                var keys = new[] { id, GetHashKey(id) };
                foreach (var key in keys.Where(key => database.KeyExists(key)))
                {
                    database.KeyExpire(key, timeout);
                }
            }

            private static string GetHashKey(string id)
            {
                return "Hash_" + id;
            }

            #endregion Public Static Method

            #region Help Class

            internal sealed class DataItem
            {
                public DataItem()
                {
                }

                public DataItem(object value)
                {
                    if (value == null)
                        return;
                    Json = JsonConvert.SerializeObject(value);
                    var type = value.GetType();
                    AssemblyQualifiedName = type.AssemblyQualifiedName;
                }

                /// <summary>
                /// 对象Json序列化后的内容。
                /// </summary>
                public string Json { get; set; }

                /// <summary>
                /// 对象类型的程序集限定名。
                /// </summary>
                public string AssemblyQualifiedName { get; set; }

                /// <summary>
                /// 反序列化。
                /// </summary>
                /// <returns>对象实例。</returns>
                public object Deserialize()
                {
                    if (string.IsNullOrEmpty(AssemblyQualifiedName) || string.IsNullOrEmpty(Json))
                        return null;

                    try
                    {
                        return JsonConvert.DeserializeObject(Json, Type.GetType(AssemblyQualifiedName));
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine($"反序列化Session中的对象发送了错误，{exception.Message}。");
                        return null;
                    }
                }
            }

            #endregion Help Class
        }

        #endregion Help Class
    }
}