using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace AUEncoder
{
    /// <summary>
    /// SelectFilesWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectFilesWindow : Window
    {
        public List<string> items { get; set;}
        public bool changeList { get; set; }

        public SelectFilesWindow(List<string> items)
        {
            InitializeComponent();

            for (var i = 0; i < items.Count(); i++)
            {
                var item = new ListBoxItem();
                item.Content = items[i];
                item_list.Items.Add(item);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems.Count != 0)
            {
                delete_button.IsEnabled = true;
            }
            else
            {
                delete_button.IsEnabled = false;
            }
        }

        private void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            var selectedItemCount = item_list.SelectedItems.Count;
            for (var i = 0; i < selectedItemCount; i++)
            {
                item_list.Items.Remove(item_list.SelectedItems[0]);
            }
        }

        private void Add_button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new OpenFileDialog();
            picker.Multiselect = true;
            picker.Filter = "動画ファイル|*.mp4;*.avi;*.wmv;*.mov;*.m4v;*.mts|AviUtlプロジェクトファイル|*.aup|すべてのファイル|*.*";
            if (picker.ShowDialog() == true)
            {
                for (var i = 0; i < picker.FileNames.Count(); i++)
                {
                    var inst = new ListBoxItem();
                    inst.Content = picker.FileNames[i];
                    item_list.Items.Add(inst);
                }
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            changeList = true;
            List<string> itemList = new List<string>();
            for (var i = 0; i < item_list.Items.Count; i++)
            {
                itemList.Add((string)(item_list.Items[i] as ListBoxItem).Content);
            }
            items = itemList;
            Close();
        }

        private void Delete_All_Button_Click(object sender, RoutedEventArgs e)
        {
            item_list.Items.Clear();
        }

        private void Item_list_PreviewDragOver(object sender, DragEventArgs e)
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

        private void Item_list_Drop(object sender, DragEventArgs e)
        {
            var isIgnoreAll = false;
            var data = e.Data.GetData(DataFormats.FileDrop) as string[];
            for (var i = 0; i < data.Count(); i++)
            {
                var path = data[i];
                if (Directory.Exists(path))
                {
                    try
                    {
                        var files = Directory.GetFiles(path);
                        for (var a = 0; a < files.Count(); a++)
                        {
                            var file = files[a];
                            if (!(file.EndsWith(".mp4") || file.EndsWith(".avi") || file.EndsWith(".mov") ||
                            file.EndsWith(".wmv") || file.EndsWith(".m4v") || file.EndsWith(".mts") ||
                            file.EndsWith(".aup")) && !isIgnoreAll)
                            {
                                var result = MessageBox.Show($"ドロップされたファイル({file.Split('\\').Last()})は動画ファイルまたはプロジェクトファイルではない可能性があります。続行しますか？\n\nはい：すべて「はい」\nいいえ：今回のみ「はい」\nキャンセル：ここから先を無かったことにする", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                                if (result == MessageBoxResult.Cancel) break;
                                if (result == MessageBoxResult.Yes) isIgnoreAll = true;
                            }
                            var item = new ListBoxItem();
                            item.Content = file;
                            item_list.Items.Add(item);
                        }
                    } catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("アクセスが拒否されました。", "ドロップエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                }
                else
                {
                    if (!(path.EndsWith(".mp4") || path.EndsWith(".avi") || path.EndsWith(".mov") ||
                        path.EndsWith(".wmv") || path.EndsWith(".m4v") || path.EndsWith(".mts") ||
                        path.EndsWith(".aup")) && !isIgnoreAll)
                    {
                        var result = MessageBox.Show($"ドロップされたファイル({path.Split('\\').Last()})は動画ファイルまたはプロジェクトファイルではない可能性があります。続行しますか？\n\nはい：すべて「はい」\nいいえ：今回のみ「はい」\nキャンセル：ここから先を無かったことにする", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Cancel) break;
                        if (result == MessageBoxResult.Yes) isIgnoreAll = true;
                    }
                    var inst = new ListBoxItem();
                    inst.Content = path;
                    item_list.Items.Add(inst);
                }
            }
        }
    }
}
