using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.SessionState;

namespace Distributed.SessionProvider.Memcached
{
    public sealed class MemcachedSessionStateStoreProvider : SessionStateStoreProviderBase
    {
        #region Field

        private static readonly Lazy<MemcachedClient> MemcachedClient;
        private static readonly string SessionKey;

        #endregion Field

        #region Constructor

        static MemcachedSessionStateStoreProvider()
        {
            SessionKey = ConfigurationManager.AppSettings["MemcachedSessionKey"] ?? "MemcachedSession";
            MemcachedClient = new Lazy<MemcachedClient>(() =>
            {
                var configuration = new MemcachedClientConfiguration();
                configuration.AddServer(ConfigurationManager.AppSettings["MemcachedServerAddress"]);
                configuration.Protocol = MemcachedProtocol.Binary;
                return new MemcachedClient(configuration);
            });
        }

        #endregion Constructor

        #region Overrides of SessionStateStoreProviderBase

        /// <summary>
        /// 释放由 <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/> 实现使用的所有资源。
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// 设置对 Global.asax 文件中定义的 Session_OnEnd 事件的 <see cref="T:System.Web.SessionState.SessionStateItemExpireCallback"/> 委托的引用。
        /// </summary>
        /// <returns>
        /// 如果会话状态存储提供程序支持调用 Session_OnEnd 事件，则为 true；否则为 false。
        /// </returns>
        /// <param name="expireCallback">对 Global.asax 文件中定义的 Session_OnEnd 事件的 <see cref="T:System.Web.SessionState.SessionStateItemExpireCallback"/> 委托。</param>
        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return true;
        }

        /// <summary>
        /// 由 <see cref="T:System.Web.SessionState.SessionStateModule"/> 对象调用，以便进行每次请求初始化。
        /// </summary>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param>
        public override void InitializeRequest(HttpContext context)
        {
        }

