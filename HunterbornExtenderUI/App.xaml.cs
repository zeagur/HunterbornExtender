using System.Windows;

namespace HunterbornExtenderUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
            

        //SynthesisPipeline.Instance
        //.SetTypicalOpen(Initialize);

        var window = new MainWindow();
        window.Show();
    }

    /*
    public static int Initialize(System.Drawing.Rectangle r)
    {

        var window = new MainWindow();
        return 0;
    }*/
}