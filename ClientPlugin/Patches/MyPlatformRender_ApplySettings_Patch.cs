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
    [HarmonyPatch(typeof(MyPlatformRender), "ApplySettings")]
    internal class MyPlatformRender_ApplySettings_Patch
    {
        private static bool Prefix(MyRenderDeviceSettings? settings)
        {
            if (settings.HasValue)
            {
                ModeDescription newTargetParametersRef = MyPlatformRender.GetCurrentModeDescriptor(settings.Value);
                if (settings.Value.WindowMode == MyWindowModeEnum.Fullscreen)
                {
                    if (settings.Value.WindowMode != MyPlatformRender.m_settings.WindowMode)
                    {
                        MyPlatformRender.m_changeToFullscreen = newTargetParametersRef;
                    }
                    else
                    {
                        MyPlatformRender.m_swapchain.ResizeTarget(ref newTargetParametersRef);
                        newTargetParametersRef.RefreshRate.Denominator = 0;
                        newTargetParametersRef.RefreshRate.Numerator = 0;
                        MyPlatformRender.m_swapchain.ResizeTarget(ref newTargetParametersRef);
                    }
                }
                else if (settings.Value.WindowMode != MyPlatformRender.m_settings.WindowMode && MyPlatformRender.m_settings.WindowMode == MyWindowModeEnum.Fullscreen)
                {
                    MyPlatformRender.m_swapchain.ResizeTarget(ref newTargetParametersRef);
                    MyPlatformRender_CreateSwapChain_Patch.ProxySwapchain.SetFullscreenState(false, null);
                }
                else if (settings.Value.WindowMode == MyWindowModeEnum.FullscreenWindow)
                {
                    MyPlatformRender.m_swapchain.ResizeTarget(ref newTargetParametersRef);
                    MyPlatformRender_CreateSwapChain_Patch.ProxySwapchain.SetFullscreenState(false, null);
                }
                MyPlatformRender.m_settings = settings.Value;
            }
            else
            {
                MyPlatformRender.TryChangeToFullscreen();
            }

            return false;
        }
    }
}
