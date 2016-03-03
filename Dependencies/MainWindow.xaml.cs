/*
    PROG8010 Group 2, Final Assignment: Dependency Manager
    Julia Aryal Sharma, 7375934
    Oscar Lucero, 7177884
    Kunal Ruparelia, 7128416    
    Charles Troster, 7388085
 */

using Microsoft.Win32;
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

namespace Dependencies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel vm;
        public MainWindow()
        {
            vm = new ViewModel();
            InitializeComponent();

            DataContext = vm;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                vm.ReadFile(ofd.FileName);
            }
        }
    }
}
