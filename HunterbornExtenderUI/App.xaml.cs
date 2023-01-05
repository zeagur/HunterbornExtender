using System.Windows;
using Autofac;
using HunterbornExtender;
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
        var mainVM = container.Resolve<MainWindowVM>();
        window.DataContext = mainVM;
        if (state.ExtraSettingsDataPath != null)
        {
            mainVM.WelcomePage.ForceSettingsDir(state.ExtraSettingsDataPath.Value);
        }
        
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
        HunterbornExtender.Settings.Settings settings = new();
        if (state.ExtraSettingsDataPath != null)
        {
            Write.Success(0, "Loading settings in RunPatch from " + state.ExtraSettingsDataPath);
            settings = PatcherSettingsIO.LoadFromDisk(System.IO.Path.Combine(state.ExtraSettingsDataPath, "settings.json"));
            if (settings != null)
            {
                Write.Success(0, "Loaded settings in RunPatch from " + state.ExtraSettingsDataPath);
            }
        }

        HunterbornExtender.Program.RunPatch(state, settings ?? new HunterbornExtender.Settings.Settings());
    }
}