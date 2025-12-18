using System.Windows;

namespace SnakeMarsTheme.Views
{
    public partial class InputNameWindow : Window
    {
        public string ThemeName { get; private set; }

        public InputNameWindow(string defaultName = "")
        {
            InitializeComponent();
            NameTextBox.Text = defaultName;
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Por favor escribe un nombre válido.", "Nombre Requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Simple validation characters
            // Remove invalid path chars
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            ThemeName = name;
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
