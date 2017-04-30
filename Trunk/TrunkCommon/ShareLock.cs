using System;
using System.Threading;

namespace TrunkCommon
{
    /// <summary>
    /// 共享锁v2
    /// </summary>
    public class ShareLock
    {
        ReaderWriterLockSlim Slim;
        IDisposable ExitReadLock, ExitWriteLock, ExitUpgradeableReadLock;

        public ShareLock()
        {
            this.Slim = new ReaderWriterLockSlim();
            this.ExitReadLock = new ReadLockDisposer(Slim);
            this.ExitWriteLock = new WriteLockDisposer(Slim);
            this.ExitUpgradeableReadLock = new UpgradeableReadDisposer(Slim);
        }

        public IDisposable ReadLock { get { this.Slim.EnterReadLock(); return ExitReadLock; } }

        public IDisposable WriteLock { get { this.Slim.EnterWriteLock(); return ExitWriteLock; } }

        public IDisposable UpgradeableReadLock { get { this.Slim.EnterUpgradeableReadLock(); return ExitUpgradeableReadLock; } }

        class LockDisposer
        {
            public ReaderWriterLockSlim Slim;
            public LockDisposer(ReaderWriterLockSlim slim) { this.Slim = slim; }
        }

        class ReadLockDisposer : LockDisposer, IDisposable
        {
            public ReadLockDisposer(ReaderWriterLockSlim slim) : base(slim) { }
            public void Dispose() { Slim.ExitReadLock(); }
        }

        class WriteLockDisposer : LockDisposer, IDisposable
        {
            public WriteLockDisposer(ReaderWriterLockSlim slim) : base(slim) { }
            public void Dispose() { Slim.ExitWriteLock(); }
        }

        class UpgradeableReadDisposer : LockDisposer, IDisposable
        {
            public UpgradeableReadDisposer(ReaderWriterLockSlim slim) : base(slim) { }
            public void Dispose() { Slim.ExitUpgradeableReadLock(); }
        }
    }
}
