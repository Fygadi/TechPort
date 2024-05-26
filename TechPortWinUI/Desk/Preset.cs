using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace TechPortWinUI.Desk;

public class Preset
{
    public string? Name { get; set;}
    public string? Description { get; set;}
    public short Height { get; set;}
    public VirtualKeyModifiers VirtualKeyModifiers { get; set;}
    public VirtualKey VirtualKey;
}