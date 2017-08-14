using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
        public InjectAttribute() { }
    }
}
