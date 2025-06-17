using HarmonyLib;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Render11.Resources;
using VRageMath;
using VRageRender;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyRender11), "ResizeSwapchain")]
    internal class MyRender11_ResizeSwapchain_Patch
    {
        private static bool Prefix(int width, int height)
        {
            MyRender11.RC.ClearState();
            MyRender11.RemoveScreenResources();
            if (MyRender11.Backbuffer != null)
            {
                MyRender11.Backbuffer.Release();
                MyPlatformRender_CreateSwapChain_Patch.ProxySwapchain.ResizeBuffers(MyRender11.m_swapchain.Description.BufferCount, width, height, MyRender11.m_swapchain.Description.ModeDescription.Format, SwapChainFlags.AllowModeSwitch);
            }
            MyRender11.Backbuffer = new MyBackbuffer(MyPlatformRender_CreateSwapChain_Patch.ProxySwapchain.GetBackBuffer<Texture2D>(0));
            MyRender11.m_resolution = new Vector2I(width, height);
            MyRender11.CreateScreenResources();

            return false;
        }
    }
}
