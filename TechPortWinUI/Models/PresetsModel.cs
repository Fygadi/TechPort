using CommunityToolkit.Mvvm.Input;

namespace TechPortWinUI.Models
{
    public partial class PresetsModel
    {
        #region Default Constructor
        public PresetsModel() { }
        #endregion

        #region Private members
        public List<PresetItem> Presets { get;} = new()
        {
            new("Preset1", 78),
            new("Preset2", 112),
            new("Preset3", 100),
            new("Preset4", 100),
            new("Preset5", 100),
        };
        #endregion

        #region Public methods
        //TODO: Verify input for these methods
        [RelayCommand]
        public void AddPreset(PresetItem preset) => Presets.Add(preset);
        [RelayCommand]
        public void DeletPreset(PresetItem preset) => Presets.Remove(preset);
        #endregion
    }

    /// <summary>
    /// Represents a single preset configuration for a desk, including its height and a name.
    /// </summary>
    public partial class PresetItem
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
        private static void ModifyPrestName(PresetItem preset, string newName) => preset.Name = newName;
        private static void ModifyPrestHeight(PresetItem preset, short newHeight) => preset.Height = newHeight;
        #endregion
    }
}