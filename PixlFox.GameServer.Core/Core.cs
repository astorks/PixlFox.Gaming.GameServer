using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

using PixlFox.Gaming.GameServer.Interfaces;
using PixlFox.Gaming.GameServer.Commands;
using PixlFox.Gaming.GameServer.DependencyInjection;
using System.Threading;
using System.Reflection;
using NLog;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace PixlFox.Gaming.GameServer
{
    public class Core
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public bool IsRunning { get; private set; }
        public bool IsFullyShutdown { get; private set; }
        private int tickDelay;
        public int TickRate { get; private set; }
        private List<IGameComponent> registeredGameComponents;
        private List<IGameService> registeredGameServices;
        private Stopwatch tickClock;
        private Stopwatch deltaClock;
        private Multimedia.Timer mediaTimer;
        private Thread commandInputThread;
        public Jint.Engine CommandEngine { get; private set; }
        public string ServerName { get; private set; }

        private Dictionary<string, CommandDescriptionInfo> commandDescriptions = new Dictionary<string, CommandDescriptionInfo>();

        /// <summary>
        /// Creates a new empty game server.
        /// </summary>
        /// <param name="tickRate">Number of tick executions per second (20-200).</param>
        public Core(string serverName, int tickRate, bool allowCommandEngineClr = false)
        {
            ServerName = serverName;
            //var externalPublicKey = Assembly.GetEntryAssembly().GetName().GetPublicKey();
            //var internalPublicKey = Assembly.GetExecutingAssembly().GetName().GetPublicKey();

            Console.Title = "PixlFox Game Server Core - Waiting";
            Console.WriteLine("PixlFox Game Server Core - Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("CONFIDENTIAL - INTERNAL BUILD - CONFIDENTIAL");
            Console.WriteLine("Copyright 2017 PixlFox LLC");

#if INTERNAL
            if (!internalPublicKey.SequenceEqual(externalPublicKey))
            {
                Console.Title = "PixlFox Game Server Core - Error";
                logger.Fatal("PUBLIC KEY IS INVALID.");
                logger.Fatal("Failed to start PixlFox Game Server Core.");
                Console.ReadLine();
                Environment.Exit(0);
            }            
            
            Console.Write("Do you agree to the EULA? (y/n): ");
            string termsAgree = Console.ReadLine();


            if (!termsAgree.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                Environment.Exit(0);
#endif

            Console.WriteLine();
            Console.WriteLine();

            logger.Info("Initializing PixlFox Game Server Core...");

            if (tickRate > 200 || tickRate < 20)
                throw new ArgumentOutOfRangeException("tickRate", "The tickrate must be between 20 and 200.");

            this.IsRunning = true;
            this.TickRate = tickRate;
            this.tickDelay = 1000 / tickRate;
            this.registeredGameComponents = new List<IGameComponent>();
            this.registeredGameServices = new List<IGameService>();
            this.tickClock = new Stopwatch();
            this.deltaClock = new Stopwatch();

            if(allowCommandEngineClr)
                this.CommandEngine = new Jint.Engine(cfg => cfg.AllowClr());
            else
                this.CommandEngine = new Jint.Engine();

            this.RegisterComponent<Components.Debugger>();
            this.RegisterCommandHandler("describeCommand", new Func<string, object>((commandName) => DescribeCommand(commandName)));
            this.RegisterCommandHandler("listCommands", new Func<object>(() => ListCommands()));
        }

        public object DescribeCommand(string commandName = null)
        {
            if (commandDescriptions.ContainsKey(commandName))
                return commandDescriptions[commandName];

            return null;
        }

        public string[] ListCommands()
        {
            return commandDescriptions.Keys.ToArray();
        }

        public T RegisterComponent<T>() where T : IGameComponent
        {
            var gameComponent = Activator.CreateInstance<T>();
            registeredGameComponents.Add(gameComponent);
            return gameComponent;
        }

        public T RegisterComponent<T>(T gameComponent) where T : IGameComponent
        {
            registeredGameComponents.Add(gameComponent);
            return gameComponent;
        }

        public T GetComponent<T>() where T : IGameComponent
        {
            return (T)GetComponent(typeof(T));
        }

        public object GetComponent(Type gameComponentType)
        {
            var gameComponent = registeredGameComponents.Where(e => e.GetType() == gameComponentType).FirstOrDefault();

            if (gameComponent == null)
                throw new Exception(string.Format("{0} is not registered as a component, did you forget to call GameServer.RegisterComponent<{0}>()", gameComponentType.Name));

            return gameComponent;
        }

        public T RegisterService<T>() where T : IGameService
        {

            var gameService = Activator.CreateInstance<T>();
            registeredGameServices.Add(gameService);
            return gameService;
        }

        public T RegisterService<T>(T gameService) where T : IGameService
        {
            registeredGameServices.Add(gameService);
            return gameService;
        }

        public T GetService<T>() where T : IGameService
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type gameServiceType)
        {
            var gameService = registeredGameServices.Where(e => e.GetType() == gameServiceType).FirstOrDefault();

            if (gameService == null)
                throw new Exception(string.Format("{0} is not registered as a service, did you forget to call GameServer.RegisterService<{0}>()", gameServiceType.Name));

            return gameService;
        }

        public void RegisterCommandHandler(string name, Delegate handler, CommandDescriptionInfo descriptionInfo = null)
        {
            this.CommandEngine.SetValue(name, handler);

            if (descriptionInfo != null)
                this.commandDescriptions.Add(name, descriptionInfo);
        }

        public object ExecuteCommand(string command)
        {
            command = command.Trim();

            try
            {
                if (command.StartsWith("?"))
                {
                    if(command == "?")
                        return string.Join(", ", ListCommands());

                    command = command.Substring(1);

                    if (commandDescriptions.ContainsKey(command))
                        return commandDescriptions[command].ToString();

                    return string.Format("Unable to find help for command '{0}'", command);
                }
                else if(commandDescriptions.ContainsKey(command) && commandDescriptions[command].Parameters.Count == 0)
                {
                    command = command + "();";
                }

                var completionValue = this.CommandEngine.Execute(command).GetCompletionValue();
                return completionValue.ToObject();
            }
            catch { return null; }
        }

        public void Start()
        {
            logger.Info("Starting PixlFox Game Server Core...");
            Initalize();
            logger.Info("PixlFox Game Server Core is online.");
            Tick();

            IsFullyShutdown = false;
        }

        public void StartCommandInputThread(bool useExperimentalHinter = false)
        {
            commandInputThread = new Thread(new ThreadStart(() =>
            {
                while (IsRunning)
                {
                    string commandInput = null;

                    if (useExperimentalHinter)
                        commandInput = HinterLib.Hinter.ReadHintedLine(commandDescriptions.Keys, cmd => cmd, linePrefix: "> ");
                    else
                    {
                        Console.Write("> ");
                        commandInput = Console.ReadLine();
                    }

                    var commandOutput = ExecuteCommand(commandInput);
                    if (commandOutput != null)
                    {
                        if (commandOutput is string)
                            Console.WriteLine("< {0}", commandOutput);
                        else
                        {
                            var commandOutputJson = JsonConvert.SerializeObject(commandOutput, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            Console.WriteLine("< {0}", commandOutputJson);
                        }
                    }
                }
            }));
            commandInputThread.Start();
        }

        public void Shutdown()
        {
            IsRunning = false;
            if (commandInputThread != null)
                commandInputThread.Abort();
        }

        private void Initalize()
        {
            logger.Info("Initializing game services...");
            foreach (var gameService in registeredGameServices)
            {
                var logger = LogManager.GetLogger(gameService.GetType().FullName, gameService.GetType());

                logger.Debug("Injecting dependencies...");
                InjectionManager.InjectDependenciesIntoIGameService(this, gameService);
                logger.Debug("Initializing...");
                gameService.Initialize(this);
                logger.Debug("Registering commands...");
                CommandManager.RegisterCommandsForIGameService(this, gameService);
                logger.Debug("Initialization complete.");
            }

            logger.Info("Initalizing game components...");
            foreach (var gameComponent in registeredGameComponents)
            {
                var logger = LogManager.GetLogger(gameComponent.GetType().FullName, gameComponent.GetType());

                logger.Debug("Injecting dependencies...");
                InjectionManager.InjectDependenciesIntoIGameComponent(this, gameComponent);
                logger.Debug("Initializing...");
                gameComponent.Initialize(this);
                logger.Debug("Registering commands...");
                CommandManager.RegisterCommandsForIGameComponent(this, gameComponent);
                logger.Debug("Initialization complete.");
            }
        }

        private void CleanUp()
        {
            registeredGameComponents.Reverse();
            registeredGameServices.Reverse();

            logger.Info("Cleaning up game components...");
            foreach (var gameComponent in registeredGameComponents)
            {
                var logger = LogManager.GetLogger(gameComponent.GetType().FullName, gameComponent.GetType());
                logger.Debug("Shutting down...");
                gameComponent.Shutdown();
                logger.Debug("Shutdown complete.");
            }

            logger.Info("Cleaning up game services...");
            foreach (var gameService in registeredGameServices)
            {
                var logger = LogManager.GetLogger(gameService.GetType().FullName, gameService.GetType());
                logger.Debug("Shutting down...", gameService.GetType().FullName);
                gameService.Shutdown();
                logger.Debug("Shutdown complete.");
            }

            IsFullyShutdown = true;
        }

        private void Tick()
        {
            var deltaTime = ((double)deltaClock.ElapsedMilliseconds / 1000d);
            deltaClock.Restart();
            tickClock.Restart();

            for (var i = 0; i < registeredGameComponents.Count; i++)
                registeredGameComponents[i].Tick(deltaTime);

            var elapsedExecutionTime = (int)tickClock.ElapsedMilliseconds;
            var delayTime = tickDelay - elapsedExecutionTime;

            if (IsRunning)
            {
                if (delayTime <= Multimedia.Timer.Capabilities.periodMin)
                {
                    Tick();
                }
                else
                {
                    mediaTimer = new Multimedia.Timer()
                    {
                        Period = tickDelay - elapsedExecutionTime,
                        Mode = Multimedia.TimerMode.OneShot
                    };
                    mediaTimer.Tick += (s, e) => Tick();
                    mediaTimer.Start();
                }
            }
            else
            {
                this.CleanUp();
            }
        }
    }
}
