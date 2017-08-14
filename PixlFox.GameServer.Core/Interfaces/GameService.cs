using NLog;
using PixlFox.Gaming.GameServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer
{
    public class GameService : IGameService
    {
        public readonly Logger logger;

        public GameService()
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
    }
}
