using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            
            AviutlPath.Text = Settings.AviUtl_Path;
            Follow_Operation_Settings_CheckBox.IsChecked = Settings.Follow_Behavior_Setting;
            foreach (ProfileLabel label in Settings.Profile_Labels)
            {
                Prof_ComboBox.Items.Add(label);
            }
            foreach (PluginLabel label in Settings.Plugin_Labels)
            {
                Plug_ComboBox.Items.Add(label);
            }
            switch (Settings.Getting_Progress_Interval)
            {
                case 0:
                    Progress_Interval.SelectedIndex = 0;
                    break;
                case 700:
                    Progress_Interval.SelectedIndex = 1;
                    break;
                case 1000:
                    Progress_Interval.SelectedIndex = 2;
                    break;
                case 2000:
                    Progress_Interval.SelectedIndex = 3;
                    break;
                case 5000:
                    Progress_Interval.SelectedIndex = 4;
                    break;
                case 10000:
                    Progress_Interval.SelectedIndex = 5;
                    break;
            }
            Restore_Complete_Operation_CheckBox.IsChecked = Settings.Restore_After_Process;
            Analyze_Input_File_CheckBox.IsChecked = Settings.Do_File_Analize;
            Pregenerate_Index_File_CheckBox.IsChecked = Settings.Pregenerate_Index_File;
            SuffixTextBox.Text = Settings.Suffix;
        }

        private void aviutlPathOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "aviutl.exe|aviutl.exe";


            if (dialog.ShowDialog() == true)
            {
                AviutlPath.Text = dialog.FileName;
            }
        }

        //private void aucPathOpen_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new CommonOpenFileDialog();
        //    dialog.IsFolderPicker = true;

        //    if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
        //    {
        //        aucPath.Text = dialog.FileName;
        //    }
        //}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Topmost = true;
            var Setting = Properties.Settings.Default;
            //Setting.AUC_Path = aucPath.Text;
            Setting.AviUtl_Path = AviutlPath.Text;
            //Setting.Use_Indexer = (bool)Use_Indexer.IsChecked;
            Setting.Follow_Behavior_Setting = (bool)Follow_Operation_Settings_CheckBox.IsChecked;
            //Setting.Indexer_Path = Indexer_Path.Text;
            switch (Progress_Interval.SelectedIndex)
            {
                case 0:
                    Setting.Getting_Progress_Interval = 0;
                    break;
                case 1:
                    Setting.Getting_Progress_Interval = 700;
                    break;
                case 2:
                    Setting.Getting_Progress_Interval = 1000;
                    break;
                case 3:
                    Setting.Getting_Progress_Interval = 2000;
                    break;
                case 4:
                    Setting.Getting_Progress_Interval = 5000;
                    break;
                case 5:
                    Setting.Getting_Progress_Interval = 10000;
                    break;
            }
            Setting.Restore_After_Process = (bool)Restore_Complete_Operation_CheckBox.IsChecked;
            Setting.Do_File_Analize = (bool)Analyze_Input_File_CheckBox.IsChecked;
            Setting.Pregenerate_Index_File = (bool)Pregenerate_Index_File_CheckBox.IsChecked;
            Setting.Suffix = SuffixTextBox.Text;

            //if ((bool)Use_Indexer.IsChecked && (Indexer_Path.Text == "" || AviutlPath.Text == ""))
            //{
            //    MessageBox.Show("aui_indexerのパスとaviutl.exeのパスを指定してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            //    e.Cancel = true;
            //}
            Setting.Save();
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
                Properties.Settings.Default.Profile_Labels.Remove(Prof_ComboBox.SelectedItem as ProfileLabel);
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
                        ext.Text = "-";
                        break;
                    case 1:
                        var addLabelWindow = new AddLabel();
                        addLabelWindow.Owner = this;
                        addLabelWindow.IsPluginSelection = true;
                        addLabelWindow.ShowDialog();
                        Plug_Edit.IsEnabled = false;
                        Plug_Delete.IsEnabled = false;
                        ext.Text = "-";
                        break;
                    default:
                        Plug_Edit.IsEnabled = true;
                        Plug_Delete.IsEnabled = true;
                        try
                        {
                            var extension = Properties.Settings.Default.Plugin_Labels[(sender as ComboBox).SelectedIndex - 2].Extension;
                            if (extension != "")
                            {
                                ext.Text = extension;
                            }
                            else
                            {
                                ext.Text = "-";
                            }
                            
                        } catch (ArgumentOutOfRangeException)
                        {
                            ext.Text = "-";
                        }

                break;
                }
            }
        }

        private void Plug_Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("ラベルを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.Plugin_Labels.Remove(Plug_ComboBox.SelectedItem as PluginLabel);
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

        //private void Indexer_Path_Open_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new OpenFileDialog();
        //    dialog.Filter = "aui_indexer|aui_indexer.exe";


        //    if (dialog.ShowDialog() == true)
        //    {
        //        Indexer_Path.Text = dialog.FileName;
        //    }
        //}

        //private void CheckBox_Click(object sender, RoutedEventArgs e)
        //{
        //    var IsChecked = (sender as CheckBox).IsChecked;

        //    if (IsChecked == true)
        //    {
        //        Indexer_Text.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
        //        Indexer_Path.IsEnabled = true;
        //        Indexer_Path_Open.IsEnabled = true;
        //        Droppable_Text_1.Visibility = Visibility.Visible;
        //    } else
        //    {
        //        Indexer_Text.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(130, 0, 0, 0));
        //        Indexer_Path.IsEnabled = false;
        //        Indexer_Path_Open.IsEnabled = false;
        //        Droppable_Text_1.Visibility = Visibility.Hidden;
        //    }
            
        //}

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Use_Indexer.IsChecked = Properties.Settings.Default.Use_Indexer;
        //    CheckBox_Click(Use_Indexer, null);
        //}

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
