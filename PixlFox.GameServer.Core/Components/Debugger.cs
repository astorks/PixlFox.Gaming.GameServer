using PixlFox.Gaming.GameServer.Commands;
using PixlFox.Gaming.GameServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer.Components
{
    public class Debugger : IGameComponent
    {
        private int currentTick = 0;
        private double[] deltaTimes;
        private int tickRate;

        private Dictionary<string, object> Variables { get; } = new Dictionary<string, object>();
        private Core gameCore;

        public void Initialize(Core gameCore)
        {
            this.gameCore = gameCore;
            Console.Title = gameCore.ServerName + " - Starting";
            tickRate = gameCore.TickRate;

            deltaTimes = new double[tickRate];
        }

        public void Shutdown()
        {
            Console.Title = gameCore.ServerName + " - Shutting Down";
        }

        public void Tick(double deltaTime)
        {
            deltaTimes[currentTick] = deltaTime;
            currentTick++;

            if (currentTick == (tickRate))
            {
                var averageDeltaTime = deltaTimes.Average();
                Console.Title = string.Format("{0} - Online - TPS: {1} - DeltaTime: {2}", gameCore.ServerName, Math.Round(1.0d / averageDeltaTime), averageDeltaTime);
                currentTick = 0;
            }
        }

        [RegisteredCommand("tps")]
        private int TPS()
        {
            return (int)Math.Round(1.0d / deltaTimes.Average());
        }

        [RegisteredCommand("deltaTime")]
        private double DeltaTime()
        {
            return deltaTimes.Average();
        }

        public void SetVariable(string name, object value)
        {
            if (Variables.ContainsKey(name))
                Variables[name] = value;
            else
                Variables.Add(name, value);
        }

        public void GetVariable(string name, object value)
        {
            if (Variables.ContainsKey(name))
                Variables[name] = value;
            else
                Variables.Add(name, value);
        }
    }
}
