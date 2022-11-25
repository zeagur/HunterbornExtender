using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;

namespace HunterbornExtenderUI
{
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

            //var window = new MainWindow();
            //window.Show();
        }

        /*
        public static int Initialize(System.Drawing.Rectangle r)
        {

            var window = new MainWindow();
            return 0;
        }*/
    }
}
