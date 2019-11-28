using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mojo.Wpf
{
    /// <summary>
    /// Interaction logic for LoadSegmentationDialog.xaml
    /// </summary>
    public partial class LoadSaveSegmentationDialog : Window
    {
        public LoadSaveSegmentationDialog( string title, object colorImagesBindingSource, string colorImagesBindingPath, object idImagesBindingSource, string idImagesBindingPath )
        {
            InitializeComponent();

            var colorImagesBinding = new Binding
                                     {
                                         Path = new PropertyPath( colorImagesBindingPath ),
                                         Source = colorImagesBindingSource
                                     };

            var idImagesBinding = new Binding
                                  {
                                      Path = new PropertyPath( idImagesBindingPath ),
                                      Source = idImagesBindingSource
                                  };

            Title = title;
            ColorImages.SetBinding( TextBox.TextProperty, colorImagesBinding );
            IdImages.SetBinding( TextBox.TextProperty, idImagesBinding );
        }

        private void OkClick( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
        }
    }
}