        /// <summary>
        /// 从会话数据存储区中返回只读会话状态数据。
        /// </summary>
        /// <returns>
        /// 使用会话数据存储区中的会话值和信息填充的 <see cref="T:System.Web.SessionState.SessionStateStoreData"/>。
        /// </returns>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param><param name="id">当前请求的 <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"/>。</param><param name="locked">当此方法返回时，如果请求的会话项在会话数据存储区被锁定，请包含一个设置为 true 的布尔值；否则请包含一个设置为 false 的布尔值。</param><param name="lockAge">当此方法返回时，请包含一个设置为会话数据存储区中的项锁定时间的 <see cref="T:System.TimeSpan"/> 对象。</param><param name="lockId">当此方法返回时，请包含一个设置为当前请求的锁定标识符的对象。有关锁定标识符的详细信息，请参见 <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/> 类摘要中的“锁定会话存储区数据”。</param><param name="actions">当此方法返回时，请包含 <see cref="T:System.Web.SessionState.SessionStateActions"/> 值之一，指示当前会话是否为未初始化的无 Cookie 会话。</param>
        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId,
            out SessionStateActions actions)
        {
            return DoGet(context, id, false, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// 从会话数据存储区中返回只读会话状态数据。
        /// </summary>
        /// <returns>
        /// 使用会话数据存储区中的会话值和信息填充的 <see cref="T:System.Web.SessionState.SessionStateStoreData"/>。
        /// </returns>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param><param name="id">当前请求的 <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"/>。</param><param name="locked">当此方法返回时，如果成功获得锁定，请包含一个设置为 true 的布尔值；否则请包含一个设置为 false 的布尔值。</param><param name="lockAge">当此方法返回时，请包含一个设置为会话数据存储区中的项锁定时间的 <see cref="T:System.TimeSpan"/> 对象。</param><param name="lockId">当此方法返回时，请包含一个设置为当前请求的锁定标识符的对象。有关锁定标识符的详细信息，请参见 <see cref="T:System.Web.SessionState.SessionStateStoreProviderBase"/> 类摘要中的“锁定会话存储区数据”。</param><param name="actions">当此方法返回时，请包含 <see cref="T:System.Web.SessionState.SessionStateActions"/> 值之一，指示当前会话是否为未初始化的无 Cookie 会话。</param>
        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge,
            out object lockId, out SessionStateActions actions)
        {
            return DoGet(context, id, true, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// 释放对会话数据存储区中项的锁定。
        /// </summary>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param><param name="id">当前请求的会话标识符。</param><param name="lockId">当前请求的锁定标识符。</param>
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
        }

        /// <summary>
        /// 使用当前请求中的值更新会话状态数据存储区中的会话项信息，并清除对数据的锁定。
        /// </summary>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param><param name="id">当前请求的会话标识符。</param><param name="item">包含要存储的当前会话值的 <see cref="T:System.Web.SessionState.SessionStateStoreData"/> 对象。</param><param name="lockId">当前请求的锁定标识符。</param><param name="newItem">如果为 true，则将会话项标识为新项；如果为 false，则将会话项标识为现有的项。</param>
        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            new MemcachedSessionStateStore(MemcachedClient.Value).Set(id, item.Timeout, item.Items);
        }

        /// <summary>
        /// 删除会话数据存储区中的项数据。
        /// </summary>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param><param name="id">当前请求的会话标识符。</param><param name="lockId">当前请求的锁定标识符。</param><param name="item">表示将从数据存储区中删除的项的 <see cref="T:System.Web.SessionState.SessionStateStoreData"/>。</param>
        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            new MemcachedSessionStateStore(MemcachedClient.Value).Delete(id);
        }

        /// <summary>
        /// 更新会话数据存储区中的项的到期日期和时间。
        /// </summary>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param><param name="id">当前请求的会话标识符。</param>
        public override void ResetItemTimeout(HttpContext context, string id)
        {
        }

        /// <summary>
        /// 创建要用于当前请求的新 <see cref="T:System.Web.SessionState.SessionStateStoreData"/> 对象。
        /// </summary>
        /// <returns>
        /// 当前请求的新 <see cref="T:System.Web.SessionState.SessionStateStoreData"/>。
        /// </returns>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param><param name="timeout">新 <see cref="T:System.Web.SessionState.SessionStateStoreData"/> 的会话状态 <see cref="P:System.Web.SessionState.HttpSessionState.Timeout"/> 值。</param>
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return CreateLegitStoreData(context, null, null, timeout);
        }

        /// <summary>
        /// 将新的会话状态项添加到数据存储区中。
        /// </summary>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param><param name="id">当前请求的 <see cref="P:System.Web.SessionState.HttpSessionState.SessionID"/>。</param><param name="timeout">当前请求的会话 <see cref="P:System.Web.SessionState.HttpSessionState.Timeout"/>。</param>
        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
        }

        /// <summary>
        /// 在请求结束时由 <see cref="T:System.Web.SessionState.SessionStateModule"/> 对象调用。
        /// </summary>
        /// <param name="context">当前请求的 <see cref="T:System.Web.HttpContext"/>。</param>
        public override void EndRequest(HttpContext context)
        {
        }

        #endregion Overrides of SessionStateStoreProviderBase

        #region Private Method

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

            int timeout;
            var sessionStateItemCollection = new MemcachedSessionStateStore(MemcachedClient.Value).Get(id, out timeout);

            return CreateLegitStoreData(context, sessionStateItemCollection, null, timeout);
        }

        #endregion Private Method

        #region Help Class

        public sealed class MemcachedSessionStateStore
        {
            private readonly IMemcachedClient _client;

            public MemcachedSessionStateStore(IMemcachedClient client)
            {
                _client = client;
            }

            [Serializable]
            public sealed class SessionEntry
            {
                public SessionEntry() : this(20)
                {
                }

                public SessionEntry(int timeout)
                {
                    Timeout = timeout;
                    Dictionary = new Dictionary<string, string>();
                }

                public int Timeout { get; set; }
                public IDictionary<string, string> Dictionary { get; set; }
            }

            public sealed class DataItem
            {
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

                public string Serialize()
                {
                    return JsonConvert.SerializeObject(this);
                }

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

                public static DataItem Create(string json)
                {
                    return JsonConvert.DeserializeObject<DataItem>(json);
                }
            }

            public ISessionStateItemCollection Get(string id, out int timeout)
            {
                var sessionDictionary = GetDictionary();
                SessionEntry sessionEntry;
                ISessionStateItemCollection sessionStateItemCollection = new SessionStateItemCollection();
                if (sessionDictionary.TryGetValue(id, out sessionEntry))
                {
                    foreach (var entry in sessionEntry.Dictionary)
                    {
                        var item = DataItem.Create(entry.Value);
                        sessionStateItemCollection[entry.Key] = item.Deserialize();
                    }
                }
                else
                {
                    sessionEntry = new SessionEntry();
                }
                timeout = sessionEntry.Timeout;
                return sessionStateItemCollection;
            }

            private Dictionary<string, SessionEntry> GetDictionary()
            {
                return _client.Get<Dictionary<string, SessionEntry>>(SessionKey) ?? new Dictionary<string, SessionEntry>();
            }

            private void Save(Dictionary<string, SessionEntry> dictionary)
            {
                _client.Store(StoreMode.Set, SessionKey, dictionary, TimeSpan.FromMinutes(1));
            }

            public void Set(string id, int timeout, ISessionStateItemCollection sessionStateItemCollection)
            {
                var dictionary = GetDictionary();
                var entry = dictionary[id] = new SessionEntry(timeout);
                foreach (string key in sessionStateItemCollection.Keys)
                {
                    entry.Dictionary[key] = new DataItem(sessionStateItemCollection[key]).Serialize();
                }
                Save(dictionary);
            }

            public void Delete(string id)
            {
                var dictionary = GetDictionary();
                if (dictionary.ContainsKey(id))
                    dictionary.Remove(id);
                Save(dictionary);
            }
        }

        #endregion Help Class
    }
}