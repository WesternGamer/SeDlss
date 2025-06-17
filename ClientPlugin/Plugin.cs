using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using ClientPlugin.Settings;
using ClientPlugin.Settings.Layouts;
using HarmonyLib;
using Sandbox;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.Platform.Windows;
using VRage.Platform.Windows.Forms;
using VRage.Plugins;
using VRageRender;
using SpaceEngineers;
using System.Threading;
using VRage.Render11.Common;
using VRage.Render11.Profiler;
using Sandbox.Graphics;
using ClientPlugin.Patches;
using SpaceEngineers.Game.GUI;
using VRage.Input;

namespace ClientPlugin
{
    // ReSharper disable once UnusedType.Global
    internal class Plugin : IPlugin, IHandleInputPlugin,  IDisposable
    {
        internal const string Name = "SeDlss";
        internal static Plugin Instance { get; private set; }
        private SettingsGenerator settingsGenerator;

        internal static bool Reloading = false;

        public Plugin()
        {
            Instance = this;
            Instance.settingsGenerator = new SettingsGenerator();

            // TODO: Put your one time initialization code here.
            Harmony harmony = new Harmony(Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            MyRenderThread_RenderCallback_Patch.ReloadRequested = true;
        }
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            
        }

        public void Dispose()
        {
            // TODO: Save state and close resources here, called when the game exits (not guaranteed!)
            // IMPORTANT: Do NOT call harmony.UnpatchAll() here! It may break other plugins.

            Instance = null;
        }

        public void Update()
        {
            
            // TODO: Put your update code here. It is called on every simulation frame!

            //if (Failed)
            //{
            //    ReloadGraphics();
            //    Failed = false;
            //}
        }

        // ReSharper disable once UnusedMember.Global
        public void OpenConfigDialog()
        {
            Instance.settingsGenerator.SetLayout<Simple>();
            MyGuiSandbox.AddScreen(Instance.settingsGenerator.Dialog);
        }

        public void HandleInput()
        {
            if ((bool)(MyInput.Static?.WasKeyPress(MyKeys.LeftAlt)) && (bool)(MyInput.Static?.WasKeyPress(MyKeys.F12)))
            {
                MyRenderThread_RenderCallback_Patch.ReloadRequested = true;
            }
        }

        //TODO: Uncomment and use this method to load asset files
        /*public void LoadAssets(string folder)
        {

        }*/
    }
}