using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoxHandle.Models.Extensions
{
    public static class RectExtension
    {
        public static int GetWidth(this RECT self) => self.right - self.left;
        public static int GetHeight(this RECT self) => self.bottom - self.top;
    }
}
