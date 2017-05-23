using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    /// <summary>
    /// 弱引用缓存内。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Weak<T>
    {
        private readonly WeakReference _target;

        public Weak(T target)
        {
            _target = new WeakReference(target);
        }

        public Weak(T target, bool trackResurrection)
        {
            _target = new WeakReference(target, trackResurrection);
        }

        public T Target
        {
            get { return (T)_target.Target; }
            set { _target.Target = value; }
        }
    }
}
