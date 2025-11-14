using System.Windows;
using UI.Wpf.ViewModels;

namespace UI.Wpf.Views;

public partial class ConnectionManagerWindow : Window
{
    public ConnectionManagerWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ConnectionManagerViewModel oldVm)
        {
            oldVm.RequestClose -= OnRequestClose;
        }

        if (e.NewValue is ConnectionManagerViewModel newVm)
        {
            newVm.RequestClose += OnRequestClose;
        }
    }

    private void OnRequestClose(bool dialogResult)
    {
        try
        {
            DialogResult = dialogResult;
        }
        catch (InvalidOperationException)
        {
            // This can happen if the window is closed while the dialog is still being shown.
            // In this case, we can ignore the error.
        }
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
