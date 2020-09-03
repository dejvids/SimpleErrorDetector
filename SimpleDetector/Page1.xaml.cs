using SimpleDetector.Models;
using SimpleDetector.ViewModels;
using System.Windows.Controls;

namespace SimpleDetector
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        ParityViewModel ViewModel;
        public Page1()
        {
            InitializeComponent();
            ViewModel = new ParityViewModel();
            this.DataContext = ViewModel;
        }

        private void ListBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DisturbedBits.SelectedIndex = -1;
        }

        private void OriginTxb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
