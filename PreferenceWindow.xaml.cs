using Microsoft.VisualBasic;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            foreach (Label label in Properties.Settings.Default.Profile_Labels)
            {
                Prof_ComboBox.Items.Add(label);
            }
            foreach (Label label in Properties.Settings.Default.Plugin_Labels)
            {
                Plug_ComboBox.Items.Add(label);
            }
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

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Prof_Edit != null && Prof_Delete != null)
            {
                switch ((sender as ComboBox).SelectedIndex)
                {
                    case 0:
                        Prof_Edit.IsEnabled = false;
                        Prof_Delete.IsEnabled = false;
                        break;
                    case 1:
                        var addLabelWindow = new AddLabel();
                        addLabelWindow.Owner = this;
                        addLabelWindow.ShowDialog();
                        Prof_Edit.IsEnabled = false;
                        Prof_Delete.IsEnabled = false;
                        break;
                    default:
                        Prof_Edit.IsEnabled = true;
                        Prof_Delete.IsEnabled = true;
                        break;
                }
            }
        }

        private void Prof_Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("ラベルを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.Profile_Labels.Remove(Prof_ComboBox.SelectedItem as Label);
                Prof_ComboBox.Items.Remove(Prof_ComboBox.SelectedItem);
                Prof_ComboBox.SelectedIndex = 0;
            }
        }

        private void Plugin_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Plug_Edit != null && Plug_Delete != null)
            {
                switch ((sender as ComboBox).SelectedIndex)
                {
                    case 0:
                        Plug_Edit.IsEnabled = false;
                        Plug_Delete.IsEnabled = false;
                        break;
                    case 1:
                        var addLabelWindow = new AddLabel();
                        addLabelWindow.Owner = this;
                        addLabelWindow.IsPluginSelection = true;
                        addLabelWindow.ShowDialog();
                        Plug_Edit.IsEnabled = false;
                        Plug_Delete.IsEnabled = false;
                        break;
                    default:
                        Plug_Edit.IsEnabled = true;
                        Plug_Delete.IsEnabled = true;
                        break;
                }
            }
        }

        private void Plug_Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("ラベルを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.Plugin_Labels.Remove(Plug_ComboBox.SelectedItem as Label);
                Plug_ComboBox.Items.Remove(Plug_ComboBox.SelectedItem);
                Plug_ComboBox.SelectedIndex = 0;
            }
        }

        private void Prof_Edit_Click(object sender, RoutedEventArgs e)
        {
            var AddLabelWindow = new AddLabel();
            AddLabelWindow.Id = Prof_ComboBox.SelectedIndex - 2;
            AddLabelWindow.Owner = this;
            AddLabelWindow.ShowDialog();
        }

        private void Plug_Edit_Click(object sender, RoutedEventArgs e)
        {
            var AddLabelWindow = new AddLabel();
            AddLabelWindow.Id = Plug_ComboBox.SelectedIndex - 2;
            AddLabelWindow.Owner = this;
            AddLabelWindow.IsPluginSelection = true;
            AddLabelWindow.ShowDialog();
        }

        private void AviutlPath_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void AviutlPath_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files.Count() != 1 || files == null)
            {
                MessageBox.Show("複数のファイルまたはディレクトリを指定することはできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            };
            if (System.IO.Directory.Exists(files[0]))
            {
                MessageBox.Show("ディレクトリを指定することはできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!files[0].EndsWith("aviutl.exe"))
            {
                MessageBox.Show("指定されたファイルは「aviutl.exe」ではありません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            (sender as TextBox).Text = files[0];
        }

        private void AucPath_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void AucPath_Drop(object sender, DragEventArgs e)
        {
            MainWindow.OnDropped(sender as TextBox, e);
        }
    }

}
