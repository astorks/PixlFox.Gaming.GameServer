using NLog;
using PixlFox.Gaming.GameServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer
{
    public class GameComponent : IGameComponent
    {
        public readonly Logger logger;

        public GameComponent()
        {
            var type = this.GetType();
            logger = LogManager.GetLogger(type.FullName);
        }

        public virtual void Initialize(Core gameCore)
        {
            
        }

        public virtual void Shutdown()
        {
            
        }

        public virtual void Tick(double deltaTime)
        {
            
        }
    }
}
