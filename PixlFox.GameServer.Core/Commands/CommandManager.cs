using NLog;
using PixlFox.Gaming.GameServer.Attributes;
using PixlFox.Gaming.GameServer.Interfaces;
using PixlFox.Gaming.GameServer.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer.Commands
{
    class CommandManager
    {
        public static void RegisterCommandsForIGameService(Core gameCore, IGameService gameService)
        {
            var delegateTypeFactory = DelegateTypeFactory.Instance;
            var gameServiceType = gameService.GetType();
            var logger = LogManager.GetLogger(gameServiceType.FullName, gameServiceType);

            foreach (var methodInfo in gameServiceType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var registeredCommandAttribute = methodInfo.GetCustomAttribute<RegisteredCommandAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete
                var commandAttribute = methodInfo.GetCustomAttribute<CommandAttribute>();

                if (commandAttribute != null)
                {
                    var commandDescriptionInfo = new CommandDescriptionInfo()
                    {
                        Name = commandAttribute.Name,
                        Description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description,
                        ReturnType = methodInfo.ReturnType?.Name
                    };
                    foreach (var paramInfo in methodInfo.GetParameters())
                        commandDescriptionInfo.Parameters.Add(new CommandParamDescriptionInfo
                        {
                            Name = paramInfo.Name,
                            ValueType = paramInfo.ParameterType.Name,
                            Description = paramInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                        });

                    var delegateType = delegateTypeFactory.CreateDelegateType(methodInfo);
                    var commandDelegate = Delegate.CreateDelegate(delegateType, gameService, methodInfo.Name);

                    gameCore.RegisterCommandHandler(commandAttribute.Name, commandDelegate, commandDescriptionInfo);
                    logger.Trace("Registered command {0} => {1}({2})", commandAttribute.Name, methodInfo.Name, string.Join(", ", commandDescriptionInfo.Parameters.Select(e => e.ValueType)));
                }
                else if (registeredCommandAttribute != null)
                {
                    var commandDescriptionInfo = new CommandDescriptionInfo()
                    {
                        Name = registeredCommandAttribute.Name,
                        Description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description,
                        ReturnType = methodInfo.ReturnType?.Name
                    };
                    foreach (var paramInfo in methodInfo.GetParameters())
                        commandDescriptionInfo.Parameters.Add(new CommandParamDescriptionInfo
                        {
                            Name = paramInfo.Name,
                            ValueType = paramInfo.ParameterType.Name,
                            Description = paramInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                        });

                    var delegateType = delegateTypeFactory.CreateDelegateType(methodInfo);
                    var commandDelegate = Delegate.CreateDelegate(delegateType, gameService, methodInfo.Name);

                    gameCore.RegisterCommandHandler(registeredCommandAttribute.Name, commandDelegate, commandDescriptionInfo);
                    logger.Trace("Registered command {0} => {1}({2})", registeredCommandAttribute.Name, methodInfo.Name, string.Join(", ", commandDescriptionInfo.Parameters.Select(e => e.ValueType)));
                }
            }
        }

        public static void RegisterCommandsForIGameComponent(Core gameCore, IGameComponent gameComponent)
        {
            var delegateTypeFactory = DelegateTypeFactory.Instance;
            var gameComponentType = gameComponent.GetType();
            var logger = LogManager.GetLogger(gameComponentType.FullName, gameComponentType);

            foreach (var methodInfo in gameComponentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var registeredCommandAttribute = methodInfo.GetCustomAttribute<RegisteredCommandAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete
                var commandAttribute = methodInfo.GetCustomAttribute<CommandAttribute>();

                if(commandAttribute != null)
                {
                    var commandDescriptionInfo = new CommandDescriptionInfo()
                    {
                        Name = commandAttribute.Name,
                        Description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description,
                        ReturnType = methodInfo.ReturnType?.Name
                    };
                    foreach (var paramInfo in methodInfo.GetParameters())
                        commandDescriptionInfo.Parameters.Add(new CommandParamDescriptionInfo
                        {
                            Name = paramInfo.Name,
                            ValueType = paramInfo.ParameterType.Name,
                            Description = paramInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                        });

                    var delegateType = delegateTypeFactory.CreateDelegateType(methodInfo);
                    var commandDelegate = Delegate.CreateDelegate(delegateType, gameComponent, methodInfo.Name);

                    gameCore.RegisterCommandHandler(commandAttribute.Name, commandDelegate, commandDescriptionInfo);
                    logger.Trace("Registered command {0} => {1}({2})", commandAttribute.Name, methodInfo.Name, string.Join(", ", commandDescriptionInfo.Parameters.Select(e => e.ValueType)));
                }
                else if (registeredCommandAttribute != null)
                {
                    var commandDescriptionInfo = new CommandDescriptionInfo()
                    {
                        Name = registeredCommandAttribute.Name,
                        Description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description,
                        ReturnType = methodInfo.ReturnType?.Name
                    };
                    foreach (var paramInfo in methodInfo.GetParameters())
                        commandDescriptionInfo.Parameters.Add(new CommandParamDescriptionInfo
                        {
                            Name = paramInfo.Name,
                            ValueType = paramInfo.ParameterType.Name,
                            Description = paramInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                        });

                    var delegateType = delegateTypeFactory.CreateDelegateType(methodInfo);
                    var commandDelegate = Delegate.CreateDelegate(delegateType, gameComponent, methodInfo.Name);

                    gameCore.RegisterCommandHandler(registeredCommandAttribute.Name, commandDelegate, commandDescriptionInfo);
                    logger.Trace("Registered command {0} => {1}({2})", registeredCommandAttribute.Name, methodInfo.Name, string.Join(", ", commandDescriptionInfo.Parameters.Select(e => e.ValueType)));
                }
            }
        }
    }

    public class CommandDescriptionInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReturnType { get; set; }
        public List<CommandParamDescriptionInfo> Parameters { get; set; } = new List<CommandParamDescriptionInfo>();

        public override string ToString()
        {
            return string.Format(@"
------------------------
{0} {1}({3})
- Description: {2}
------------------------
Parameters:
{4}
------------------------", ReturnType, Name, Description, string.Join(", ", Parameters.Select(e => e.ValueType + " " + e.Name)), string.Join("\r\n\r\n", Parameters.Select(e => e.ToString())));
        }
    }

    public class CommandParamDescriptionInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueType { get; set; }

        public override string ToString()
        {
            return string.Format(@"{0} {1}
- Description: {2}", ValueType, Name, Description);
        }
    }
}
