using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleDetector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Page1 parityPage;
        Page2 crcPage;
        Page3 hammingPage;
        public MainWindow()
        {
            InitializeComponent();
            parityPage = new Page1();
            crcPage = new Page2();
            hammingPage = new Page3();
            Main.Content = parityPage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = parityPage;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = crcPage;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = hammingPage;
        }
    }
}
