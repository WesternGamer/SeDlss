using HarmonyLib;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Render11.Common;
using VRage.Render11.Profiler;
using VRageRender;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyRender11), "Present")]
    internal class Render11_Present_Patch
    {
        private static bool Prefix()
        {
            if (MyRender11.m_swapchain == null)
            {
                return false;
            }

            if (MyRender11.Settings.User.DRScaling && MyRender11.DebugOverrides.EnableDRS)
            {
                MyPlatformRender_CreateSwapChain_Patch.ProxySwapchain.Present(MyRender11.GetDeviceVSyncMode(), PresentFlags.None);
            }
            else
            {
                MyPlatformRender_CreateSwapChain_Patch.ProxySwapchain.Present(MyRender11.m_settings.VSync, PresentFlags.None);
            }
            if (!MyRender11.m_deferredStateChanges)
            {
                MyManagers.OnUpdate();
            }
            MyGpuProfiler.EndFrame();
            MyGpuProfiler.StartFrame();
            MyVRage.Platform.Render.ApplyRenderSettings(null);
            MyRender11.RC.CurrentCpuNOPGap = null;

            return false;
        }
    }
}
