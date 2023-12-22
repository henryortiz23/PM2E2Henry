using PM2E2Henry.Models;
using PM2E2Henry.ViewModels;

namespace PM2E2Henry.Views;

public partial class PageListFirmas : ContentPage
{
    ViewModelSitios _viewModel;
    Sitios sitioSelected;


    public PageListFirmas()
    {
        InitializeComponent();
        ShowToolbarItems(true, false, false);

        
        
    }


    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await Task.Run(() => getDatos());

    }
    private async Task getDatos()
    {

        _viewModel = new ViewModelSitios("", true, stack_background);
        this.BindingContext = _viewModel;
    }


    private async void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Sitios selectedSitio)
        {
            if (selectedSitio != null)
            {
                sitioSelected = selectedSitio;
                ShowToolbarItems(true, true, true);
            }
            else
            {
                ShowToolbarItems(true, false, false);
            }
        }
    }



    private void ShowToolbarItems(bool mAdd, bool mEdit, bool mDel)
    {
        ToolbarItems.Clear();

        if (mAdd)
        {
            ToolbarItems.Add(new ToolbarItem("Crear sitio", "map_add.svg", NuevoSitio));
        }

        if (mEdit)
        {
            ToolbarItems.Add(new ToolbarItem("Editar sitio", "map_edit.svg", ModificarSitio));
        }

        if (mDel)
        {
            ToolbarItems.Add(new ToolbarItem("Eliminar sitio", "map_del.svg", EliminarSitio));
        }
    }

    private async void NuevoSitio()
    {
        await Navigation.PushAsync(new NuevoSitioPage(null));
    }

    private async void ModificarSitio()
    {
        bool resp = await DisplayAlert("Confirmar", "Desea modificar el sitio seleccionado?", "Si", "No");

        if (resp)
        {
            await Navigation.PushAsync(new NuevoSitioPage(sitioSelected));
            ShowToolbarItems(true,false, false);
        }


    }
        private async void EliminarSitio()
    {
        bool resp = await DisplayAlert("Confirmar", "Desea eliminar el sitio seleccionado?", "Si", "No");

        if (resp)
        {
            try
            {
                await _viewModel.DeleteData(sitioSelected.id);
                await DisplayAlert("Eliminado","Sitio selecionado eliminado correctamente","Cerrar");
                ShowToolbarItems(true, false, false);
            }catch(Exception ex)
            {
                await DisplayAlert("Error", "No se pudo eliminar el sitio selecionado\nError: "+ex.Message, "Cerrar");
            }
        }
        
    }

}


