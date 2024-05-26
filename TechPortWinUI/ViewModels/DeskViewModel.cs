using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechPortWinUI.Models;

namespace TechPortWinUI.ViewModels
{
    public class DeskViewModel
    {
        public List<DeskItem> DeskItems;
        public void AddDesk(string id) { }
        public void RemoveDesk() { }
    }

    public partial class DeskItem
    {
        #region default constructor
        public DeskItem()
        {
            
        }
        #endregion

        #region Public members
        public const double _MIN_HEIGHT = 0.62;
        public const double _MAX_HEIGHT = 1.27;

        public bool IsConnected;

        public string ID;   
        public string Name;
        public short Height;
        public short Speed;
        public List<PresetItem> Presets { get; } = new()
        {
            new("Preset1", 78),
            new("Preset2", 112),
            new("Preset3", 100),
            new("Preset4", 100),
            new("Preset5", 100),
        };
        #endregion

        #region Public methods
        public BitmapImage Icon;

        public void ConnectToDesk() { }
        public void DisconectToDesk() { }

        public void MoveUp() { }
        public void MoveDown() { }
        public void MoveToHeight() { }

        //TODO: Verify input for these methods
        [RelayCommand]
        public void AddPreset(PresetItem preset) => Presets.Add(preset);
        [RelayCommand]
        public void DeletPreset(PresetItem preset) => Presets.Remove(preset);
        #endregion

        /// <summary>
        /// Represents a single preset configuration for a desk, including its height and a name.
        /// </summary>
        public class PresetItem
        {
            #region public members
            /// <summary>
            /// Gets or sets the name of the preset.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Gets or sets the height of the desk in the preset.
            /// </summary>
            public short Height { get; set; }
            #endregion

            #region Default constructor
            /// <summary>
            /// Initializes a new instance of the <see cref="PresetsModel"/> class with the specified height and name.
            /// </summary>
            /// <param name="height">The height of the desk in the preset.</param>
            /// <param name="name">The name of the preset.</param>
            public PresetItem(string name, short height)
            {
                Name = name;
                Height = height;
            }
            #endregion

            #region Public methods
            //TODO: Verify input for these methods
            private void ModifyPrestName(string newName) => this.Name = newName;
            private void ModifyPrestHeight(short newHeight) => this.Height = newHeight;
            #endregion
        }
    }
}
