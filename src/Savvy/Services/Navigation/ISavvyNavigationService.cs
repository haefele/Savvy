using Caliburn.Micro;

namespace Savvy.Services.Navigation
{
    public interface ISavvyNavigationService
    {
        NavigateHelper<TViewModel> For<TViewModel>();
        void ClearBackStack();
    }
}