using HarmonyLib;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SLSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Platform.Windows.Render;
using VRageRender;
using VRageRender.ExternalApp;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyPlatformRender), "CreateSwapChain")]
    internal class MyPlatformRender_CreateSwapChain_Patch
    {
        public static SwapChain ProxySwapchain = null;

        private static bool Prefix(IntPtr windowHandle)
        {
            MyPlatformRender.DisposeSwapChain();
            MyPlatformRender.Log.WriteLine("CreateDeviceInternal create swapchain");
            if (MyPlatformRender.m_swapchain == null)
            {
                SwapChainDescription swapChainDescription = new SwapChainDescription
                {
                    BufferCount = 2,
                    Flags = SwapChainFlags.AllowModeSwitch,
                    IsWindowed = true,
                    ModeDescription = MyPlatformRender.GetCurrentModeDescriptor(MyPlatformRender.m_settings),
                    SampleDescription =
                    {
                        Count = 1,
                        Quality = 0
                    },
                    OutputHandle = windowHandle,
                    Usage = (Usage.ShaderInput | Usage.RenderTargetOutput),
                    SwapEffect = SwapEffect.Discard
                };
                Factory factory = MyPlatformRender.GetFactory();

                IntPtr ptr = factory.NativePointer;

                Streamline.slUpgradeInterface(ref ptr);

                try
                {
                    ProxySwapchain = new SwapChain(factory, MyPlatformRender.DeviceInstance, swapChainDescription);

                    IntPtr nativeSwapchainPtr = IntPtr.Zero;

                    Streamline.slGetNativeInterface(ProxySwapchain.NativePointer, ref nativeSwapchainPtr);

                    MyPlatformRender.m_swapchain = new SwapChain(nativeSwapchainPtr);
                }
                catch (Exception ex)
                {
                    MyPlatformRender.Log.WriteLine("SwapChain factory = " + factory);
                    MyPlatformRender.Log.WriteLine("SwapChain Device = " + MyPlatformRender.DeviceInstance);
                    MyPlatformRender.PrintSwapChainDescriptionToLog(swapChainDescription);
                    throw ex;
                }
                factory.MakeWindowAssociation(windowHandle, WindowAssociationFlags.IgnoreAll);
            }

            return false;
        }
    }
}
