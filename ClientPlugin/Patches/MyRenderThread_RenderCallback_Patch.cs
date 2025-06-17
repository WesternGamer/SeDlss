using HarmonyLib;
using SharpDX;
using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using VRage.Render11.Common;
using VRage.Render11.Profiler;
using VRage.Utils;
using VRageRender;
using VRageRender.ExternalApp;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyRenderThread), "RenderCallback")]
    internal class MyRenderThread_RenderCallback_Patch
    {
        internal static bool ReloadRequested = false;

        private static bool Prefix(MyRenderThread __instance, bool async)
        {
            if (!RenderFrame(__instance, async) || ReloadRequested)
            {
                MyGpuProfiler.EndFrame();
                typeof(MyGpuProfiler).TypeInitializer.Invoke(null, null);
                MyGpuProfiler.GatherFinishedFrames();
                MyGpuProfiler.m_currentFrame = null;
                MyQueryFactory.m_disjointQueries.Clean();
                MyQueryFactory.m_timestampQueries.Clean();
                MyQueryFactory.m_eventQueries.Clean();

                if (!TryCreateDevice(__instance, __instance.CurrentSettings))
                {
                    throw new InvalidOperationException("Unable to create device.");
                }

                ReloadRequested = false;
            }
          
            return false;
        }

        private static bool TryCreateDevice(MyRenderThread instance, MyRenderDeviceSettings settings)
        {
            instance.m_settings = MyRenderProxy.CreateDevice(instance, settings, out instance.m_adapterList);
            if (instance.m_settings.AdapterOrdinal == -1)
            {
                return false;
            }
            MyRenderProxy.SendCreatedDeviceSettings(instance.m_settings);
            return true;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static bool RenderFrame(MyRenderThread instance, bool async)
        {
            try
            {
                instance.RenderFrame(async);
            }
            catch (SharpDXException ex)
            {
                MyLog.Default.WriteLine($"Exception on render thread.\n" +
                    $"=====================================================================\n" +
                    $"{ex}\n" +
                    $"=====================================================================\n" +
                    $"Previous message: {MyRender11_ProcessMessage_Patch.MessageType}\n" +
                    $"Attempting to recover.");

                return false;
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Exception on render thread.\n" +
                    $"=====================================================================\n" +
                    $"{ex}\n" +
                    $"=====================================================================\n" +
                    $"Previous message: {MyRender11_ProcessMessage_Patch.MessageType}\n" +
                    $"Exiting.");

                throw;
            }

            return true;
        }
    }
}
