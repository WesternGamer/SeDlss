using HarmonyLib;
using SharpDX;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Platform.Windows.Render;
using VRage.Utils;
using VRageRender;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyPlatformRender), "ApplySettings")]
    internal class MyPlatformRender_TryChangeToFullscreen_Patch
    {
        private static bool Prefix()
        {
            if (!MyPlatformRender.m_changeToFullscreen.HasValue)
            {
                return false;
            }
            ModeDescription newTargetParametersRef = MyPlatformRender.m_changeToFullscreen.Value;
            try
            {
                int adapterDeviceId = MyPlatformRender.m_adapterInfoList[MyPlatformRender.m_settings.AdapterOrdinal].AdapterDeviceId;
                int outputId = MyPlatformRender.m_adapterInfoList[MyPlatformRender.m_settings.AdapterOrdinal].OutputId;
                MyPlatformRender.m_swapchain.ResizeTarget(ref newTargetParametersRef);
                MyPlatformRender_CreateSwapChain_Patch.ProxySwapchain.SetFullscreenState(true, (MyPlatformRender.GetFactory().Adapters[adapterDeviceId].Outputs.Length > outputId) ? MyPlatformRender.GetFactory().Adapters[adapterDeviceId].Outputs[outputId] : null);
                newTargetParametersRef.RefreshRate.Numerator = 0;
                newTargetParametersRef.RefreshRate.Denominator = 0;
                MyPlatformRender.m_swapchain.ResizeTarget(ref newTargetParametersRef);
                MyPlatformRender.m_changeToFullscreen = null;
                MyPlatformRender.Log.WriteLine("DXGI SetFullscreenState succeded");
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode == ResultCode.Unsupported)
                {
                    MyPlatformRender.m_changeToFullscreen = null;
                }
                else if (ex.ResultCode == Result.OutOfMemory)
                {
                    MyPlatformRender.Log.Error(ex.ToString());
                    throw;
                }
                MyPlatformRender.Log.WriteLine("TryChangeToFullscreen failed with " + ex.ResultCode);
            }

            return false;
        }
    }
}
