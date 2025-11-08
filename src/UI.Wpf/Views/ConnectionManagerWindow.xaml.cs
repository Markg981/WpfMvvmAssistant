using System.Windows;

namespace UI.Wpf.Views;

public partial class ConnectionManagerWindow : Window
{
    public ConnectionManagerWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
