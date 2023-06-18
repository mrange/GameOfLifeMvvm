using Avalonia.Controls;

namespace ItIsMyLife;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Content = new ItIsMyLifeControl();
    }
}