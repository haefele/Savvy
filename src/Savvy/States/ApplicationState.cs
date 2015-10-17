namespace Savvy.States
{
    public abstract class ApplicationState
    {
        public IApplication Application { get; set; }

        public abstract void Enter();
        public abstract void Leave();
    }
}