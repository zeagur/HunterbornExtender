using Autofac;
using HunterbornExtenderUI;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HunterbornExtenderUI
{
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

            container.Resolve<StateProvider>();
            container.Resolve<EDIDtoForm>();

            var mvm = container.Resolve<MainWindowVM>();
            this.DataContext = mvm;
        }
    }
}
