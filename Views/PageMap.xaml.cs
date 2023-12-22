using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using PM2E2Henry.Models;

namespace PM2E2Henry.Views;

public partial class PageMap : ContentPage
{
    private Sitios sitioSeleccionado;
	public PageMap(Sitios datosSitio)
	{
		InitializeComponent();
        if (datosSitio != null)
        {
            sitioSeleccionado = datosSitio;
            cargarMapa();
        }
	}


    private async void cargarMapa() { 

        Title = "Ubicacion de " + sitioSeleccionado.descripcion;
        
            double latitud = Convert.ToDouble(sitioSeleccionado.latitud);
            double longitud = Convert.ToDouble(sitioSeleccionado.longitud);
            string nombre = sitioSeleccionado.descripcion;
            Location location = new Location(latitud, longitud);
            var pin = new Pin { Type = PinType.Place, Location=location, Label = nombre};

            map.Pins.Add(pin);

        map.MoveToRegion(new MapSpan(location, 0.1, 0.1));        
    }
}