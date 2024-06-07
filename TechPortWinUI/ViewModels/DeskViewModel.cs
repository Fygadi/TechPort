using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Reflection;
using TechPortWinUI.Desk;

namespace TechPortWinUI.ViewModels
{
    public partial class DeskViewModel : ObservableRecipient
    {

        //private string _desksConfigPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\DesksConfig.json";
        //TODO: Update the Path of the file
        private const string _desksConfigPath = $"E:\\TechPort\\DesksConfig.json";

        private readonly ObservableCollection<IDesk> _desks = new();
        public ObservableCollection<IDesk> Desks => _desks;

        private IDesk _activeDesk = new DefaultDesk();
        public IDesk ActiveDesk { get => _activeDesk; private set => _activeDesk = value; }

        #region Default constructor
        public DeskViewModel()
        {
            _ = CreateDesksFromJsonAsync();
        }
        #endregion

        public void AddDesk(IDesk desk) => Desks.Add(desk);
        public void RemoveDesk(IDesk desk) => Desks.Remove(desk);

        [RelayCommand]
        public void MoveUp() => MoveToHeight(ActiveDesk.MaxHeight);
        [RelayCommand]
        public void MoveDown() => MoveToHeight(ActiveDesk.MinHeight);
        [RelayCommand]
        public void MoveToHeight(short targetHeight) => ActiveDesk.MoveToHeightAsync(targetHeight);
        [RelayCommand]
        public void StopMovement() => ActiveDesk.StopMovingAsync();

        private void SaveDesksToJson()
        {
            var deskDataList = new List<object>();

            foreach (var desk in Desks)
            {
                var deskData = new
                {
                    TypeFullName = desk.GetType().FullName,
                    Id = desk.DeviceId
                };
                deskDataList.Add(deskData);
            }
            var finalJson = JsonConvert.SerializeObject(deskDataList, Formatting.Indented);
            File.WriteAllText(_desksConfigPath, finalJson);
        }

        private async Task CreateDesksFromJsonAsync()
        {
            if (!File.Exists(_desksConfigPath))
                return;

            string json = await File.ReadAllTextAsync(_desksConfigPath);
            List<dynamic>? desksData = JsonConvert.DeserializeObject<List<dynamic>>(json);

            if (desksData == null)
                return;

            foreach (var deskData in desksData)
            {
                string typeFullName = deskData.TypeFullName;
                string id = deskData.Id;

                try
                {
                    //TODO: Implement Idesk instead of IdasenDesk
                    Type type = Type.GetType(typeFullName);
                    MethodInfo methodInfo = type.GetMethod(nameof(IDesk.CreateAsync));
                    Task<IdasenDesk> method = (Task<IdasenDesk>)methodInfo.Invoke(null, new object[] { id });
                    IDesk result = await method;

                    _activeDesk = result;
                    ActiveDesk.UpdateProperty();
                }
                catch (Exception ex) { }
            }
        }
    }
}