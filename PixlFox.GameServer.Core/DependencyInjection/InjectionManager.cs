using PixlFox.Gaming.GameServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NLog;
using PixlFox.Gaming.GameServer.Attributes;

namespace PixlFox.Gaming.GameServer.DependencyInjection
{
    class InjectionManager
    {
        public static void InjectDependenciesIntoIGameService(Core gameCore, IGameService gameService)
        {
            var gameServiceType = gameService.GetType();
            var logger = LogManager.GetLogger(gameServiceType.FullName, gameServiceType);

            foreach (var propertyInfo in gameServiceType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var gameComponentDependencyAttribute = propertyInfo.GetCustomAttribute<GameComponentDependencyAttribute>();
                var gameServiceDependencyAttribute = propertyInfo.GetCustomAttribute<GameServiceDependencyAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete
                var injectAttribute = propertyInfo.GetCustomAttribute<InjectAttribute>();
                var setMethod = propertyInfo.GetSetMethod(true);
                var propertyType = propertyInfo.PropertyType;

                if(injectAttribute != null && setMethod != null)
                {
                    if(propertyType.GetInterfaces().Contains(typeof(IGameComponent)))
                    {
                        setMethod.Invoke(gameService, new object[] { gameCore.GetComponent(propertyType) });
                        logger.Trace("Injected {0} => {1}.", propertyType.FullName, propertyInfo.Name);
                    }
                    else if(propertyType.GetInterfaces().Contains(typeof(IGameService)))
                    {
                        setMethod.Invoke(gameService, new object[] { gameCore.GetService(propertyType) });
                        logger.Trace("Injected {0} => {1}.", propertyType.FullName, propertyInfo.Name);
                    }
                    else
                    {
                        logger.Error("Failed to injected {0} into {1}.", propertyType.FullName, propertyInfo.Name);
                    }
                }
                else if (gameComponentDependencyAttribute != null && setMethod != null)
                {
                    setMethod.Invoke(gameService, new object[] { gameCore.GetComponent(propertyType) });
                    logger.Trace("Injected {0} => {1}.", propertyType.FullName, propertyInfo.Name);
                }
                else if (gameServiceDependencyAttribute != null && setMethod != null)
                {
                    setMethod.Invoke(gameService, new object[] { gameCore.GetService(propertyType) });
                    logger.Trace("Injected {0} => {1}.", propertyType.FullName, propertyInfo.Name);
                }
            }
        }

        public static void InjectDependenciesIntoIGameComponent(Core gameCore, IGameComponent gameComponent)
        {
            var gameComponentType = gameComponent.GetType();
            var logger = LogManager.GetLogger(gameComponentType.FullName, gameComponentType);

            foreach (var propertyInfo in gameComponentType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var gameComponentDependencyAttribute = propertyInfo.GetCustomAttribute<GameComponentDependencyAttribute>();
                var gameServiceDependencyAttribute = propertyInfo.GetCustomAttribute<GameServiceDependencyAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete
                var injectAttribute = propertyInfo.GetCustomAttribute<InjectAttribute>();
                var setMethod = propertyInfo.GetSetMethod(true);
                var propertyType = propertyInfo.PropertyType;

                if (injectAttribute != null && setMethod != null)
                {
                    if (propertyType.GetInterfaces().Contains(typeof(IGameComponent)))
                    {
                        setMethod.Invoke(gameComponent, new object[] { gameCore.GetComponent(propertyType) });
                        logger.Trace("Injected {0} => {1}.", propertyType.FullName, propertyInfo.Name);
                    }
                    else if (propertyType.GetInterfaces().Contains(typeof(IGameService)))
                    {
                        setMethod.Invoke(gameComponent, new object[] { gameCore.GetService(propertyType) });
                        logger.Trace("Injected {0} => {1}.", propertyType.FullName, propertyInfo.Name);
                    }
                    else
                    {
                        logger.Error("Failed to injected {0} into {1}.", propertyType.FullName, propertyInfo.Name);
                    }
                }
                else if (gameComponentDependencyAttribute != null && setMethod != null)
                {
                    setMethod.Invoke(gameComponent, new object[] { gameCore.GetComponent(propertyType) });
                    logger.Trace("Injected {0} => {1}.", propertyType.FullName, propertyInfo.Name);
                }
                else if (gameServiceDependencyAttribute != null && setMethod != null)
                {
                    setMethod.Invoke(gameComponent, new object[] { gameCore.GetService(propertyType) });
                    logger.Trace("Injected {0} => {1}.", propertyType.FullName, propertyInfo.Name);
                }
            }
        }
    }
}
