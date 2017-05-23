using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    /// <summary>
    /// 缓存失效规则提供者
    /// <para>（生命周期：全局单例）</para>
    /// </summary>
    public interface ISignals : IVolatileProvider
    {
        /// <summary>
        /// 触发使缓存失效（设置IsCurrent为false）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        void Trigger<T>(T signal);
        /// <summary>
        /// 创建缓存失效规则
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <returns></returns>
        IVolatileToken When<T>(T signal);
    }

    /// <summary>
    /// 缓存失效规则提供者
    /// <para>（生命周期：全局单例）</para>
    /// </summary>
    public class Signals : ISignals
    {
        readonly IDictionary<object, Token> _tokens = new Dictionary<object, Token>();
        /// <summary>
        /// 触发使缓存失效（设置IsCurrent为false）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        public void Trigger<T>(T signal)
        {
            lock (_tokens)
            {
                Token token;
                if (_tokens.TryGetValue(signal, out token))
                {
                    _tokens.Remove(signal);
                    token.Trigger();
                }
            }

        }
        /// <summary>
        /// 创建缓存失效规则
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <returns></returns>
        public IVolatileToken When<T>(T signal)
        {
            lock (_tokens)
            {
                Token token;
                if (!_tokens.TryGetValue(signal, out token))
                {
                    token = new Token();
                    _tokens[signal] = token;
                }
                return token;
            }
        }

        class Token : IVolatileToken
        {
            public Token()
            {
                IsCurrent = true;
            }
            public bool IsCurrent { get; private set; }
            public void Trigger() { IsCurrent = false; }
        }
    }
}
