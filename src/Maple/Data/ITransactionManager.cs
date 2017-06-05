using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Data
{
    public interface ITransactionManager
    {
        /// <summary>
        ///  开启数据库事务
        /// </summary>
        void BeginTransaction();
        /// <summary>
        /// 提交数据库事务
        /// </summary>
        void Commit();
        /// <summary>
        /// 回滚数据库事务
        /// </summary>
        void Rollback();
        /// <summary>
        /// 回滚数据库事务
        /// </summary>
        /// <param name="RollBackException">异常信息</param>
        void Rollback(Exception RollBackException);
    }
}
