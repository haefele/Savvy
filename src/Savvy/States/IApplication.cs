using Caliburn.Micro;
using Savvy.Views.Shell;

namespace Savvy.States
{
    public interface IApplication
    {
        ApplicationState CurrentState { get; set; }

        BindableCollection<NavigationItemViewModel> Actions { get; }
        BindableCollection<NavigationItemViewModel> SecondaryActions { get; }
    }
}