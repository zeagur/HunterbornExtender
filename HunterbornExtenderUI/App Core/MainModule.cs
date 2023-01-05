using Autofac;
using HunterbornExtenderUI.App_Core.Death_Item_Selection;

namespace HunterbornExtenderUI;

public class MainModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Singletons
        builder.RegisterType<EDIDtoForm>().AsSelf().SingleInstance();

        builder.RegisterType<SettingsProvider>().AsSelf().SingleInstance();

        builder.RegisterType<PluginLoader>().AsSelf().SingleInstance();

        builder.RegisterType<VM_PluginList>().AsSelf().SingleInstance();
        builder.RegisterType<VM_DeathItemSelectionList>().AsSelf().SingleInstance();

        builder.RegisterType<VMLoader_Plugins>().AsSelf().SingleInstance();
        builder.RegisterType<VMLoader_DeathItems>().AsSelf().SingleInstance();

        builder.RegisterType<VM_WelcomePage>().AsSelf().SingleInstance();
        builder.RegisterType<VM_DeathItemAssignmentPage>().AsSelf().SingleInstance();
        builder.RegisterType<VM_PluginEditorPage>().AsSelf().SingleInstance();
        builder.RegisterType<VM_WelcomePage>().AsSelf().SingleInstance();

        builder.RegisterType<MainWindowVM>().AsSelf().SingleInstance();
        // Other
        builder.RegisterType<PluginEntryLegacyzEdit>().AsSelf();
        builder.RegisterType<VM_Plugin>().AsSelf();
        builder.RegisterType<VM_PluginEntry>().AsSelf();
        builder.RegisterType<VM_DeathItemSelection>().AsSelf();
        builder.RegisterType<VM_MaterialEntry>().AsSelf();
        builder.RegisterType<VM_Material>().AsSelf();
        builder.RegisterType<VM_DeathItemSelection>().AsSelf();
    }
}