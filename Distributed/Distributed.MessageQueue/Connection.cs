using System;
using System.Threading.Tasks;

namespace Distributed.MessageQueue
{
    public interface IConnection : IDisposable
    {
        Task Start();

        Task Shutdown();
    }

    internal abstract class Connection : IConnection
    {
        #region Field

        private bool _isStart;
        private bool _isShutdown;

        #endregion Field

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            try
            {
                if (_isShutdown)
                {
                    lock (this)
                    {
                        if (_isShutdown)
                            return;
                        _isShutdown = true;
                    }
                }
                Shutdown();
            }
            finally
            {
                Dispose();
            }
        }

        #endregion Implementation of IDisposable

        #region Implementation of IConnection

        async Task IConnection.Start()
        {
            if (_isStart)
            {
                lock (this)
                {
                    if (_isStart)
                        return;
                    _isStart = true;
                }
            }
            await Task.Run(() => Start());
        }

        async Task IConnection.Shutdown()
        {
            if (_isShutdown)
            {
                lock (this)
                {
                    if (_isShutdown)
                        return;
                    _isShutdown = true;
                }
            }
            await Task.Run(() => Shutdown());
        }

        #endregion Implementation of IConnection

        #region Protected Abstract Method

        protected abstract void Start();

        protected abstract void Shutdown();

        protected abstract void Dispose();

        #endregion Protected Abstract Method
    }
}