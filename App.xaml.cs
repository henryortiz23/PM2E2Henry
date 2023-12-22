namespace PM2E2Henry
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Views.PageListFirmas());//new AppShell();
        }
    }
}