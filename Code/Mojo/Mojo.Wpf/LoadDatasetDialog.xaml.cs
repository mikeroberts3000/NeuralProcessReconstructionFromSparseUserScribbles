using System.Windows;

namespace Mojo.Wpf
{
    /// <summary>
    /// Interaction logic for LoadDatasetDialog.xaml
    /// </summary>
    public partial class LoadDatasetDialog : Window
    {
        public LoadDatasetDialog()
        {
            InitializeComponent();
        }

        private void OkClick( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
        }
    }
}
