using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HunterbornExtenderUI
{
    public class MainWindowVM : VM
    {
        private StateProvider _stateProvider;
        private PluginLoader _pluginLoader;
        private DataState _dataState;

        [Reactive]
        public object DisplayedSubView { get; set; }
        public VM_WelcomePage WelcomePage { get; set; }
        public VM_DeathItemAssignmentPage DeathItemMenu { get; set; }
        public VM_PluginEditorPage PluginEditorPage { get; set; }

        [Reactive]
        public RelayCommand ClickDeathItemAssignment { get; }
        [Reactive]
        public RelayCommand ClickPluginsMenu { get; }
        [Reactive]
        public RelayCommand Test { get; }
        public MainWindowVM(StateProvider stateProvider, PluginLoader pluginLoader, DataState dataState)
        {
            _stateProvider = stateProvider;
            _pluginLoader = pluginLoader;
            _dataState = dataState;

            WelcomePage = new(_dataState);
            PluginEditorPage = new(_stateProvider);
            DeathItemMenu = new(_stateProvider, _dataState);

            Init();

            //DisplayedSubView = WelcomePage;
            DisplayedSubView = PluginEditorPage;

            ClickDeathItemAssignment = new RelayCommand(
                canExecute: _ => true,
                execute: _ => DisplayedSubView = DeathItemMenu
            );

            ClickPluginsMenu = new RelayCommand(
                canExecute: _ => true,
                execute: _ => DisplayedSubView = PluginEditorPage
            );

            Test = new RelayCommand(
                canExecute: _ => true,
                execute: _ => MessageBox.Show("Test")
            );
        }
        public void Init()
        {
            _dataState.Plugins = _pluginLoader.LoadPlugins();
            PluginEditorPage.Plugins = new VMLoader_Plugins(_stateProvider).GetPluginVMs(_dataState.Plugins);
        }
    }
}
