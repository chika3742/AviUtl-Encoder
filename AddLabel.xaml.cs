using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// AddLabel.xaml の相互作用ロジック
    /// </summary>
    
    public class ProfileLabel
    {
        public int Number { get; set; }
        public string Name { get; set; }
    }
    public class PluginLabel
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
    }
    public partial class AddLabel : Window
    {
        public bool IsPluginSelection { get; set; }
        public int Id { get; set; } = -1;
        int defNumber = 0;
        public AddLabel()
        {
            InitializeComponent();
        }
        

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            var text = (sender as TextBox).Text + e.Text;
            e.Handled = regex.IsMatch(text);
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Number.Text != "" && Name.Text != "")
            {
                if (!int.TryParse(Number.Text, out int numText))
                {
                    MessageBox.Show("入力された番号の形式が間違っています。(半角数字で入力してください。)", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (IsPluginSelection)
                {
                    if (Id == -1)
                    {
                        if (Properties.Settings.Default.Plugin_Labels.Exists(delegate (PluginLabel l) { return l.Number.ToString() == Number.Text; }))
                        {
                            MessageBox.Show("同じ番号のラベルが既に存在します。「編集」ボタンから操作してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        var item = new PluginLabel();
                        item.Name = Name.Text;
                        item.Number = int.Parse(Number.Text);
                        item.Extension = Extension.Text;
                        Properties.Settings.Default.Plugin_Labels.Add(item);
                        (Owner as PreferenceWindow).Plug_ComboBox.Items.Add(item);
                        (Owner as PreferenceWindow).Plug_ComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        if (Properties.Settings.Default.Plugin_Labels.Exists(delegate (PluginLabel l) { return l.Number.ToString() == Number.Text; }) && defNumber.ToString() != Number.Text)
                        {
                            MessageBox.Show("同じ番号のラベルが既に存在します。「編集」ボタンから操作してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        Properties.Settings.Default.Plugin_Labels[Id].Name = Name.Text;
                        Properties.Settings.Default.Plugin_Labels[Id].Number = int.Parse(Number.Text);
                        var item = new PluginLabel();
                        item.Name = Name.Text;
                        item.Number = int.Parse(Number.Text);
                        item.Extension = Extension.Text;
                        var window = Owner as PreferenceWindow;
                        window.Plug_ComboBox.Items[Id + 2] = null;
                        window.Plug_ComboBox.Items[Id + 2] = item;
                        window.Plug_ComboBox.SelectedIndex = Id;
                    }
                    //Properties.Settings.Default.Plugin_Labels.Sort((a, b) => a.Number - b.Number);
                    Close();
                }
                else
                {
                    if (Id == -1)
                    {
                        if (Properties.Settings.Default.Profile_Labels.Exists(delegate (ProfileLabel l) { return l.Number.ToString() == Number.Text; }))
                        {
                            MessageBox.Show("同じ番号のラベルが既に存在します。「編集」ボタンから操作してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        var item = new ProfileLabel();
                        item.Name = Name.Text;
                        item.Number = int.Parse(Number.Text);
                        Properties.Settings.Default.Profile_Labels.Add(item);
                        (Owner as PreferenceWindow).Prof_ComboBox.Items.Add(item);
                        (Owner as PreferenceWindow).Prof_ComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        if (Properties.Settings.Default.Profile_Labels.Exists(delegate (ProfileLabel l) { return l.Number.ToString() == Number.Text; }) && defNumber.ToString() != Number.Text)
                        {
                            MessageBox.Show("同じ番号のラベルが既に存在します。「編集」ボタンから操作してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        Properties.Settings.Default.Profile_Labels[Id].Name = Name.Text;
                        Properties.Settings.Default.Profile_Labels[Id].Number = int.Parse(Number.Text);
                        var item = new ProfileLabel();
                        item.Name = Name.Text;
                        item.Number = int.Parse(Number.Text);

                        var window = Owner as PreferenceWindow;
                        window.Prof_ComboBox.Items[Id + 2] = null;
                        window.Prof_ComboBox.Items[Id + 2] = item;
                        window.Prof_ComboBox.SelectedIndex = Id + 2;
                    }
                    //Properties.Settings.Default.Profile_Labels.Sort((a, b) => a.Number - b.Number);
                    Close();
                }
            } else
            {
                MessageBox.Show("すべての欄に入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsPluginSelection)
            {
                (Owner as PreferenceWindow).Plug_ComboBox.SelectedIndex = 0;
            }
            else
            {
                (Owner as PreferenceWindow).Prof_ComboBox.SelectedIndex = 0;
            }
            
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsPluginSelection)
            {
                NumberText.Text = "プラグイン番号";
                ExtText.Visibility = Visibility.Visible;
                Extension.Visibility = Visibility.Visible;
                if (Id != -1)
                {
                    Number.Text = Properties.Settings.Default.Plugin_Labels[Id].Number.ToString();
                    Name.Text = Properties.Settings.Default.Plugin_Labels[Id].Name;
                    defNumber = Properties.Settings.Default.Plugin_Labels[Id].Number;
                    Extension.Text = Properties.Settings.Default.Plugin_Labels[Id].Extension;
                    this.Title = "プラグインラベルの編集";
                }
            }
            else
            {
                ExtText.Visibility = Visibility.Hidden;
                Extension.Visibility = Visibility.Hidden;
                if (Id != -1)
                {
                    Number.Text = Properties.Settings.Default.Profile_Labels[Id].Number.ToString();
                    Name.Text = Properties.Settings.Default.Profile_Labels[Id].Name;
                    defNumber = Properties.Settings.Default.Profile_Labels[Id].Number;
                    this.Title = "プロファイルラベルの編集";
                }
            }
        }
    }
}
