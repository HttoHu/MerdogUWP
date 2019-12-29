using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Merdog_Windows.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MerdogConsole : Page
    {
        public MerdogConsole()
        {
            this.InitializeComponent();
            DisPlayBox.Text += Classes.data.result;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButton.IsEnabled = this.Frame.CanGoBack;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Classes.data.result = "";
            DisPlayBox.Text = "";
        }
    }
}
