using System.Windows;
using UI.Wpf.ViewModels;

namespace UI.Wpf.Views;

public partial class ConnectionManagerWindow : Window
{
    public ConnectionManagerWindow()
    {
        InitializeComponent();

        if (DataContext is ConnectionManagerViewModel vm)
        {
            vm.RequestClose += (dialogResult) =>
            {
                DialogResult = dialogResult;
                Close();
            };
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
