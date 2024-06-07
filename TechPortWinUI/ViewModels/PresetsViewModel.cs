using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Windows.System;

namespace TechPortWinUI.ViewModels
{
    /// <summary>
    /// Represent a list of PresetItem
    /// </summary>
    public partial class PresetsViewModel : ObservableRecipient
    {
        #region Fields
        private readonly ObservableCollection<PresetItem> _presets = CreatePresetsFromJson();
        private const string _presetsConfigPath = $"E:\\TechPort\\PresetsConfig.json";
        #endregion

        #region Property
        public ObservableCollection<PresetItem> Presets { get => _presets; }
        #endregion

        #region Constructor
        public PresetsViewModel()
        {
            //_presets = GetPresetsFromJson();
        }
        #endregion

        [RelayCommand]
        public void AddPreset(PresetItem preset) => _presets.Add(preset);
        [RelayCommand]
        public void RemovePreset(PresetItem preset) => _presets.Remove(preset);

        private static ObservableCollection<PresetItem> CreatePresetsFromJson()
        {
            if (!File.Exists(_presetsConfigPath))
                return new ObservableCollection<PresetItem>();

            string json = File.ReadAllText(_presetsConfigPath);
            ObservableCollection<PresetItem>? presets = JsonConvert.DeserializeObject<ObservableCollection<PresetItem>>(json);
            return presets ?? new ObservableCollection<PresetItem>();
        }
        public async void SavePresetsToJson()
        {
            string finalJson = JsonConvert.SerializeObject(_presets, Formatting.Indented);
            await File.WriteAllTextAsync(_presetsConfigPath, finalJson);
        }

        public IEnumerator<PresetItem> GetEnumerator() => _presets.GetEnumerator();

        /// <summary>
        /// Represent a single preset for a desk
        /// </summary>
        public class PresetItem
        {
            #region Property
            public string Name { get; set; }
            public short Height { get; set; }
            public string Description { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public VirtualKey VirtualKey { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public VirtualKeyModifiers VirtualKeyModifiers { get; set; }
            #endregion

            #region Default constructor
            public PresetItem(string name,
                              short height,
                              string description = "",
                              VirtualKey virtualKey = VirtualKey.None,
                              VirtualKeyModifiers virtualKeyModifiers = VirtualKeyModifiers.None)
            {
                Name = name;
                Height = height;
                Description = description;
                VirtualKey = virtualKey;
                VirtualKeyModifiers = virtualKeyModifiers;
            }
            #endregion
        }
    }
}