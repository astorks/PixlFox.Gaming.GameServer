using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    [Obsolete("PixlFox.Gaming.GameServer.Commands.RegisteredCommandAttribute is deprecated, please use PixlFox.Gaming.GameServer.Attributes.Command instead.")]
    public class RegisteredCommandAttribute : Attribute
    {
        public string Name { get; }

        public RegisteredCommandAttribute(string name)
        {
            Name = name;
        }
    }
}
