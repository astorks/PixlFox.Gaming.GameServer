namespace PixlFox.Gaming.GameServer.Interfaces
{
    public interface IGameComponent
    {
        void Initialize(Core gameCore);
        void Shutdown();
        void Tick(double deltaTime);
    }
}
