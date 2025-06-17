using HarmonyLib;
using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Utils;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SLSharp;
using SLSharp.Dlss;
using SLSharp.Dlss.Structs;
using SLSharp.Enums;
using SLSharp.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRage;
using VRage.FileSystem;
using VRage.Library.Utils;
using VRage.Render11.Common;
using VRage.Render11.LightingStage;
using VRage.Render11.LightingStage.EnvironmentProbe;
using VRage.Render11.Render;
using VRage.Render11.RenderContext;
using VRage.Render11.Resources;
using VRage.Render11.Scene;
using VRage.Render11.Scene.Components;
using VRage.Render11.Sprites;
using VRageMath;
using VRageRender;
using VRageRender.ExternalApp;
using VRageRender.Messages;
using VRageRender.Vertex;
using static VRageRender.MyBlur;
using static VRageRender.MyMeshes;
using static VRageRender.MyShadowsSettings;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyRender11), "CreateDeviceInternal")]
    internal class MyRender11_CreateDeviceInternal_Patch
    {
        private static bool Prefix(MyRenderDeviceSettings? settings)
        {
            MyRender11.Log.WriteLine("CreateDeviceInternal - START");

            MyRender11.RenderThread = Thread.CurrentThread;

            MyRender11.ForceWindowed();
            MyRender11.RemoveScreenResources();

            MyRender11.Backbuffer?.Release();
            MyRender11.Backbuffer = null;

            MyRender11.RC?.Dispose();
            MyRender11.RC = null;

            MyVRage.Platform.Render.DisposeRenderDevice();
            
            MyRender11.DebugDevice?.ReportLiveDeviceObjects(ReportingLevel.Summary | ReportingLevel.Detail);
            MyRender11.DebugDevice?.Dispose();

            Preferences preferences = new Preferences()
            {
                showConsole = true,
                pathsToPlugins = new string[] { Path.GetFullPath(Path.Combine(MyFileSystem.ExePath, "SeDlss")) }.ToPtr(),
                numPathsToPlugins = 1,
                flags = SLSharp.Enums.PreferenceFlags.eUseDXGIFactoryProxy | PreferenceFlags.eUseManualHooking,
                featuresToLoad = new uint[] { (uint)SLSharp.Enums.Feature.kFeatureDLSS, (uint)SLSharp.Enums.Feature.kFeatureImGUI }.ToPtr(),
                numFeaturesToLoad = 2,
                engine = SLSharp.Enums.EngineType.eCustom,
                engineVersion = "0.0.0",
                renderAPI = RenderAPI.eD3D11
            };

            ResultCheck.Check(() => SLSharp.Streamline.slInit(ref preferences, Consts.kSDKVersion));

            MyVRage.Platform.Render.CreateRenderDevice(ref settings, out var deviceInstance, out var swapChain);

            ResultCheck.Check(() => Streamline.slSetD3DDevice((deviceInstance as SharpDX.Direct3D11.Device1).NativePointer));

            MyRender11.m_settings = settings.Value;
            MyRender11.DeviceInstance = deviceInstance as SharpDX.Direct3D11.Device1;
            MyRender11.m_swapchain = swapChain as SwapChain;

            if (!MyRender11.m_initialized)
            {
                MyAdapterInfo myAdapterInfo = MyRender11.GetAdaptersList()[MyRender11.m_settings.AdapterOrdinal];
                MyRender11.ParallelVertexBufferMapping = myAdapterInfo.ParallelVertexBufferMapping;
                MyRender11.DeferredTransferData = myAdapterInfo.DeferredTransferData;
                MyRender11.BatchedConstantBufferMapping = myAdapterInfo.BatchedConstantBufferMapping;
                MyRender11.m_resolution = new Vector2I(MyRender11.m_settings.BackBufferWidth, MyRender11.m_settings.BackBufferHeight);
                MyRender11.ViewportResolution = MyRender11.m_resolution;
            }

            MyRender11.RC = new MyRenderContext();
            MyRender11.RC.Initialize(MyRender11.DeviceInstance.ImmediateContext1);
            
            if (!MyRender11.m_initializedOnce)
            {
                MyRender11.InitSubsystemsOnce();
                MyRender11.m_initializedOnce = true;
            }
            if (!MyRender11.m_initialized)
            {
                MyRender11.InitSubsystems(MyRender11.m_settings.InitParallel);
                MyRender11.m_initialized = true;
            }
            else
            {
                ResetSubsystems();
            }

            MyRender11.m_settings.WindowMode = MyWindowModeEnum.Window;
            MyRender11.ApplySettings(MyRender11.m_settings);
            //InitDlss();
            MyRender11.Log.WriteLine("CreateDeviceInteral - END");
            return false;
        }

        //Only safe to call when scene is not rendered!
        private unsafe static void ResetSubsystems()
        {
            MyShaders.OnDeviceReset();

            ResetManagers();

            MyCommon.Init();

            MyLinesRenderer.m_VB = MyManagers.Buffers.CreateVertexBuffer("MyLinesRenderer", MyLinesRenderer.m_currentBufferSize, sizeof(MyVertexFormatPositionColor), null, ResourceUsage.Dynamic, isStreamOutput: false, isGlobal: true);

            MyPrimitivesRenderer.m_vb = MyManagers.Buffers.CreateVertexBuffer("MyPrimitivesRenderer", MyPrimitivesRenderer.m_currentBufferSize, sizeof(MyVertexFormatPositionColor), null, ResourceUsage.Dynamic, isStreamOutput: false, isGlobal: true);

            MyRender11.m_mainSprites = MyManagers.SpritesManager.GetSpritesRenderer();

            MyScreenPass.Init(MyImmediateRC.RC);

            MyRenderContext deferredRC = MyManagers.DeferredRCs.AcquireRC("InitSubsytems_RC");

            MyMeshes.OnDeviceEnd();
            MyMeshes.m_meshUpdateBatch.Dispose();
            MyMeshes.m_meshUpdateBatch = null;
            
            MyRender11.m_meshBatch = MyMeshes.OpenMeshUpdateBatch();
            MyMeshes.OnDeviceReset();

            MyManagers.Shadows.ShadowCascades?.UnloadResources();
            MyManagers.Shadows.ShadowCascades = null;
            MyRender11.ResetShadows(deferredRC, MyShadowCascades.Settings.Data.CascadesCount, MyRender11.Settings.User.ShadowMemoryQuality.ShadowCascadeResolution());

            MyMaterialShaders.OnDeviceReset();

            MyLightsRendering.OnDeviceEnd();
            MyLightsRendering.Init(MyRender11.m_meshBatch);

            MyBlur.m_blurConstantBuffer = MyManagers.Buffers.CreateConstantBuffer("MyBlur", sizeof(BlurConstants), null, ResourceUsage.Dynamic, isGlobal: true);

            MyTransparentRendering.OnDeviceEnd();
            MyTransparentRendering.Init();

            MyBillboardRenderer.m_fileTextures.Clear();
            MyBillboardRenderer.Init();

            MyDebugRenderer.m_inputLayout = MyInputLayouts.Create(MyDebugRenderer.m_screenVertexShader.InfoId, MyVertexLayouts.GetLayout(MyVertexInputComponentType.POSITION2, MyVertexInputComponentType.TEXCOORD0));
            MyDebugRenderer.m_quadBuffer = MyManagers.Buffers.CreateVertexBuffer("MyDebugRenderer quad", 6, MyVertexFormatPosition2Texcoord.STRIDE, null, ResourceUsage.Dynamic, isStreamOutput: false, isGlobal: true);

            MyScreenDecals.OnDeviceReset();

            MyEnvProbeProcessing.m_transformConstants = MyManagers.Buffers.CreateConstantBuffer("TransformConstants", sizeof(Matrix) * 2 + sizeof(Vector4), null, ResourceUsage.Dynamic, isGlobal: true);

            MyAtmosphereRenderer.OnDeviceEnd();
            MyAtmosphereRenderer.Init(MyRender11.m_meshBatch);

            MyLuminanceAverage.m_prevLum = MyManagers.RwTextures.CreateUav("MyLuminanceAverage.PrevLum", 1, 1, Format.R32G32_Float);

            MyEyeAdaptation.Init(deferredRC);

            MyVoxelMaterials.OnDeviceReset();
            MyMeshMaterials1.OnDeviceReset();

            MyHBAO.Init(deferredRC);

            MyRenderableComponent.MarkAllDirty();
            foreach (MyMergeGroupRootComponent item in MyComponentFactory<MyMergeGroupRootComponent>.GetAll())
            {
                item.OnDeviceReset();
            }

            MyBigMeshTable.Table.OnDeviceReset();
            MyInstancing.OnDeviceReset();

            MyRenderProxy.EnqueueMessage(new MyRenderMessageInitSubsystemsConsume());

            MyScene11.Instance.Updater.CallIn(delegate
            {
                MyFinishedContext fc = deferredRC.FinishDeferredContext();
                MyRender11.RC.ExecuteContext(ref fc);
                MyRender11.RC.ClearState();
                MyRender11.InitSubsystemsConsumeReady = true;
            }, MyTimeSpan.Zero);
        }

        private static void ResetManagers()
        {
            MyManagers.OnDeviceReset();
            AccessTools
                   .Field(typeof(MyRenderScheduler), nameof(MyRenderScheduler.m_batch))
                   .SetValue(MyManagers.RenderScheduler, new DependencyBatch(WorkPriority.VeryHigh));
            MyManagers.RenderScheduler.OnDeviceInit();
        }

        private static void InitDlss()
        {
            DlssOptimalSettings settings = new DlssOptimalSettings();
            DlssOptions dlssOptions = new DlssOptions()
            {
                mode = SLSharp.Dlss.Enums.DlssMode.eDLAA,
                outputWidth = (uint)MySandboxGame.Config.ScreenWidth.Value,
                outputHeight = (uint)MySandboxGame.Config.ScreenHeight.Value,
            };

            ResultCheck.Check(() => Dlss.slDLSSGetOptimalSettings(ref dlssOptions, ref settings));

            
            var depth = new SLSharp.Structs.Resource(ResourceType.eTex2d, MyGBuffer.Main.DepthStencil.Resource.NativePointer, IntPtr.Zero, IntPtr.Zero, 0);

            Extent extent = new Extent()
            {
                width = (uint)MyGBuffer.Main.DepthStencil.Size.X,
                height = (uint)MyGBuffer.Main.DepthStencil.Size.Y,
            };

            var depthTag = new ResourceTag(ref depth, BufferType.kBufferTypeDepth, ResourceLifecycle.eValidUntilPresent, extent);
        }
    }
}
