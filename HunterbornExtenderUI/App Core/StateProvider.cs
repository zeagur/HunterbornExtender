using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Skyrim;
using System.Reflection;
using Noggog;

namespace HunterbornExtenderUI;

public interface IStateProvider
{
    ILoadOrderGetter<IModListingGetter<ISkyrimModGetter>> LoadOrder { get; }
    ILinkCache<ISkyrimMod, ISkyrimModGetter> LinkCache { get; }
    DirectoryPath ExtraSettingsDataPath { get; }
}

public class StateProvider : IStateProvider
{
    public ILoadOrderGetter<IModListingGetter<ISkyrimModGetter>> LoadOrder { get; set; }
    public ILinkCache<ISkyrimMod, ISkyrimModGetter> LinkCache { get; set; }
    public ISkyrimMod PatchMod { get; set; }
    public DirectoryPath ExtraSettingsDataPath { get; set; } = String.Empty;

    public StateProvider()
    {
        string exeLocation = string.Empty;
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null && assembly.Location != null)
        {
            exeLocation = System.IO.Path.GetDirectoryName(assembly.Location) ?? string.Empty;
        }

        ExtraSettingsDataPath = exeLocation;

        // Environment setup. Temporary code until OpenForSettings gets refactored and I can get a patcher state
        var builder = GameEnvironment.Typical.Builder<ISkyrimMod, ISkyrimModGetter>(GameRelease.SkyrimSE);
        var environment = builder
            .TransformModListings(x =>
                x.OnlyEnabledAndExisting())
            .Build();

        LoadOrder = environment.LoadOrder;
        LinkCache = environment.LinkCache;
        PatchMod = new SkyrimMod(ModKey.FromName("HunterBornExtender", ModType.Plugin), SkyrimRelease.SkyrimSE);
            
    }
}