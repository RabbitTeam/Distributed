# 分布式解决方案
为应用程序提供分布式部署的相关支持组件。

### 现有组件：
1. Distributed.SessionProvider.Redis（基于Redis的Session共享组件）
2. Distributed.MessageQueue（一个抽象的消息队列，集成了Aliyun ONS <阿里云开放消息服务>）
3. Distributed.SessionProvider.Memcached（基于Memcached <兼容阿里云OCS>的Session共享组件）
4. Distributed.Utility（分布式相关工具，包含了一个简单的分布式锁实现）
