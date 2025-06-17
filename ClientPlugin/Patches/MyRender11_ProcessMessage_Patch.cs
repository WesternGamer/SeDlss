using HarmonyLib;
using VRageRender;
using VRageRender.Messages;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyRender11), "ProcessMessage")]
    internal class MyRender11_ProcessMessage_Patch
    {
        public static MyRenderMessageEnum MessageType = MyRenderMessageEnum.DrawCommands;

        private static bool Prefix(MyRenderMessageBase message)
        {
            MessageType = message.MessageType;
            return true;
        }
    }
}
