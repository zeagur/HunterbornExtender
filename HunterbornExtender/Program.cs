﻿namespace HunterbornExtender;

using DynamicData;
using HunterbornExtender.IO;
using HunterbornExtender.Settings;
using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Json;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Exceptions;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Newtonsoft.Json;
using Noggog;
using System;
using System.Collections.Generic;
using static HunterbornExtender.FormKeys;
using DeathItemGetter = Mutagen.Bethesda.Skyrim.ILeveledItemGetter;
using MeatSet = ValueTuple<Mutagen.Bethesda.Skyrim.IItemGetter, Mutagen.Bethesda.Skyrim.IConstructibleGetter, Mutagen.Bethesda.Skyrim.IConstructibleGetter>;
using PatchingRecords = StandardRecords<Mutagen.Bethesda.Skyrim.ISkyrimMod, Mutagen.Bethesda.Skyrim.FormList>;
using PeltSet = ValueTuple<Mutagen.Bethesda.Skyrim.IConstructibleGetter, Mutagen.Bethesda.Skyrim.IConstructibleGetter, Mutagen.Bethesda.Skyrim.IConstructibleGetter, Mutagen.Bethesda.Skyrim.IConstructibleGetter>;

sealed public class Program
{
    /// <summary>
    /// This is neutered because the patcher should get called from HunterbornExtenderUI.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        return 0;
    }

    /// <summary>
    /// Runs the patcher. This gets called from HunterbornExtenderUI, which provides the state and settings.
    /// 
    /// </summary>
    public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, Settings.Settings settings)
    {
        //
        // If the Settings were provided by HunterbornExtenderUI, the patcher can be run immediately.
        // Currently not happening!
        //
        if (CheckSettings("PARAMETER SETTINGS", settings))
        {
            new Program(settings, state).Initialize().Patch();
        }

        //
        // Load the settings.json file to populate the Settings.
        //
        else
        { 
            string settingsFilename = "settings.json";
            string settingsPath = $"{state.ExtraSettingsDataPath}\\{settingsFilename}";
            Write.Title(0, settingsPath);
            Console.WriteLine(settingsPath);

            var obj = JSONhandler<Settings.Settings>.LoadJSONFile(settingsPath, out string errorString);

            // Was settings.json successfully read? If so, use it to run the patcher.
            if (obj is Settings.Settings settings2 && CheckSettings("PARSED SETTINGS", settings2))
            {
                SelectionLinker.LinkDeathItemSelections(settings2.DeathItemSelections, settings2.PluginEntries);
                new Program(settings2, state).Initialize().Patch();
            }

            // If settings.json couldn't be read, use the empty settings. The patcher will compensate, but there
            // will be no selections other than the heuristics.
            else
            {
                Write.Fail(0, "Failure to parse: " + errorString);
                new Program(settings, state).Initialize().Patch();
            }
        } 
    }

    /// <summary>
    /// Creates a new instance of Program.
    /// </summary>
    /// 
    public Program(Settings.Settings settings, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        State = state;
        Settings = settings;
        LinkCache = State.LinkCache;
        LoadOrder = State.LoadOrder;
        PatchMod = State.PatchMod;
        FormLinkSubstitution = SpecialCases.GetCACOSub(LoadOrder.ContainsKey(CACO_MODKEY));
        ItemSubstitution = SpecialCases.GetCACOSubResolved(LoadOrder.ContainsKey(CACO_MODKEY), LinkCache);
    }

    /// <summary>
    /// Prepares the patcher object.
    /// Ensures that the plugins are all recreated and parsed.
    /// Matches selections to plugins.
    /// </summary>
    public Program Initialize()
    {
        //
        // If the Settings object contains NO plugins at all, perform the full parsing and recreating.
        // Normally this should happen in HunterbornExtenderUI.
        //
        if (Settings.PluginEntries.Count == 0)
        {
            Settings.PluginEntries = ImportPlugins();
        }

        //
        // If the Settings object contains plugins, it still shouldn't contain the internal ones because they
        // need to be recreated at patching time so that the patcher can get extra data that isn't passed
        // to the UI.
        //
        else
        {
            (var plugins, var recreationData) = RecreateInternal.RecreateInternalPlugins(LinkCache, DebuggingMode);
            plugins.AddRange(Settings.PluginEntries);
            Settings.PluginEntries = plugins;

            // Copy the recreation data into local dictionaries.
            foreach (var e in recreationData.KnownDeathItems) KnownDeathItems.Add(e.Key, e.Value);
            foreach (var e in recreationData.KnownCarcasses) KnownCarcasses.Add(e.Key, e.Value);
            foreach (var e in recreationData.KnownPelts) KnownPelts.Add(e.Key, e.Value);
            OriginalIndices = recreationData.OriginalIndices;
        }

        //
        // Add all creature types to the Advanced Taxonomy power.
        //
        Taxonomy.AddCreature(Skip.SKIP);
        Settings.PluginEntries.Where(plugin => plugin is not Skip).ForEach(plugin => Taxonomy.AddCreature(plugin));

        //
        // Link death entryItem selection to corresponding creature entry
        //
        SelectionLinker.LinkDeathItemSelections(Settings.DeathItemSelections, Settings.PluginEntries);

        Write.Success(0, $"Imported {Settings.DeathItemSelections.Length} death item selections");
        Write.Success(0, $"Found {Settings.PluginEntries.Count} creature types.");
        foreach(var plugin in Settings.PluginEntries )
        {
            Write.Action(1, plugin.SortName);
        }

        //
        // Heuristic matching and user selections should already be done.
        //
        // Scan the load order and apply heuristics to anything that is missing.
        // 
        try
        {
            Write.Action(0, $"Running heuristics.");
            var npcs = LoadOrder.PriorityOrder.Npc().WinningOverrides();
            
            DeathItemSelection[] selections = Heuristics.MakeHeuristicSelections(npcs, Settings.PluginEntries, Settings.DeathItemSelections, LinkCache, Settings.DebuggingMode);
            Settings.DeathItemSelections = selections;

            Write.Success(0, $"Heuristics produced {selections.Length} results.");
        }
        catch (HeuristicsError ex)
        {
            if (ex.GetBaseException() is RecordException cause)
                Write.Fail(0, $"Missing reference during heuristic: [{cause.EditorID} ({cause.FormKey})].");
            Write.Fail(0, ex.Message);
            Console.WriteLine(ex.ToString());
            throw ex;
        }
        
        foreach (var rec in PatchMod.EnumerateMajorRecords())
        {
            rec.IsCompressed = false;
        }

        return this;
    }

    /// <summary>
    /// Recreates the internal plugins and reads the JSON files. 
    /// If converted ones are not found, the legacies will be converted.
    /// </summary>
    /// 
    /// <returns>A list of plugins; first SKIP, then the internal plugins, then the addons.</returns>
    public List<PluginEntry> ImportPlugins()
    {
        //
        // Read the addon jsons.
        //
        Write.Divider(0);
        Write.Action(0, "Importing plugins.");
        var addonPlugins = LegacyConverter.ImportAndConvert(State);
        Write.Success(0, $"{addonPlugins.Count} creature types imported.");

        //
        // Recreate the internal plugins and store the recreation data.
        //
        List<PluginEntry> internalPlugins;
        try
        {
            Write.Divider(0);
            Write.Action(0, "Trying to recreate the hard-coded core plugin from Hunterborn.esp.");
            (internalPlugins, var recreationData) = RecreateInternal.RecreateInternalPlugins(LinkCache, DebuggingMode);

            foreach (var e in recreationData.KnownDeathItems) KnownDeathItems.Add(e.Key, e.Value);
            foreach (var e in recreationData.KnownCarcasses) KnownCarcasses.Add(e.Key, e.Value);
            foreach (var e in recreationData.KnownPelts) KnownPelts.Add(e.Key, e.Value);
            OriginalIndices = recreationData.OriginalIndices;

            if (internalPlugins.Count > 0)
            {
                Write.Success(0, $"Success: {internalPlugins.Count} hard-coded creature types found.");
            }
            else
            {
                Write.Fail(0, "No hard-coded creature types found. Check your Hunterborn installation.");
                throw new Exception("No hard-coded creature types found. Check your Hunterborn installation.");
            }
        }
        catch (RecreationError ex)
        {
            if (ex.InnerException is RecordException cause)
                Write.Fail(0, $"Missing reference during recreation: [{cause.EditorID} ({cause.FormKey})].");
            Write.Fail(0, ex.Message);
            Console.WriteLine(ex.ToString());
            throw ex;
        }

        //
        // Merge the lists and return the combined list.
        //
        List<PluginEntry> plugins = new();
        plugins.AddRange(internalPlugins);
        plugins.AddRange(addonPlugins);
        Write.Success(0, $"Addon creatures and hard-coded creatures merged; {plugins.Count} total.");

        return plugins;
    }

    /// <summary>
    /// Requires that Initialize() has already been called.
    /// </summary>
    public void Patch()
    {
        //
        // Resolve and locate all the FormLists and ScriptProperties that need patching.
        // 
        PatchingRecords std;
        try
        {
            Write.Divider(0);
            Write.Action(0, "Trying to resolve required forms from Hunterborn.esp, and preparing the patch structure.");
            std = PatchingRecords.CreatePatchingInstance(PatchMod, LoadOrder, LinkCache);
            Write.Success(0, $"Success.");
        }
        catch (Exception ex)
        {
            if (ex is RecordException cause) Write.Fail(0, $"Failed to resolve required forms because of unresolved reference [{cause.EditorID} ({cause.FormKey})]");
            Write.Fail(0, ex.Message);
            Console.WriteLine(ex.ToString());
            return;
        }

        //
        // Iterate through all the selections and create new records where appropriate.
        // Each DeathItem only needs one set of records created for it.
        //
        foreach (var selection in Settings.DeathItemSelections)
        {
            var name = selection.CreatureEntryName;
            PluginEntry? prototype = selection.Selection;

            //
            // null is used to indicate "SKIP". So is the SKIP plugin.
            // If either is found, continue on to the next selection.
            //
            if (prototype == null || PluginEntry.SKIP.Equals(prototype))
            {
                if (Settings.DebuggingMode) Write.Action(0, $"(SKIPPING) {name}");
                continue;
            }

            LinkCache.TryResolve<ILeveledItemGetter>(selection.DeathItem, out var deathItem);
            if (deathItem is null) continue;
            if (KnownDeathItems.ContainsKey(deathItem))
            {
                Write.Fail(1, $"Skipped {name}: DeathItem already processed.");
                continue;
            }
            else if (SpecialCases.Lists.ForbiddenDeathItems.Contains(deathItem))
            {
                Write.Fail(1, $"Skipped {name}: a forbidden DeathItem slipped past the filters!");
                continue;
            }

            try
            {
                //
                // Patching data for a specific DeathItem.
                //
                var data = CreateCreatureData(selection, prototype);

                //
                // Make the new records.
                //
                AddRecordFor(data, std);

                //
                // Avoid double-patching!
                //
                KnownDeathItems.Add(deathItem, prototype);
            }
            catch (RecordException ex)
            {
                Write.Fail(1, $"Skipped {name}: DeathItem could not be resolved: [{ex.FormKey} {ex.EditorID}]");
            }
            catch (DeathItemAlreadyAddedException)
            {
                Write.Fail(1, $"Skipped {name}: DeathItem already processed.");
            }
            catch (NoDeathItemException)
            {
                Write.Fail(1, $"Skipped {name}: No DeathItem.");
            }
            catch (Exception ex)
            {
                Write.Fail(1, $"Skipped {name}: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }
        }

        //
        // Correct the names of the internal plugins.
        //
        Write.Action(0, "Correcting internal names.");
        foreach (var e in OriginalIndices)
        {
            var plugin = e.Key;
            var index = e.Value;
            var list = std.GetCCFor(plugin.Type).RaceIndex;
            Write.Action(1, $"{list.Data[index]} => {plugin.ProperName}");
            list.Data[index] = plugin.ProperName;
        }

        //
        // Finish the advanced taxonomy data.
        //
        if (Settings.AdvancedTaxonomy)
        {
            try
            {
                Write.Divider(0);
                Write.Action(0, "Preparing advanced taxonomy data.");
                Taxonomy.Finalize(PatchMod, LinkCache);
                Write.Success(0, "Advanced taxonomy data added.");
            }
            catch (Exception ex)
            {
                if (ex is RecordException recex)
                    Write.Fail(0, $"Error while preparing advanced taxonomy data; record not found: [{recex.EditorID} ({recex.FormKey})].");
                else Write.Fail(0, "Error while preparing advanced taxonomy data.");
                Write.Fail(0, ex.Message);
                Console.WriteLine(ex.ToString());
                return;
            }
        }
        foreach (var rec in PatchMod.EnumerateMajorRecords())
        {
            rec.IsCompressed = false;
        }
    }


    /// <summary>
    /// Things that have to be done for each race:
    /// 
    /// 
    /// ==NEW RECORDS==
    /// Create a token MiscItem that identifies the creature as being Hunterborn-enabled.
    /// Create a carcass MiscItem that can go in the player's inventory.
    /// 
    /// Create a materials FormList containing 4 leveled lists of stuff, for the four levels of harvesting skill.
    /// 
    /// Create either 3 or 4 pelt MiscItems (3 if there's a default one the creature already).
    /// Create Leather and Fur Plate recipes for the pelts.
    /// 
    /// Add the creature's DeathItem to the DeathItems formlist.
    /// 
    /// Creature a new CustomDeathItem that contains tokens for the actions supported by the creature.
    /// 
    /// 
    /// ==HUNTERBORN QUEST SCRIPT==
    /// Add the carcass size to its array property.
    /// Add the carcass custom message (if any) to its array property.
    /// Add the CustomDeathItem to its array property.
    /// Add the meat type (if any) to its array property.
    /// Add the meat weight (if any) to its array property.
    /// Add the default pelt value (if any) to its array property.
    /// Add the shared death entryItem (if any) to its array property.
    /// Add the proper internalName to its array property.
    /// 
    /// For monsters:
    /// Add the venom (if any) to its array property.
    /// Add the blood (if any) to its array property.
    /// Add the "negative treasure" (if any) to its array property.
    /// 
    /// </summary>
    /// 
    private void AddRecordFor(CreatureData data, PatchingRecords std)
    {
        std.GetCCFor(data).RaceIndex.Data.Add(data.DescriptiveName);
        std.GetCCFor(data).CarcassSizes.Data.Add(data.Prototype.CarcassSize);
        std.GetCCFor(data).Switches.Objects.Add(CreateProperty(data.Prototype.Toggle));
        std.GetCCFor(data).SharedDeathItems.Objects.Add(CreateProperty(data.Prototype.SharedDeathItems));

        if (GetDefaultMeat(data, std) is IItemGetter meat)
        {
            if (IsCacoInstalled()) meat = ItemSubstitution(meat);
            std.GetCCFor(data).MeatType.Objects.Add(CreateProperty(meat.ToLink()));
            std.GetCCFor(data).MeatWeights.Data.Add(meat is IWeightValueGetter w ? w.Weight : 0.0f);
        }
        else
        {
            std.GetCCFor(data).MeatType.Objects.Add(CreateProperty(new FormLink<Ingestible>()));
            std.GetCCFor(data).MeatWeights.Data.Add(0.0f);
        }

        if (data.IsMonster)
        {
            std.Monsters.BloodItems.Objects.Add(CreateProperty(data.Prototype.BloodType));
            std.Monsters.VenomItems.Objects.Add(CreateProperty(data.Prototype.Venom));
        }
        else
        {
            std.Animals.CarcassMessages.Objects.Add(CreateProperty(data.Prototype.CarcassMessageBox));
        }

        var token = CreateToken(data, std);
        var pelts = CreatePelts(data, std);

        // Do this last because if quicklootpatch is enabled, pelts and meat could end up in materials.
        var mats = CreateMaterials(data, std);

        (var deathDescriptor, var tokens) = CreateDeathDescriptor(data, pelts, mats, std);
        if (data.IsAnimal) CreateCarcass(data, std);
        if (data.IsMonster) CreateDiscards(data, std);

        Write.Success(0, $"{DeathItemNamer(data.DeathItem)} => {data.Prototype.Name} {tokens.Select(t=>ItemNamer(t)).ToList().Pretty()}");
    }

    /// <summary>
    /// Create a CreatureData record. Convenience class for passing multiple fields of data to methods.
    /// </summary>
    /// <param name="deathItem"></param>
    /// <param name="prototype"></param>
    /// <param name="std"></param>
    /// <returns></returns>
    private CreatureData CreateCreatureData(DeathItemSelection selection, PluginEntry prototype)
    {
        var deathItem = selection.DeathItem.ToLink<DeathItemGetter>().Resolve(LinkCache);
        var internalName = CreateInternalName(deathItem);

        var descriptiveName = (selection.AssignedNPCs.Count == 1 && selection.AssignedNPCs.First() is INpcGetter creature)
            ? $"{Naming.PluginNameFB(prototype)} - {creature.Name?.ToString() ?? internalName}"
            : $"{Naming.PluginNameFB(prototype)} - {internalName}";

        return new(deathItem, internalName, descriptiveName, prototype, prototype.Type == EntryType.Animal, prototype.Type == EntryType.Monster);
    }

    /// <summary>
    /// Creates a unique internal internalName for the specified DeathItem. 
    /// This is used to derive the editorIds for the new forms that will be created 
    /// for the specified DeathItem.
    /// </summary>
    /// <param internalName="deathItem">The DeathItem to create a reasonably unique internalName for.</param>
    /// <returns>A reasonably unique internalName.</returns>
    /// 
    static string CreateInternalName(DeathItemGetter deathItem)
    {
        var name = deathItem.EditorID ?? deathItem.FormKey.ToString();
        var filteredName = SpecialCases.EditorIdFilter.Replace(name, "");
        var deprefixedName = SpecialCases.DeathItemPrefix.Replace(filteredName, "");

        if (!deprefixedName.IsNullOrWhitespace()) return deprefixedName;
        else if (!filteredName.IsNullOrWhitespace()) return filteredName;
        else return name;
    }

    /// <summary>
    /// Creates the Misc deathtoken for a creature.
    /// 
    /// The new deathtoken is appended to the deathtoken formlist for animals or monsters.
    /// 
    /// The deathtoken will be derived from the prototype's token (if it exists) or derived from the COW's deathtoken.
    /// 
    /// Naming is done heuristically. 
    /// </summary>
    /// 
    private MiscItem CreateToken(CreatureData data, PatchingRecords std)
    {
        // Get a pre-existing token that already has the keywords and model set.
        // That way all that needs to be done is to change the internalName and editor ID.
        var existingTokenLink = data.IsAnimal ? DEFAULT_TOKEN_ANIMAL : DEFAULT_TOKEN_MONSTER;
        existingTokenLink.TryResolve(LinkCache, out var existingToken);
        if (existingToken == null) throw new CoreRecordMissing(existingTokenLink);

        // Add the token to the patch.
        var token = PatchMod.MiscItems.DuplicateInAsNewRecord(existingToken);
        if (token == null) throw new InvalidOperationException();

        // Set the EditorID.
        token.EditorID = $"_DS_DI{data.InternalName}";
        token.Name = $"{data.InternalName} Token";

        // Put the token in the correct formlist.
        std.GetCCFor(data)._DS_FL_DeathItemTokens.Items.Add(token);

        return token;
    }

    private MiscItem CreateCarcass(CreatureData data, PatchingRecords std)
    {
        // Get a pre-existing carcass that already has the keywords set.
        DEFAULT_CARCASS.TryResolve(LinkCache, out var existingCarcass);
        if (existingCarcass == null) throw new CoreRecordMissing(DEFAULT_CARCASS);

        // Add the carcass to the patch.
        var carcass = PatchMod.MiscItems.DuplicateInAsNewRecord(existingCarcass);
        if (carcass == null) throw new InvalidOperationException();

        var oldName = carcass.Name?.String;
        if (oldName.IsNullOrEmpty()) oldName = "Cow Carcass";

        carcass.EditorID = $"_DS_Carcass{data.InternalName}";
        carcass.Name = oldName.Replace("Cow", $"{data.Prototype.ProperName}");

        carcass.Value = (uint)data.Prototype.CarcassValue;
        carcass.Weight = data.Prototype.CarcassWeight;

        carcass.Model = KnownCarcasses.ContainsKey(data.Prototype)
            ? KnownCarcasses[data.Prototype].Model?.DeepCopy() ?? CreateDefaultCarcassModel()
            : CreateDefaultCarcassModel();

        // Put the carcass in the correct formlist.
        std.Animals._DS_FL_CarcassObjects.Items.Add(carcass);

        return carcass;
    }

    static private Model CreateDefaultCarcassModel()
    {
        return new Model { File = "Clutter\\Containers\\MiscSackLarge.nif", AlternateTextures = null };
    }

    /// <summary>
    /// Creates a FormList of LeveledItems from a list of MaterialLevels in a prototype.
    /// </summary>
    /// 
    private FormList CreateMaterials(CreatureData data, PatchingRecords std)
    {
        // QuickLoot patching data.
        Dictionary<IFormLinkGetter<IItemGetter>, int> deathItemStuff = new();
        if (Settings.QuickLootPatch && data.DeathItem.Entries is IReadOnlyList<ILeveledItemEntryGetter> diEntries)
        {
            foreach (var entry in diEntries)
            {
                if (entry?.Data?.Reference is IItemGetter item && entry?.Data?.Count is short count)
                {
                    deathItemStuff[item.ToLinkGetter()] = count;
                }
                
            }
            LeveledItem patchedItems = PatchMod.LeveledItems.GetOrAddAsOverride(data.DeathItem);
            patchedItems.Entries?.Clear();
        }

        var matsFormList = PatchMod.FormLists.AddNew();
        if (matsFormList == null) throw new InvalidOperationException();

        var matsPerfectLvld = PatchMod.LeveledItems.AddNew();
        if (matsPerfectLvld == null) throw new InvalidOperationException();

        matsFormList.EditorID = $"_DS_FL_Mats_{data.InternalName}";
        matsPerfectLvld.EditorID = $"_DS_FL_Mats_Perfect_{data.InternalName}";

        int index = 0;
        foreach (MaterialLevel skillLevel in data.Prototype.Materials)
        { 
            // QuickLoot correction.
            if (Settings.QuickLootPatch)
            {
                //if (skillLevel.Items is Dictionary<IFormLinkGetter<IItemGetter>, int> items)
                if (skillLevel.Items is List<MaterialItem> items)
                {
                    foreach (var diEntry in deathItemStuff)
                    {
                        /*if (!items.ContainsKey(diEntry.Key))
                        {
                            items[diEntry.Key] = diEntry.Value;
                        }*/
                        if (!items.Any(item => item.Item.Equals(diEntry.Key)))
                        {
                            skillLevel.Add(diEntry.Key, diEntry.Value);
                        }
                    }
                }
            }

            var mat = PatchMod.LeveledItems.AddNew();
            if (mat == null) throw new InvalidOperationException();

            mat.EditorID = $"{matsFormList.EditorID}{index:D2}";
            index++;

            var entries = mat.Entries = new();

            foreach (var itemEntry in skillLevel.Items)
            {
                IFormLinkGetter<IItemGetter> item = new FormLink<IItemGetter>(itemEntry.Item.FormKey);
                
                if (IsCacoInstalled()) item = FormLinkSubstitution(item);
                entries.Add(CreateLeveledItemEntry(item, 1, itemEntry.Count));
            }

            matsFormList.Items.Add(mat);
        }

        // Put the materials formlist in the correct formlist.
        std.GetCCFor(data)._DS_FL_Mats__Lists.Items.Add(matsFormList);
        std.GetCCFor(data)._DS_FL_Mats__Perfect.Items.Add(matsPerfectLvld);

        return matsFormList;
    }

    private IFormListGetter CreatePelts(CreatureData data, PatchingRecords std)
    {
        var peltFormList = PatchMod.FormLists.AddNew();
        if (peltFormList == null) throw new InvalidOperationException();
        peltFormList.EditorID = $"_DS_FL_Pelts{data.InternalName}";
        std.GetCCFor(data)._DS_FL_PeltLists.Items.Add(peltFormList);

        // If the pelt counts are absent, don't make any pelts or recipes.
        if (data.Prototype.PeltCount.Length == 0)
        {
            std.GetCCFor(data).PeltValues.Data.Add(0);
            return peltFormList;
        }

        if (!KnownPelts.ContainsKey(data.Prototype))
        {
            var standard = GetDefaultPelt(data);

            // QuickLoot patch.
            if (standard is not null && Settings.QuickLootPatch)
            {
                LeveledItem patchedItems = PatchMod.LeveledItems.GetOrAddAsOverride(data.DeathItem);
                patchedItems.Entries?.RemoveAll(entry => entry?.Data?.Reference.Equals(standard) ?? false);
            }

            bool createdDefaultPelt = standard is not null;
            standard ??= CreateDefaultPelt(data);

            if (DebuggingMode) Write.Action(3, "Creating new Pelt records in Misc.");

            var poor = PatchMod.MiscItems.DuplicateInAsNewRecord(standard);
            var fine = PatchMod.MiscItems.DuplicateInAsNewRecord(standard);
            var flawless = PatchMod.MiscItems.DuplicateInAsNewRecord(standard);

            if (DebuggingMode) Write.Success(3, "Created \"poor\".");
            if (DebuggingMode) Write.Success(3, "Created \"standard\".");
            if (DebuggingMode) Write.Success(3, "Created \"flawless\".");

            string edid = $"_DS_Pelt_{data.InternalName ?? data.InternalName}";
            poor.EditorID = $"{edid}_00";
            fine.EditorID = $"{edid}_02";
            flawless.EditorID = $"{edid}_03";

            poor.Name = $"{standard.Name} (poor)";
            fine.Name = $"{standard.Name} (fine)";
            flawless.Name = $"{standard.Name} (flawless)";

            // Adjust the values of the non-standard pelts.
            poor.Value /= 2;
            fine.Value *= 2;
            flawless.Value *= 20;

            //PeltSet peltSet = (poor, standard, fine, flawless);
            var newPeltSet = (poor, standard, fine, flawless);
            KnownPelts[data.Prototype] = newPeltSet;

            std.GetCCFor(data).PeltValues.Data.Add((int)standard.Value);
            if (createdDefaultPelt) CreatePeltRecipes(data, newPeltSet, createdDefaultPelt);
        }

        // Add the pelts to the pelts formlist.
        var peltSet = KnownPelts[data.Prototype];
        peltFormList.Items.AddRange(new IFormLinkGetter<IItemGetter>[] { peltSet.Item1.ToLink(), peltSet.Item2.ToLink(), peltSet.Item3.ToLink(), peltSet.Item4.ToLink() });

        // Add the pelt value.
        var value = peltSet.Item2 is IWeightValueGetter item ? item.Value : 0;
        std.GetCCFor(data).PeltValues.Data.Add((int)value);

        return peltFormList;
    }

    /// <summary>
    /// 
    /// Create a new default pelt for a creature using a pre-existing pelt as a template.
    /// 
    /// </summary>
    /// <param name="data">Description of the creature to make a pelt for.</param>
    /// <returns></returns>
    /// <exception cref="CoreRecordMissing"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private MiscItem CreateDefaultPelt(CreatureData data)
    {
        if (DebuggingMode) Write.Action(3, $"Creating new pelt for {data.InternalName}");

        DEFAULT_PELT.TryResolve(LinkCache, out var existingPelt);
        if (existingPelt == null) throw new CoreRecordMissing(DEFAULT_PELT);

        var newPelt = PatchMod.MiscItems.DuplicateInAsNewRecord(DEFAULT_PELT.Resolve(LinkCache));
        if (newPelt == null) throw new InvalidOperationException();

        if (data.Prototype.DefaultPelt == null && data.Prototype.CreateDefaultPelt) data.Prototype.DefaultPelt = newPelt.ToLink();

        newPelt.EditorID = $"_DS_Pelt_{data.InternalName}_01";
        newPelt.Name = $"{data.Prototype.ProperName} Pelt";

        // @TODO Fill this in with something better.
        newPelt.Value = (uint)data.Prototype.CarcassValue / 2;

        if (DebuggingMode) Write.Success(3, $"Created new default Pelt: '{newPelt.Name}' ('{newPelt.EditorID}')");

        return newPelt;
    }

    /// <summary>
    /// Checks the creature's DeathItem for anything whose internalName or editorID contains a string-match
    /// that indicates it's probably a pelt. Returns the pelt.
    /// 
    /// </summary>
    /// <returns>A pair consisting of the pelt Item and a flag indicating whether it was created.</returns>
    /// 
    private IMiscItemGetter? GetDefaultPelt(CreatureData data)
    {
        if (!data.Prototype.DefaultPelt.IsNull) return data.Prototype.DefaultPelt.Resolve(LinkCache);
        var defaultPelt = RecreateInternal.FindDefaultPelt(data.DeathItem, LinkCache, DebuggingMode);
        if (defaultPelt is IMiscItemGetter pelt) return pelt;
        else return null;
    }

    /// <summary>
    /// Checks the creature's DeathItem for anything whose internalName or editorID contains a string-match
    /// that indicates it's probably meat. Returns the meat.
    /// 
    /// If no meat was found, one will be created and added to the patch.
    /// 
    /// </summary>
    /// 
    private IItemGetter? GetDefaultMeat(CreatureData data, PatchingRecords std)
    {
        if (!data.Prototype.Meat.IsNull) return data.Prototype.Meat.Resolve(LinkCache);
        else
        {
            var defaultMeat = FindDefaultMeat(data.DeathItem, std);

            // QuickLoot patch.
            if (defaultMeat is not null && Settings.QuickLootPatch)
            {
                LeveledItem patchedItems = PatchMod.LeveledItems.GetOrAddAsOverride(data.DeathItem);
                patchedItems.Entries?.RemoveAll(entry => entry?.Data?.Reference.Equals(defaultMeat) ?? false);
            }

            if (defaultMeat is IItemGetter meat) return meat;
            else if (data.Prototype.CreateDefaultMeat) return CreateDefaultMeat(data);
            else return null;
        }
    }

    /// <summary>
    /// Checks the creature's DeathItem for anything whose internalName or editorID contains a string-match
    /// that indicates it's probably a meat. Returns the meat.
    /// 
    /// If no meat was found, one will be created and added to the patch.
    /// 
    /// </summary>
    /// 
    private IItemGetter? FindDefaultMeat(DeathItemGetter data, PatchingRecords std)
    {
        var entries = data.Entries;
        if (entries == null) return null;

        foreach (var entry in entries)
        {
            var entryItem = entry.Data?.Reference.TryResolve(LinkCache);
            var edid = entryItem?.EditorID ?? "";
            if (entryItem == null || edid == null) continue;

            if (SpecialCases.DefaultMeatRegex.Matches(edid).Any())
            {
                if (entryItem is ILeveledItemGetter lvld)
                {
                    if (DebuggingMode) Write.Action(4, $"Meat search recursing into {DeathItemNamer(lvld)}");
                    if (lvld.Entries is not null && lvld.Entries.Count == 1 && FindDefaultMeat(lvld, std) is IItemGetter subItem)
                        return subItem;
                }
                else if (entryItem is IItemGetter item)
                {
                    if (DebuggingMode) Write.Action(4, $"Meat search found {ItemNamer(item)}");
                    return item;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Create a new default meat for a creature using a pre-existing meat as a template.
    /// Also creates cooked meat and jerky but they are not returned.
    /// 
    /// </summary>
    /// <param internalName="data"></param>
    /// <param internalName="std"></param>
    /// <returns></returns>
    /// <exception cref="CoreRecordMissing"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private IItemGetter CreateDefaultMeat(CreatureData data)
    {
        if (DebuggingMode) Write.Action(3, $"Creating new meats for {data.InternalName}");

        DEFAULT_MEAT.TryResolve(LinkCache, out var existingMeat);
        DEFAULT_COOKED.TryResolve(LinkCache, out var existingCooked);
        DEFAULT_JERKY.TryResolve(LinkCache, out var existingJerky);
        if (existingMeat == null) throw new CoreRecordMissing(DEFAULT_MEAT);
        if (existingCooked == null) throw new CoreRecordMissing(DEFAULT_COOKED);
        if (existingJerky == null) throw new CoreRecordMissing(DEFAULT_JERKY);

        var newMeat = PatchMod.Ingestibles.DuplicateInAsNewRecord(existingMeat);
        var newCooked = PatchMod.Ingestibles.DuplicateInAsNewRecord(existingCooked);
        var newJerky = PatchMod.Ingestibles.DuplicateInAsNewRecord(existingJerky);

        if (newMeat == null) throw new InvalidOperationException();
        if (newCooked == null) throw new InvalidOperationException();
        if (newJerky == null) throw new InvalidOperationException();

        newMeat.EditorID = $"_DS_Meat_{data.InternalName}";
        newCooked.EditorID = $"_DS_Food_{data.InternalName}Cooked";
        newJerky.EditorID = $"_DS_Food_{data.InternalName}Jerky";

        newMeat.Name = $"{data.Prototype.ProperName} Meat (raw)";
        newCooked.Name = $"{data.Prototype.ProperName} Meat (cooked)";
        newJerky.Name = $"{data.Prototype.ProperName} Jerky";

        newMeat.Keywords ??= new();
        newCooked.Keywords ??= new();
        newJerky.Keywords ??= new();

        if (data.Prototype.Meat == null && data.Prototype.CreateDefaultMeat) data.Prototype.Meat = newMeat.ToLink();
        if (!newMeat.HasKeyword(_DS_KW_Food_Raw)) newMeat.Keywords.Add(_DS_KW_Food_Raw);
        if (!newMeat.HasKeyword(Skyrim.Keyword.VendorItemFoodRaw)) newMeat.Keywords.Add(Skyrim.Keyword.VendorItemFoodRaw);

        if (IsCacoInstalled())
        {
            if (!newMeat.HasKeyword(VendorItemFoodMeat)) newMeat.Keywords.Add(VendorItemFoodMeat);
            if (!newMeat.HasKeyword(VendorItemFoodUncooked)) newMeat.Keywords.Add(VendorItemFoodUncooked);
            if (!newMeat.HasKeyword(EnableKeywordSpoil_CACO)) newMeat.Keywords.Add(EnableKeywordSpoil_CACO);
            if (!newCooked.HasKeyword(VendorItemFoodMeat)) newCooked.Keywords.Add(VendorItemFoodMeat);
            if (!newCooked.HasKeyword(VendorItemFoodCooked)) newCooked.Keywords.Add(VendorItemFoodCooked);
            if (!newJerky.HasKeyword(VendorItemFoodMeat)) newJerky.Keywords.Add(VendorItemFoodMeat);
            if (!newJerky.HasKeyword(VendorItemFoodPreserved)) newJerky.Keywords.Add(VendorItemFoodPreserved);
        }

        if (IsLastSeedInstalled())
        {
            if (!newMeat.HasKeyword(VendorItemFoodMeat)) newMeat.Keywords.Add(VendorItemFoodMeat);
            if (!newMeat.HasKeyword(_Seed_PO3_Detection_MeatRaw)) newMeat.Keywords.Add(_Seed_PO3_Detection_MeatRaw);
            if (!newCooked.HasKeyword(VendorItemFoodMeat)) newCooked.Keywords.Add(VendorItemFoodMeat);
            if (!newCooked.HasKeyword(_Seed_PO3_Detection_MeatCooked)) newCooked.Keywords.Add(_Seed_PO3_Detection_MeatCooked);
            if (!newJerky.HasKeyword(VendorItemFoodMeat)) newJerky.Keywords.Add(VendorItemFoodMeat);
            if (!newJerky.HasKeyword(_Seed_PO3_Detection_Preserved)) newJerky.Keywords.Add(_Seed_PO3_Detection_Preserved);
            if (!newJerky.HasKeyword(_Seed_PO3_Detection_Salted)) newJerky.Keywords.Add(_Seed_PO3_Detection_Salted);
            if (!newJerky.HasKeyword(VendorItemFoodPreserved)) newJerky.Keywords.Add(VendorItemFoodPreserved);
            if (!newJerky.HasKeyword(VendorItemFoodSalted)) newJerky.Keywords.Add(VendorItemFoodSalted);
        }

        // @TODO Fill this in with something better.
        newMeat.Value = (uint)data.Prototype.CarcassValue / 5;

        MeatSet meatSet = (newMeat, newCooked, newJerky);
        // Make recipes.
        CreateMeatRecipes(data, meatSet);

        if (DebuggingMode) Write.Success(3, $"Created new meats: {meatSet}");

        return newMeat;
    }

    /// <summary>
    /// Create a new discards formlist for a monster.
    /// Dicards are also called "negativetreasure" internally.
    /// </summary>
    /// 
    private FormList CreateDiscards(CreatureData data, PatchingRecords std)
    {
        var discards = PatchMod.FormLists.AddNew();
        if (discards == null) throw new InvalidOperationException();

        discards.EditorID = $"_DS_FL_Discard{data.InternalName}_01";
        discards.Items.AddRange(data.Prototype.Discard);
        std.Monsters.Discards.Objects.Add(CreateProperty(discards.ToLink()));

        return discards;
    }

    /// <summary>
    /// Create a new DeathDescriptor for a creature.
    /// This is a LeveledItem that gets added to a carcass's inventory when the
    /// player interacts with the carcass for the first time.
    /// 
    /// The Tokens it adds are what determine which actions the player can take.
    /// 
    /// </summary>
    /// 
    private (ILeveledItemGetter, List<IFormLinkGetter<IMiscItemGetter>>) 
        CreateDeathDescriptor(CreatureData data, IFormListGetter pelts, FormList mats, PatchingRecords std)
    {
        // Create the new descriptor.
        LeveledItem deathDescriptor = PatchMod.LeveledItems.AddNew();
        deathDescriptor.EditorID = $"_DS_DeathItem_{data.InternalName}";
        deathDescriptor.Entries = new();
        deathDescriptor.Flags = LeveledItem.Flag.UseAll;
        
        List<IFormLinkGetter<IMiscItemGetter>> tokens = new();

        // If the pelts FormList isn't empty, then harvesting pelts is enabled.
        if (pelts.Items is not null && pelts.Items.Count > 0) tokens.Add(_DS_Token_Pelt);

        // If the materials FormList isn't empty, then harvesting materials is enabled.
        if (mats.Items is not null && mats.Items.Count > 0) tokens.Add(_DS_Token_Mat);

        // Animals need to be cleaned. Monsters apparently not?
        if (data.IsAnimal) tokens.Add(_DS_Token_Carcass_Clean);

        // If the Meat field in the PluginEntry isn't null then harvesting meat is enabled.
        if (!data.Prototype.Meat.IsNull)
        {
            tokens.Add(_DS_Token_Meat);
            tokens.Add(_DS_Token_Meat_Fresh);
        }

        // If the Venom or Blood fields in the PluginEntry aren't null then harvesting venom and/or blood is enabled.
        if (data.IsMonster)
        {
            if (!data.Prototype.Venom.IsNull) tokens.Add(_DS_Token_Venom);
            if (!data.Prototype.BloodType.IsNull) tokens.Add(_DS_Token_Blood);
        }

        foreach (var token in tokens)
        {
            deathDescriptor.Entries.Add(CreateLeveledItemEntry(token, 1, 1));
        }

        // Add the creature's actual DeathItem to the appropriate FormList.
        // When hunterborn starts up, the creature's Token will be added to this formlist.
        // THIS IS HOW HUNTERBORN RECOGNIZES HARVESTABLE CREATURES.
        std.GetCCFor(data)._DS_FL_DeathItems.Items.Add(data.DeathItem);

        // Add the DeathDescriptor to the quest array property.
        std.GetCCFor(data).DeathDescriptors.Objects.Add(new() { Object = deathDescriptor.ToLink() });

        return (deathDescriptor, tokens);
    }

    /// <summary>
    /// Creates leather and pelt recipes for created leathers.
    /// </summary>
    /// 
    private void CreatePeltRecipes(CreatureData data, PeltSet pelts, bool createdStandardPelt)
    {
        if (data.Prototype.FurPlateCount.Length >= 3)
        {
            if (createdStandardPelt)
            {
                var standard = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_PELT_STD_RECIPE.Resolve(LinkCache));
                standard.CreatedObjectCount = (ushort)data.Prototype.PeltCount[1];
                if (standard.Items?[0].Item is ContainerItem containerItem2) containerItem2.Item = pelts.Item2.ToLink();
                if (standard.Conditions?[4].Data is FunctionConditionData data2) data2.ParameterOneRecord = pelts.Item2.ToLink();
                standard.EditorID = $"_DS_Recipe_Pelt_{data.InternalName}_01";
            }

            var poor = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_PELT_POOR_RECIPE.Resolve(LinkCache));
            var fine = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_PELT_FINE_RECIPE.Resolve(LinkCache));
            var flawless = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_PELT_FLAWLESS_RECIPE.Resolve(LinkCache));

            poor.EditorID = $"_DS_Recipe_Pelt_{data.InternalName}_00";
            fine.EditorID = $"_DS_Recipe_Pelt_{data.InternalName}_02";
            flawless.EditorID = $"_DS_Recipe_Pelt_{data.InternalName}_03";

            poor.CreatedObjectCount = (ushort)data.Prototype.PeltCount[0];
            fine.CreatedObjectCount = (ushort)data.Prototype.PeltCount[2];
            flawless.CreatedObjectCount = (ushort)data.Prototype.PeltCount[2];

            if (poor.Items?[0].Item is ContainerItem containerItem1) containerItem1.Item = pelts.Item1.ToLink();
            if (fine.Items?[0].Item is ContainerItem containerItem3) containerItem3.Item = pelts.Item3.ToLink();
            if (flawless.Items?[0].Item is ContainerItem containerItem4) containerItem4.Item = pelts.Item4.ToLink();

            if (poor.Conditions?[4].Data is FunctionConditionData data1) data1.ParameterOneRecord = pelts.Item1.ToLink();
            if (fine.Conditions?[4].Data is FunctionConditionData data3) data3.ParameterOneRecord = pelts.Item3.ToLink();
            if (flawless.Conditions?[4].Data is FunctionConditionData data4) data4.ParameterOneRecord = pelts.Item4.ToLink();

            fine.CreatedObject = pelts.Item2.ToNullableLink();
            flawless.CreatedObject = pelts.Item3.ToNullableLink();

            if (DebuggingMode) Write.Success(3, $"Created new tanning recipes.");
        }

        if (data.Prototype.FurPlateCount.Length >= 3)
        {
            var poor = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_FURS_POOR_RECIPE.Resolve(LinkCache));
            var standard = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_FURS_STD_RECIPE.Resolve(LinkCache));
            var fine = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_FURS_FINE_RECIPE.Resolve(LinkCache));

            poor.EditorID = $"HB_Recipe_FurPlate_{data.InternalName}_00";
            standard.EditorID = $"HB_Recipe_FurPlate_{data.InternalName}_01";
            fine.EditorID = $"HB_Recipe_FurPlate_{data.InternalName}_02";

            poor.CreatedObjectCount = (ushort)data.Prototype.FurPlateCount[0];
            standard.CreatedObjectCount = (ushort)data.Prototype.FurPlateCount[1];
            fine.CreatedObjectCount = (ushort)data.Prototype.FurPlateCount[2];

            if (poor.Items?[0].Item is ContainerItem containerItem1) containerItem1.Item = pelts.Item1.ToLink();
            if (standard.Items?[0].Item is ContainerItem containerItem2) containerItem2.Item = pelts.Item2.ToLink();
            if (fine.Items?[0].Item is ContainerItem containerItem3) containerItem3.Item = pelts.Item3.ToLink();

            if (poor.Conditions?[4].Data is FunctionConditionData data1) data1.ParameterOneRecord = pelts.Item1.ToLink();
            if (standard.Conditions?[4].Data is FunctionConditionData data2) data2.ParameterOneRecord = pelts.Item2.ToLink();
            if (fine.Conditions?[4].Data is FunctionConditionData data3) data3.ParameterOneRecord = pelts.Item3.ToLink();

            if (DebuggingMode) Write.Success(3, $"Created new fur-plate recipes.");
        }
    }

    /// <summary>
    /// Creates standard, campfire, primitive cooking, and jerky recipes for created meats.
    /// </summary>
    /// 
    private void CreateMeatRecipes(CreatureData data, MeatSet meats)
    {
        var recipeCooked = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_CHARRED_RECIPE.Resolve(LinkCache));
        var recipeCampfire = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_CAMPFIRE_RECIPE.Resolve(LinkCache));
        var recipePrimitive = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_PRIMITIVE_RECIPE.Resolve(LinkCache));
        var recipeJerky = PatchMod.ConstructibleObjects.DuplicateInAsNewRecord(DEFAULT_JERKY_RECIPE.Resolve(LinkCache));

        (var meat, var cooked, var jerky) = meats;

        recipeCooked.EditorID = $"_DS_Recipe_Food_CharredMeat_{data.InternalName}";
        recipeCampfire.EditorID = $"HB_Recipe_FireFood_CharredMeat_{data.InternalName}";
        recipePrimitive.EditorID = $"_DS_Recipe_Food_Primitive_CharredMeat_{data.InternalName}";
        recipeJerky.EditorID = $"_DS_Recipe_Food_{data.InternalName}Jerky";

        if (recipeCooked.Items?[0].Item is ContainerItem containerItem0) containerItem0.Item = meat.ToLink();
        if (recipeCampfire.Items?[0].Item is ContainerItem containerItem1) containerItem1.Item = meat.ToLink();
        if (recipePrimitive.Items?[0].Item is ContainerItem containerItem2) containerItem2.Item = meat.ToLink();
        if (recipeJerky.Items?[1].Item is ContainerItem containerItem3) containerItem3.Item = meat.ToLink();

        if (recipeCooked.Conditions?[1].Data is ConditionData data0) data0.Reference = meat.ToLink();
        if (recipeJerky.Conditions?[3].Data is ConditionData data3) data3.Reference = meat.ToLink();

        recipeCooked.CreatedObject = cooked.ToNullableLink();
        recipeJerky.CreatedObject = jerky.ToNullableLink();

        if (DebuggingMode) Write.Success(3, $"Created new meat recipes.");
    }

    /// <summary>
    /// Checks a Settings object to see if it is populated or not. 
    /// Prints out status information.
    /// </summary>
    /// <param name="label">A label stating where the Settings came from.</param>
    /// <param name="settings">The Settings object.</param>
    /// <returns>A flag indicating whether the Settings is populated.</returns>
    public static bool CheckSettings(string label, Settings.Settings settings)
    {
        Write.Title(0, label);
        Write.Success(2, $"{settings.QuickLootPatch}");
        Write.Success(2, $"{settings.ReuseSelections}");
        Write.Success(2, $"Selections: {settings.DeathItemSelections.Length}");
        Write.Success(2, $"Plugins:    {settings.PluginEntries.Count}");
        return settings.DeathItemSelections.Length > 0 && settings.PluginEntries.Count > 0;
    }

    /// <summary>
    /// Convenience method for creating new LeveledItemEntry.
    /// No extra data is added.
    /// </summary>
    /// <param internalName="item">The entryItem.</param>
    /// <param internalName="level">The player level.</param>
    /// <param internalName="count">The entryItem count.</param>
    /// <returns></returns>
    static LeveledItemEntry CreateLeveledItemEntry(IFormLinkGetter<IItemGetter> item, int level, int count) =>
        new() { Data = new LeveledItemEntryData { Reference = (IFormLink<IItemGetter>)item, Level = (short)level, Count = (short)count } };

    /// <summary>
    /// Convenience method for creating new ScriptObjectProperty wrapping a FormLink.
    /// It has to turn the Getter into a Setter internally.
    /// </summary>
    /// <param internalName="item">The FormLinkGetter.</param>
    /// <returns>The ScriptObjectProperty.</returns>
    static ScriptObjectProperty CreateProperty<T>(IFormLinkGetter<T> item) where T : class, ISkyrimMajorRecordGetter
    {
        var link = item.FormKey.ToLink<ISkyrimMajorRecordGetter>();
        return new() { Object = link };
    }

    public bool IsCacoInstalled() => LoadOrder.ContainsKey(CACO_MODKEY);

    public bool IsLastSeedInstalled() => LoadOrder.ContainsKey(LASTSEED_MODKEY);

    private Func<IFormLinkGetter<IItemGetter>, IFormLinkGetter<IItemGetter>> FormLinkSubstitution { get; }

    private Func<IItemGetter, IItemGetter> ItemSubstitution { get; }

    private string NpcNamer(INpcGetter npc) => FormNamer(npc, () => LinkNamer(npc.Race, () => FormIDNamer(npc)));

    private static string DeathItemNamer(DeathItemGetter deathItem) => FormNamer(deathItem);

    private static string ItemNamer(IItemGetter item) => FormNamer(item);

    private string ItemNamer(IFormLinkGetter<IMiscItemGetter> link)
    {
        LinkCache.TryResolve(link, out var item);
        return FormNamer(item);
    }

    //private string NpcNamerFallback(INpcGetter npc) => NpcNamer(npc) ?? NpcRaceNamer(npc) ?? FormIDNamer(npc);
    //static private string DeathItemNamerFallback(DeathItemGetter deathItem) => DeathItemNamer(deathItem) ?? FormIDNamer(deathItem);
    //static private string ItemNamerFallback(IItemGetter item)
    //=> item.EditorID ?? FormIDNamer(item);
    //private string? NpcRaceNamer(INpcGetter npc) => RaceNamer(npc.Race.Resolve(LinkCache));
    //private string? NpcDeathItemNamer(INpcGetter npc) => DeathItemNamer(npc.DeathItem?.Resolve(LinkCache));

    /// <summary>
    /// Wraps for <code>BasicNamer</code> for <code>INpcGetter</code>.
    /// </summary>
    //static private string? NpcNamer(INpcGetter npc) => FormNamer(npc);

    //static private string? RaceNamer(IRaceGetter? race) => FormNamer(race);
    //static private string? DeathItemNamer(DeathItemGetter? deathItem) => deathItem?.EditorID;
    static private string FormIDNamer(IMajorRecordGetter? thing) => thing?.FormKey.IDString() ?? "NULL";

    /// <summary>
    /// Returns name of <code>thing</code>. 
    /// If no name is available, returns the EditorID of <code>thing</code>.
    /// If no EditorID is available, returns null;
    /// </summary>
    /// 
    private string LinkNamer(IFormLinkGetter<IMajorRecordGetter>? thing, Func<string?>? fallback = null)
    {
        if (thing is null) return "<NULL>";
        thing.TryResolve(LinkCache, out var form);
        return FormNamer(form, fallback);
    }

    /// <summary>
    /// Returns name of <code>thing</code>. 
    /// If no name is available, returns the EditorID of <code>thing</code>.
    /// If no EditorID is available, returns null;
    /// </summary>
    /// 
    static private string FormNamer(IMajorRecordGetter? thing, Func<string?>? fallback = null)
    {
        if (thing is INamedGetter named && named.Name?.ToString() is string name) return name;
        else if (thing is ISkyrimMajorRecordGetter rec && rec.EditorID is string edid) return edid;
        else if (fallback is not null && fallback() is string fallbackName) return fallbackName;
        else return FormIDNamer(thing);
    }

    public record CreatureData(DeathItemGetter DeathItem, string InternalName, string DescriptiveName, PluginEntry Prototype, bool IsAnimal, bool IsMonster);
    private static Lazy<Settings.Settings> _settings = null!;

    /// <summary>
    /// Default carcasses for each Plugin, so that they can be copied.
    /// This is useful if the default carcass has an interesting model or keywords.
    /// </summary>
    private Dictionary<PluginEntry, IMiscItemGetter> KnownCarcasses { get; } = new();

    /// <summary>
    /// Contains prototypes for which a full pelt set and recipe set already exist.
    /// </summary>
    private Dictionary<PluginEntry, PeltSet> KnownPelts { get; } = new();

    /// <summary>
    /// Associates DeathItems with plugins. Mainly used to avoid processing a DeathItem more than once.
    /// </summary>
    private OrderedDictionary<DeathItemGetter, PluginEntry> KnownDeathItems { get; } = new();

    /// <summary>
    /// The original indices of the internalplugins in the hunterborn plugin.
    /// </summary>
    private Dictionary<InternalPluginEntry, int> OriginalIndices { get; set; } = new();

    //private string PluginsPath { get; }
    private Settings.Settings Settings { get; }
    private IPatcherState<ISkyrimMod, ISkyrimModGetter> State { get; }
    private ILinkCache<ISkyrimMod, ISkyrimModGetter> LinkCache { get; }
    private ILoadOrder<IModListing<ISkyrimModGetter>> LoadOrder { get; }
    private ISkyrimMod PatchMod { get; }
    private bool DebuggingMode { get { return Settings.DebuggingMode; } }

    private readonly AdvancedTaxonomy Taxonomy = new();

}

