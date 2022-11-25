using Autofac;

namespace HunterbornExtenderUI;

public class MainModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Singletons
        builder.RegisterType<StateProvider>().AsSelf().SingleInstance();
        builder.RegisterType<DataState>().AsSelf().SingleInstance();
        builder.RegisterType<EDIDtoForm>().AsSelf().SingleInstance();
        builder.RegisterType<PluginLoader>().AsSelf().SingleInstance();
        builder.RegisterType<VMLoader_Plugins>().AsSelf().SingleInstance();
        builder.RegisterType<VM_WelcomePage>().AsSelf().SingleInstance();
        builder.RegisterType<VM_DeathItemAssignmentPage>().AsSelf().SingleInstance();
        builder.RegisterType<VM_PluginEditorPage>().AsSelf().SingleInstance();

        builder.RegisterType<MainWindowVM>().AsSelf().SingleInstance();
        // Other
        builder.RegisterType<PluginEntryLegacyzEdit>().AsSelf();
        builder.RegisterType<VM_Plugin>().AsSelf();
        builder.RegisterType<VM_PluginEntry>().AsSelf();
    }
}