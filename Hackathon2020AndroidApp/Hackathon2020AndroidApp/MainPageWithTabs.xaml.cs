using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hackathon2020AndroidApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageWithTabs : TabbedPage
    {
        public MainPageWithTabs()
        {
            InitializeComponent();
            Children.Add(new MainPage());
            Children.Add(new CameraMainPage());
        }
    }
}