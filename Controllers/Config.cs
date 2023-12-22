namespace PM2E2Henry.Controllers
{
    public class Config
    {
        private string Url;

        public Config()
        {
            Url = "https://nailbar-e4252-default-rtdb.firebaseio.com/PM2E2Henry/";
        }
        public string GetUrlMain()
        {
            return Url;
        }

    }

}
