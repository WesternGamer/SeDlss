using HarmonyLib;
using Sandbox.Game.World;
using SLSharp;
using SLSharp.Enums;
using SLSharp.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Utils;
using VRage.Render11.Resources;
using VRageMath;
using VRageRender;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyToneMapping), "Run")]
    internal static class MyToneMapping_Run_Patch
    {
        private static void Prefix()
        {
            var depth = new SLSharp.Structs.Resource(ResourceType.eTex2d, MyGBuffer.Main.DepthStencil.Resource.NativePointer, IntPtr.Zero, IntPtr.Zero, 0);

            Extent extent = new Extent()
            {
                width = (uint)MyGBuffer.Main.DepthStencil.Size.X,
                height = (uint)MyGBuffer.Main.DepthStencil.Size.Y,
            };

            var depthTag = new ResourceTag(ref depth, BufferType.kBufferTypeDepth, ResourceLifecycle.eValidUntilPresent, extent);

            // TODO: Tag other resources required for DLSS https://github.com/NVIDIA-RTX/Streamline/blob/main/docs/ProgrammingGuideDLSS.md#40-tag-all-required-resources
        }

        
    }
}
