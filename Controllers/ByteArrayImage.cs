using System.Globalization;

namespace PM2E2Henry.Controllers
{
    internal class ByteArrayImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameters, CultureInfo culture)
        {
            ImageSource image = null;
            
            string base64String = (string)value;
            byte[] bytesx = System.Convert.FromBase64String(base64String);
                
            
            if (value != null)
            {
                var stream = new MemoryStream(bytesx);
                image = ImageSource.FromStream(() => stream);
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}