using Firebase.Database;
using Firebase.Database.Query;
using PM2E2Henry.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PM2E2Henry.ViewModels
{
    public class ViewModelSitios
    {
        private FirebaseClient _firebase;
        private string childString;

        public ObservableCollection<Sitios> DataItems { get; } = new ObservableCollection<Sitios>();

        public ViewModelSitios(string child, bool cargar, StackLayout stackDialog)
        {
            childString = child;
            _firebase = new FirebaseClient(new Controllers.Config().GetUrlMain());

            if (cargar)
                ListenToChanges(stackDialog);
        }

        private void ListenToChanges(StackLayout stackDialog)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!stackDialog.IsVisible)
                    stackDialog.IsVisible = true;
            });
            _firebase
                .Child(childString)
                .AsObservable<Sitios>()
                .Subscribe(args =>
                {
                    if (args.Object == null)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (stackDialog.IsVisible)
                                stackDialog.IsVisible = false;
                        });
                    }
                    else  if (args.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                    {
                        var newItem = args.Object;
                        newItem.id = args.Key;
                        var existingItem = DataItems.FirstOrDefault(x => x.id == newItem.id);

                        if (existingItem != null)
                        {
                            int index = GetIndexId(newItem.id);

                            DataItems.RemoveAt(index);
                            DataItems.Insert(index, newItem);
                        }
                        else
                        {
                            DataItems.Add(newItem);
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (stackDialog.IsVisible)
                                stackDialog.IsVisible = false;
                        });

                    }
                    else if (args.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                    {
                        var itemToRemove = DataItems.FirstOrDefault(x => x.id == args.Key);
                        if (itemToRemove != null)
                        {
                            DataItems.Remove(itemToRemove);
                        }
                    }

                });


        }

        public async Task InsertData(Sitios newItem)
        {
            await _firebase
            .Child(childString)
            .PostAsync(newItem);
        }

        public async Task UpdateData(Sitios updatedItem, String id)
        {
            await _firebase
                .Child(childString)
                .Child(id)
                .PutAsync(updatedItem);
        }

        public async Task DeleteData(string itemId)
        {
            await _firebase
                .Child(childString)
                .Child(itemId)
                .DeleteAsync();
        }

        public int GetIndexId(string itemId)
        {
            for (int i = 0; i < DataItems.Count; i++)
            {
                if (DataItems[i].id == itemId)
                {
                    return i; // Devuelve el índice si se encuentra el elemento
                }
            }
            return -1; // Devuelve -1 si no se encuentra el elemento con ese Id
        }


    }
}

