using System;
namespace Terminal.Domain.Enums
{
    /// <summary>
    /// Represents the available display options for some text.
    /// </summary>
    [Flags]
    public enum DisplayMode
    {
        None =      0,
        Dim =       1 << 0,
        Inverted =  1 << 1,
        Parse =     1 << 2,
        Italics =   1 << 3,
        Bold =      1 << 4,
        DontType =  1 << 5,
        Mute =      1 << 6,
        DontWrap =  1 << 7
    }
}
