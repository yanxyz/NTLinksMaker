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

namespace NTLinksMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtCwd.Text = Environment.CurrentDirectory;
            cmbLinkTypes.ItemsSource = new LinkTypes().List;
            HandleArguments();
        }

        private void HandleArguments()
        {
            // usage: exe target link options
            var args = Environment.GetCommandLineArgs();
            var n = 0;
            var maybeDir = false;
            for (var i = 1; i < args.Length; i++)
            {
                var item = args[i];
                if (item == "/admin")
                {
                    break;
                }

                if (item[0] == '/') continue;
                if (n == 0)
                {
                    txtTarget.Text = item;
                    maybeDir = Utils.MaybeDirectory(item);
                    ++n;
                }
                else
                {
                    txtLink.Text = item;
                    break;
                }
            }

            if (App.PreferSymlink)
                n = maybeDir ? 3 : 2;
            else
                n = maybeDir ? 1 : 0;
            cmbLinkTypes.SelectedIndex = n;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            var link = GetText(txtLink);
            var target = GetText(txtTarget);
            if (target == String.Empty) return;

            btnOk.IsEnabled = false;
            try
            {
                var linkType = cmbLinkTypes.SelectedItem as LinkType;
                Utils.CreateLink(link, target, linkType, relative: GetChecked(cbRelative), force: GetChecked(cbForce));
            }
            catch (Exception ex)
            {
                App.ShowError(ex.Message);
                //throw;
            }
            btnOk.IsEnabled = true;
        }

        private string GetText(TextBox tb)
        {
            var text = tb.Text.Trim();
            if (text == String.Empty)
            {
                tb.Focus();
            }
            return text;
        }

        private bool GetChecked(CheckBox cb)
        {
            return cb.IsChecked ?? false;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void cmbLinkTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLinkTypes.SelectedIndex < 2)
            {
                ctlRelative.Visibility = Visibility.Collapsed;
            }
            else
            {
                ctlRelative.Visibility = Visibility.Visible;
            }
        }

        private void Relative_Click(object sender, RoutedEventArgs e)
        {
            var target = GetText(txtTarget);
            if (target == String.Empty) return;

            var link = GetText(txtLink);
            string text;

            if (Utils.IsAbsolutePath(target))
            {
                text = $"The calculated relative path is\n{Utils.GetRelativePath(target, link)}";
            }
            else
            {
                text = "Do nothing as Target is a relative path";
            }

            MessageBox.Show(text);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/yanxyz/NTLinksMaker/");
        }
    }
}
