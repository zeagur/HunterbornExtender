using Autofac;

namespace HunterbornExtenderUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
{
    public MainWindow()
    {
        InitializeComponent();

        var builder = new ContainerBuilder();
        builder.RegisterModule<MainModule>();
        var container = builder.Build();

        var mvm = container.Resolve<MainWindowVM>();
        this.DataContext = mvm;
    }
}