using System;

namespace Maple.Web.FileSystems.LockFile
{
    public interface ILockFile : IDisposable {
        void Release();
    }
}
