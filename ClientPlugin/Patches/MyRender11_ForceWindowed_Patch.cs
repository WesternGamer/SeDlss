using HarmonyLib;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Platform.Windows.Render;
using VRageRender;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyRender11), "ForceWindowed")]
    internal class MyRender11_ForceWindowed_Patch
    {
        private static bool Prefix()
        {
            if (MyRender11.m_settings.WindowMode == MyWindowModeEnum.Fullscreen && MyRender11.m_swapchain != null)
            {
                try
                {
                    MyPlatformRender_CreateSwapChain_Patch.ProxySwapchain.SetFullscreenState(false, null);
                }
                catch (Exception)
                {
                }
            }

            return false;
        }
    }
}
