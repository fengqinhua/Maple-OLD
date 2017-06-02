using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Web.Security
{
    /// <summary>
    /// Interface provided by the "User" model. 
    /// </summary>
    public interface IUser 
    {
        string UserName { get; }
        string Email { get; }
    }
}
