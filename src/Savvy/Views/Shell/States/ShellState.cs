namespace Savvy.Views.Shell.States
{
    public abstract class ShellState
    {
        public ShellViewModel ViewModel { get; set; }

        public abstract void Enter();
        public abstract void Leave();
    }
}