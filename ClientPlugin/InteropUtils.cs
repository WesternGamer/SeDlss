using SLSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientPlugin
{
    internal static class InteropUtils
    {
        public static IntPtr ToPtr(this string[] strings)
        {
            IntPtr[] ptrs = new IntPtr[strings.Length];

            for (int i = 0; i < strings.Length; i++)
            {
                ptrs[i] = Marshal.StringToCoTaskMemUni(strings[i]);
            }

            GCHandle gch = GCHandle.Alloc(ptrs, GCHandleType.Pinned);

            return gch.AddrOfPinnedObject();
        }

        public static IntPtr ToPtr(this uint[] features)
        {
            GCHandle gch = GCHandle.Alloc(features, GCHandleType.Pinned);

            return gch.AddrOfPinnedObject();
        }
    }
}
