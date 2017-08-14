using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Property)]
    [Obsolete("PixlFox.Gaming.GameServer.DependencyInjection.GameServiceDependencyAttribute is deprecated, please use PixlFox.Gaming.GameServer.Attributes.InjectAttribute instead.")]
    public class GameServiceDependencyAttribute : Attribute
    {
        public GameServiceDependencyAttribute() { }
    }
}
