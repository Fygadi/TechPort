using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using TechPort.Desk;
using TechPortWinUI.Desk;

namespace TechPortWinUI.ViewModels
{
    public partial class DeskViewModel : ObservableObject
    {
        public ObservableCollection<IDesk> Desks { get => _desks; }
        public IDesk ActiveDesk { get => _activeDesk; }

        //private string _desksConfigPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\DesksConfig.json";
        //TODO: Update the Path of the file
        private const string _desksConfigPath = $"E:\\TechPort\\DesksConfig.json";
        private readonly ObservableCollection<IDesk> _desks = new();
        private IDesk _activeDesk = new DefaultDesk();

        #region Default constructor
        public DeskViewModel()
        {
            //_ = CreateDesksFromJson();
        }
        #endregion

        public void AddDesk(IDesk desk) => Desks.Add(desk);
        public void RemoveDesk(IDesk desk) => Desks.Remove(desk);

        [RelayCommand]
        public void MoveUp() => ActiveDesk.MoveUpAsync();
        [RelayCommand]
        public void MoveDown() => ActiveDesk.MoveDownAsync();
        [RelayCommand]
        public void MoveToHeight(short targetHeight) => ActiveDesk.MoveToHeightAsync(targetHeight);

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
        private async Task CreateDesksFromJson()
        {
            try
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

                    //IDesk result = await (Task<IDesk>)Type.GetType(typeFullName).GetMethod(nameof(IDesk.CreateAsync)).Invoke(null, new object[] { id });

                    try
                    {
                        Type? deskType = Type.GetType(typeFullName);
                        MethodInfo? methodCreateAsync = deskType?.GetMethod(nameof(IDesk.CreateAsync));
                        var result = methodCreateAsync?.Invoke(null, new object[] { id });
                        var a = await (Task<IDesk>)result;

                        //IDesk result = await (Task<IDesk>)Type.GetType(typeFullName).GetMethod(nameof(IDesk.CreateAsync)).Invoke(null, new object[] { id });


                        //_desks.Clear();
                        //_desks.Add(result);

                        //if (_activeDesk == null)
                        //    _activeDesk = result;
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}