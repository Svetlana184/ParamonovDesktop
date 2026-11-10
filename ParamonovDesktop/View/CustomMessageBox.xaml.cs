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

namespace ParamonovDesktop.View
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public string MessageText { get; set; }

        public CustomMessageBox(string message, string title = "Сообщение")
        {
            InitializeComponent();
            MessageText = message;
            DataContext = this;
            Title = title;
        }

        public static bool? Show(string message, string title = "Сообщение", Window owner = null)
        {
            var box = new CustomMessageBox(message, title);
            if (owner != null) box.Owner = owner;
            return box.ShowDialog();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
