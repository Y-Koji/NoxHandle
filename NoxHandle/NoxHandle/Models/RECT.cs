using System.Runtime.InteropServices;

namespace NoxHandle.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        internal int left;
        internal int top;
        internal int right;
        internal int bottom;
    }
}
