using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;

namespace Savvy.Views.Shell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellView : Page
    {
        public ShellView()
        {
            this.InitializeComponent();
        }

        public ShellViewModel ViewModel => this.DataContext as ShellViewModel;

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (NavigationItemViewModel)e.ClickedItem;
            clickedItem.Execute();

            this.Navigation.IsPaneOpen = false;
        }

        private void OpenNavigationView(object sender, RoutedEventArgs e)
        {
            this.Navigation.IsPaneOpen = true;
        }
    }
}
