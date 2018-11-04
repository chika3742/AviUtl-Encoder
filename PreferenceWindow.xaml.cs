using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// PreferenceWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PreferenceWindow : Window
    {
        public PreferenceWindow()
        {
            InitializeComponent();

            aucPath.Text = Properties.Settings.Default.AUC_Path;
            aviutlPath.Text = Properties.Settings.Default.AviUtl_Path;
        }

        private void aviutlPathOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "aviutl.exe|aviutl.exe";


            if (dialog.ShowDialog() == true)
            {
                aviutlPath.Text = dialog.FileName;
            }
        }

        private void aucPathOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                aucPath.Text = dialog.FileName;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Topmost = true;
            Properties.Settings.Default.AUC_Path = aucPath.Text;
            Properties.Settings.Default.AviUtl_Path = aviutlPath.Text;
        }
    }

}
