using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Property)]
    [Obsolete("PixlFox.Gaming.GameServer.DependencyInjection.GameComponentDependencyAttribute is deprecated, please use PixlFox.Gaming.GameServer.Attributes.InjectAttribute instead.")]
    public class GameComponentDependencyAttribute : Attribute
    {
        public GameComponentDependencyAttribute() { }
    }
}
