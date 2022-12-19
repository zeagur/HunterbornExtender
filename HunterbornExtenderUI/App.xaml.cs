using System.Windows;
using Autofac;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog.WPF;

namespace HunterbornExtenderUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SynthesisPipeline.Instance
            .SetOpenForSettings(OpenForSettings)
            .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
            .SetTypicalOpen(StandaloneOpen)
            .SetForWpf()
            .Run(e.Args)
            .Wait();
    }

    public static int OpenForSettings(IOpenForSettingsState state)
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<MainModule>();
        builder.RegisterInstance(new OpenForSettingsWrapper(state)).AsImplementedInterfaces();
        var container = builder.Build();
        
        var window = new MainWindow();
        window.DataContext = container.Resolve<MainWindowVM>();
        window.Show();
        window.CenterAround(state.RecommendedOpenLocation);

        return 0;
    }

    public static int StandaloneOpen()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<MainModule>();
        builder.RegisterType<StandaloneRunStateProvider>().AsImplementedInterfaces();
        var container = builder.Build();
        
        var window = new MainWindow();
        window.DataContext = container.Resolve<MainWindowVM>();
        window.Show();
        
        return 0;
    }

    private async Task RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        throw new NotImplementedException();
    }
}