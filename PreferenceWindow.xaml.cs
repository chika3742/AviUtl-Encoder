using Microsoft.VisualBasic;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace AUEncoder
{
    /// <summary>
    /// PreferenceWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PreferenceWindow : Window
    {
        public PreferenceWindow()
        {
            InitializeComponent();

            var Settings = Properties.Settings.Default;

            aucPath.Text = Settings.AUC_Path;
            aviutlPath.Text = Settings.AviUtl_Path;
            Indexer_Path.Text = Settings.Indexer_Path;
            Lw_Path.Text = Settings.InputPlugin_Path;
            foreach (Label label in Settings.Profile_Labels)
            {
                Prof_ComboBox.Items.Add(label);
            }
            foreach (Label label in Settings.Plugin_Labels)
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
            var Setting = Properties.Settings.Default;
            Setting.AUC_Path = aucPath.Text;
            Setting.AviUtl_Path = aviutlPath.Text;
            Setting.Use_Indexer = (bool)Use_Indexer.IsChecked;
            Setting.Indexer_Path = Indexer_Path.Text;
            Setting.InputPlugin_Path = Lw_Path.Text;

            if ((bool)Use_Indexer.IsChecked && (Indexer_Path.Text == "" || Lw_Path.Text == ""))
            {
                MessageBox.Show("aui_indexerのパスとlwinput.auiのパスを指定してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
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

        private void ChangeDragOverCursor(object sender, DragEventArgs e)
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

        private void AucPath_Drop(object sender, DragEventArgs e)
        {
            MainWindow.OnDropped(sender as TextBox, e);
        }

        private void Indexer_Path_Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "aui_indexer|aui_indexer.exe";


            if (dialog.ShowDialog() == true)
            {
                Indexer_Path.Text = dialog.FileName;
            }
        }

        private void Lwinput_Path_Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "L-SMASH Works 入力プラグイン|lwinput.aui";


            if (dialog.ShowDialog() == true)
            {
                Lw_Path.Text = dialog.FileName;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var IsChecked = (sender as CheckBox).IsChecked;

            if (IsChecked == true)
            {
                Indexer_Text.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                Lw_Text.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                Indexer_Path.IsEnabled = true;
                Lw_Path.IsEnabled = true;
                Indexer_Path_Open.IsEnabled = true;
                Lwinput_Path_Open.IsEnabled = true;
                Droppable_Text_1.Visibility = Visibility.Visible;
                Droppable_Text_2.Visibility = Visibility.Visible;
            } else
            {
                Indexer_Text.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(130, 0, 0, 0));
                Lw_Text.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(130, 0, 0, 0));
                Indexer_Path.IsEnabled = false;
                Lw_Path.IsEnabled = false;
                Indexer_Path_Open.IsEnabled = false;
                Lwinput_Path_Open.IsEnabled = false;
                Droppable_Text_1.Visibility = Visibility.Hidden;
                Droppable_Text_2.Visibility = Visibility.Hidden;
            }
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Use_Indexer.IsChecked = Properties.Settings.Default.Use_Indexer;
            CheckBox_Click(Use_Indexer, null);
        }

        private void Indexer_Path_Drop(object sender, DragEventArgs e)
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
            if (!files[0].EndsWith("aui_indexer.exe"))
            {
                MessageBox.Show("指定されたファイルは「aui_indexer.exe」ではありません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            (sender as TextBox).Text = files[0];
        }

        private void Lw_Path_Drop(object sender, DragEventArgs e)
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
            if (!files[0].EndsWith("lwinput.aui"))
            {
                MessageBox.Show("指定されたファイルは「lwinput.aui」ではありません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            (sender as TextBox).Text = files[0];
        }
    }

}
