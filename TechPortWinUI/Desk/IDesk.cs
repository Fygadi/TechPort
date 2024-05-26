using TechPortWinUI.Desk;

namespace TechPort.Desk;
    /// <summary>
    /// Represent a Desk
    /// </summary>
    internal interface IDesk
    {
        // Properties
        string Id { get; set; }
        string Name { get; set; }
        short MinHeight { get; set; }
        short MaxHeight { get; set; }
        short Height { get; set; }
        short Speed { get; set; }
        MovementStatus CurrentMovementStatus { get; set; }
        List<Preset> PresetList { get; set; }


        // Methods
        void MoveUpAsync();
        void MoveDownAsync();
        void MoveToHeightAsync();
    }