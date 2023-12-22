using Plugin.Maui.Audio;
using PM2E2Henry.Models;
using PM2E2Henry.ViewModels;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace PM2E2Henry.Views
{
    public partial class NuevoSitioPage : ContentPage
    {
        ViewModelSitios _viewModel;
        IAudioManager audioManager;
        IAudioRecorder audioRecorder;
        AsyncAudioPlayer audioPlayer;
        IAudioSource audioSource = null;
        bool modificoFirma;
        bool modificoAudio;
        bool isPlaying;
        bool isRecorder;
        Sitios sitioActual;
        private Stream streamImage = null;
        public NuevoSitioPage(Sitios datosSitio)
        {
            InitializeComponent();
            if (datosSitio != null)
            {
                sitioActual = datosSitio;
                CargarDatos();
            }
            else
            {
                ObtenerUbicacion();
            }

            

        }

        async void ObtenerUbicacion()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Default);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    txtlatitud.Text = location.Latitude.ToString();
                    txtlongitud.Text = location.Longitude.ToString();
                }
                
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Error", "La geolocalización no es compatible con este dispositivo", "Ok");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Error","No se otorgo permiso para obtener la ubicacion","Ok");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Ocurrio un error\nError: "+ex.Message, "Ok");
            }
        }


        private async void CargarDatos()
        {

            if (sitioActual != null)
            {
                entDescripcion.Text = sitioActual.descripcion;

                await Task.Run(() => obtenerFirma());
                await Task.Run(() => obtenerAudio());
                if (sitioActual != null)
                {
                    imgFirma.IsVisible = true;
                    firma.IsVisible = false;

                    if (!"".Equals(sitioActual.archivo_audio))
                    {
                        btnPlay.IsEnabled = true;
                        btnPlay.ImageSource = "play.svg";
                        btnStart.Text = "Regrabar audio";
                    }

                    txtlatitud.Text = sitioActual.latitud;
                    txtlongitud.Text = sitioActual.longitud;
                    btnMap.IsVisible = true;
                }
            }


        }

        private void creandoFirma(object sender, CommunityToolkit.Maui.Core.DrawingLineCompletedEventArgs e)
        {
            ImageView.Dispatcher.Dispatch(async () =>
            {
                var stream = await firma.GetImageStream(300, 300);
                streamImage = stream;
            });
        }
        private void btnBorrar_Clicked(object sender, EventArgs e)
        {
            borrar();
        }

        private async void borrar()
        {
            firma.Lines.Clear();
            streamImage = null;
            if (sitioActual != null)
            {
                bool resp = await DisplayAlert("Confirmar", "Desea borrar la firma actua y crear una nueva?", "Si", "No");

                if (resp)
                {
                    imgFirma.IsVisible = false;
                    firma.IsVisible = true;
                    modificoFirma = true;
                    //imgFirma.Source = null;
                }
            }
        }



        private async void btnCancelar_clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        public string ImageToBase64()
        {
            if (streamImage != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    Stream stream = streamImage;//image.GetStream();
                    stream.CopyTo(memory);
                    byte[] data = memory.ToArray();
                    string base64 = Convert.ToBase64String(data);

                    return base64;
                }
            }

            return null;
        }

        public string AudioToBase64()
        {
            if (audioSource != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    Stream stream = audioSource.GetAudioStream();
                    stream.CopyTo(memory);
                    byte[] data = memory.ToArray();
                    string base64 = Convert.ToBase64String(data);

                    return base64;
                }
            }

            return null;
        }

        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {

            dialogSave.IsVisible = true;
            await InsertarAsync();
            //await Task.Run(() => InsertarAsync());
            dialogSave.IsVisible = false;

        }

        private async Task InsertarAsync()
        {

            if (sitioActual == null)
            {
                var newSitio = new Sitios
                {
                    descripcion = entDescripcion.Text,
                    latitud = txtlatitud.Text,
                    longitud = txtlongitud.Text,
                    firma_digital = ImageToBase64(),
                    archivo_audio = AudioToBase64()
                };


                //dialogSave.IsVisible = true;
                await Insertando(newSitio);
                //dialogSave.IsVisible = false;
            }
            else
            {
                Sitios sitioModificado = new Sitios
                {
                    descripcion = entDescripcion.Text,
                    latitud = sitioActual.latitud,
                    longitud = sitioActual.longitud,
                    firma_digital = modificoFirma ? ImageToBase64() : sitioActual.firma_digital,
                    archivo_audio = modificoAudio ? AudioToBase64() : sitioActual.archivo_audio
                };
                //dialogSave.IsVisible = true;
                await Actualizando(sitioModificado);
                //dialogSave.IsVisible = false;
            }
        }

        private async Task Insertando(Sitios newItem)
        {
            try
            {
                ViewModelSitios viewModel = new ViewModelSitios("", false, null);

                await viewModel.InsertData(newItem);
                await DisplayAlert("Éxito", "Información almacenada correctamente.", "Cerrar");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudo guardar la información\nCódigo de error: " + ex.Message, "Cerrar");
            }

        }

        private async Task Actualizando(Sitios newItem)
        {
            try
            {
                ViewModelSitios viewModel = new ViewModelSitios("", false, null);

                await viewModel.UpdateData(newItem, sitioActual.id);
                await DisplayAlert("Éxito", "Información actualizada correctamente.", "Cerrar");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                
                await DisplayAlert("Error", "No se pudo actualizar la información\nCódigo de error: " + ex.Message, "Cerrar");
            }

        }

        private async void StartAudio(object sender, EventArgs e)
        {
            if (await ComprobarPermisos<Microphone>())
            {
                if (audioManager == null)
                {
                    audioManager = Plugin.Maui.Audio.AudioManager.Current;
                }


                if (!isRecorder)
                {
                    if (sitioActual != null && audioRecorder == null)
                    {
                        bool resp = await DisplayAlert("Confirmar", "Desea volver a grabar el audio?", "Si", "No");

                        if (resp)
                        {
                            modificoAudio = true;

                            isRecorder = true;
                            audioRecorder = audioManager.CreateRecorder();

                            btnPlay.IsEnabled = false;
                            btnPlay.ImageSource = "play_disabled.svg";

                            await audioRecorder.StartAsync();
                            btnStart.Text = "Detener grabación";
                        }
                    }
                    else
                    {
                        isRecorder = true;
                        audioRecorder = audioManager.CreateRecorder();

                        await audioRecorder.StartAsync();
                        btnStart.Text = "Detener grabación";
                    }

                }
                else
                {
                    isRecorder = false;
                    audioSource = await audioRecorder.StopAsync();
                    btnStart.Text = "Regrabar audio";
                    btnPlay.IsEnabled = true;
                    btnPlay.ImageSource = "play.svg";
                }
            }
        }

        private async void Play(object sender, EventArgs e)
        {
            if (audioSource != null)
            {
                audioPlayer = this.audioManager.CreateAsyncPlayer(((FileAudioSource)audioSource).GetAudioStream());

                btnPlay.ImageSource = "stop.svg";
                isPlaying = true;
                await audioPlayer.PlayAsync(CancellationToken.None);
                isPlaying = false;
                btnPlay.ImageSource = "play.svg";
            }
            else
            {
                if (sitioActual != null)
                {
                    if (!"".Equals(sitioActual.archivo_audio))
                    {
                        btnPlay.ImageSource = "stop.svg";
                        isPlaying = true;
                        await audioPlayer.PlayAsync(CancellationToken.None);
                        isPlaying = false;
                        btnPlay.ImageSource = "play.svg";
                    }
                }
            }

        }

        public static async Task<bool> ComprobarPermisos<TPermission>() where TPermission : BasePermission, new()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

            if (status == PermissionStatus.Granted)
            {
                return true;
            }

            if (Permissions.ShouldShowRationale<TPermission>())
            {

            }

            status = await Permissions.RequestAsync<TPermission>();

            return status == PermissionStatus.Granted;
        }

        private async Task obtenerFirma()
        {
            string base64String = sitioActual.firma_digital;
            if (base64String != null && !string.IsNullOrEmpty(base64String))
            {
                byte[] bytes = System.Convert.FromBase64String(base64String);
                var stream = new MemoryStream(bytes);
                imgFirma.Source = ImageSource.FromStream(() => stream);
                imgFirma.IsVisible = true;
                firma.IsVisible = false;
            }
        }

        private async Task obtenerAudio()
        {
            try
            {
                if (sitioActual != null && !string.IsNullOrEmpty(sitioActual.archivo_audio))
                {
                    byte[] bytes = System.Convert.FromBase64String(sitioActual.archivo_audio);
                    var stream = new MemoryStream(bytes);

                    if (audioManager == null)
                    {
                        audioManager = Plugin.Maui.Audio.AudioManager.Current;
                    }

                    audioPlayer = audioManager.CreateAsyncPlayer(stream);
                }
            }
            catch (Exception ex)
            {

            }
        }


        private bool comprobar()
        {
            if (sitioActual == null)//Nuevo sitio
            {
                if (streamImage == null)
                {
                    DisplayAlert("Firma necesaria", "Debe proporcionar una firma", "Ok");
                    return false;
                }
                if (audioSource == null)
                {
                    DisplayAlert("Audio necesario", "Debe proporcionar un audio", "Ok");
                    return false;
                }
                if ("".Equals(entDescripcion))
                {
                    DisplayAlert("Descripcion necesaria", "Debe proporcionar una descripcion", "Ok");
                    entDescripcion.Focus();
                    return false;
                }

                return true;
            }
            else
            {
                if (modificoFirma)
                {
                    if (streamImage == null)
                    {
                        DisplayAlert("Firma necesaria", "Debe proporcionar una firma", "Ok");
                        return false;
                    }
                }
                
                if (modificoAudio)
                {
                    if (audioSource == null)
                    {
                        DisplayAlert("Audio necesario", "Debe proporcionar un audio", "Ok");
                        return false;
                    }
                }
                if ("".Equals(entDescripcion))
                {
                    DisplayAlert("Descripcion necesaria", "Debe proporcionar una descripcion", "Ok");
                    entDescripcion.Focus();
                    return false;
                }
                return true;
            }
            //return false;
        }

        private async void Tapped_Mostrar_Ubicacion(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new PageMap(sitioActual));
        }
    }


}