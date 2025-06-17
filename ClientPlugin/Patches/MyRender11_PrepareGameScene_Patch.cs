using HarmonyLib;
using Sandbox;
using Sandbox.Game.World;
using SLSharp;
using SLSharp.Dlss;
using SLSharp.Dlss.Enums;
using SLSharp.Dlss.Structs;
using SLSharp.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Utils;
using VRageMath;
using VRageRender;

namespace ClientPlugin.Patches
{
    [HarmonyPatch(typeof(MyRender11), "PrepareGameScene")]
    internal static class MyRender11_PrepareGameScene_Patch
    {
        public static ViewportHandle ViewportHandle = new(5382);

        public static IntPtr FrameToken = IntPtr.Zero; 

        private static void Prefix()
        {
            // TODO: Check DLSS available

            // TODO: Reset DLSS if disabled

            var frameCount = (uint)MyRender11.m_messageFrameCounter;

            Streamline.slGetNewFrameToken(ref FrameToken, ref frameCount);

            DlssOptions dlssOptions = new DlssOptions()
            {
                mode = SLSharp.Dlss.Enums.DlssMode.eDLAA,
                outputWidth = (uint)MyRender11.ViewportResolution.X,
                outputHeight = (uint)MyRender11.ViewportResolution.Y,
                colorBuffersHDR = SLSharp.Enums.Boolean.eFalse,
                sharpness = 0.5f,
                useAutoExposure = SLSharp.Enums.Boolean.eFalse,
            };

            Dlss.slDLSSSetOptions(ref ViewportHandle, ref dlssOptions);

            Constants constants = new Constants()
            {
                cameraAspectRatio = MyRender11.ViewportResolution.X / MyRender11.ViewportResolution.Y,
                cameraFOV = MySector.MainCamera != null ? MySector.MainCamera.FieldOfViewDegrees : 0.0f,
                cameraFar = MySector.MainCamera != null ? MySector.MainCamera.FarPlaneDistance : MyCamera.DefaultFarPlaneDistance,
                cameraMotionIncluded = SLSharp.Enums.Boolean.eTrue,
                cameraNear = MySector.MainCamera != null ? MySector.MainCamera.GetSafeNear() : 0.05f,
                cameraPinholeOffset = new Float2(0.0f, 0.0f),
                cameraPos = MySector.MainCamera != null ? new Float3((float)MySector.MainCamera.Position.X, (float)MySector.MainCamera.Position.Y, (float)MySector.MainCamera.Position.Z) : new Float3(0.0f, 0.0f, 0.0f),
                cameraFwd = MySector.MainCamera != null ? new Float3(MySector.MainCamera.ForwardVector.X, MySector.MainCamera.ForwardVector.Y, MySector.MainCamera.ForwardVector.Z) : new Float3(0.0f, 0.0f, 0.0f),
                cameraUp = MySector.MainCamera != null ? new Float3(MySector.MainCamera.UpVector.X, MySector.MainCamera.UpVector.Y, MySector.MainCamera.UpVector.Z) : new Float3(0.0f, 0.0f, 0.0f),
                cameraRight = MySector.MainCamera != null ? new Float3((float)MySector.MainCamera.WorldMatrix.Right.X, (float)MySector.MainCamera.WorldMatrix.Right.Y, (float)MySector.MainCamera.WorldMatrix.Right.Z) : new Float3(0.0f, 0.0f, 0.0f),
                cameraViewToClip = MySector.MainCamera != null ? MySector.MainCamera.ProjectionMatrix.ToSlMatrix() : MatrixD.Identity.ToSlMatrix(),
                clipToCameraView = MySector.MainCamera != null ? MySector.MainCamera.ProjectionMatrix.Invert().ToSlMatrix() : MatrixD.Identity.Invert().ToSlMatrix(),
                depthInverted = SLSharp.Enums.Boolean.eFalse,
                jitterOffset = new Float2(0, 0),
                mvecScale = new Float2(1, 1),
                prevClipToClip = MatrixD.Identity.ToSlMatrix(),
                reset = SLSharp.Enums.Boolean.eFalse,
                motionVectors3D = SLSharp.Enums.Boolean.eFalse,
                motionVectorsInvalidValue = Consts.INVALID_FLOAT
            };

            // TODO/FIX: Some of the constants required are being provided fake/wrong values

            Streamline.slSetConstants(ref constants, FrameToken, ref ViewportHandle);
        }

        public static Float4x4 ToSlMatrix(this MatrixD matrix)
        {
            Float4x4 result = new Float4x4();
            result.SetRow(0, new Float4((float)matrix.M11, (float)matrix.M12, (float)matrix.M13, (float)matrix.M14));
            result.SetRow(1, new Float4((float)matrix.M21, (float)matrix.M22, (float)matrix.M23, (float)matrix.M24));
            result.SetRow(2, new Float4((float)matrix.M31, (float)matrix.M32, (float)matrix.M33, (float)matrix.M34));
            result.SetRow(3, new Float4((float)matrix.M41, (float)matrix.M42, (float)matrix.M43, (float)matrix.M44));

            return result;
        }

        public static MatrixD Invert(this MatrixD matrix)
        {
            MatrixD result = MatrixD.Identity;
            double m = matrix.M11;
            double m2 = matrix.M12;
            double m3 = matrix.M13;
            double m4 = matrix.M14;
            double m5 = matrix.M21;
            double m6 = matrix.M22;
            double m7 = matrix.M23;
            double m8 = matrix.M24;
            double m9 = matrix.M31;
            double m10 = matrix.M32;
            double m11 = matrix.M33;
            double m12 = matrix.M34;
            double m13 = matrix.M41;
            double m14 = matrix.M42;
            double m15 = matrix.M43;
            double m16 = matrix.M44;
            double num = m11 * m16 - m12 * m15;
            double num2 = m10 * m16 - m12 * m14;
            double num3 = m10 * m15 - m11 * m14;
            double num4 = m9 * m16 - m12 * m13;
            double num5 = m9 * m15 - m11 * m13;
            double num6 = m9 * m14 - m10 * m13;
            double num7 = m6 * num - m7 * num2 + m8 * num3;
            double num8 = 0.0 - (m5 * num - m7 * num4 + m8 * num5);
            double num9 = m5 * num2 - m6 * num4 + m8 * num6;
            double num10 = 0.0 - (m5 * num3 - m6 * num5 + m7 * num6);
            double num11 = 1.0 / (m * num7 + m2 * num8 + m3 * num9 + m4 * num10);
            result.M11 = num7 * num11;
            result.M21 = num8 * num11;
            result.M31 = num9 * num11;
            result.M41 = num10 * num11;
            result.M12 = (0.0 - (m2 * num - m3 * num2 + m4 * num3)) * num11;
            result.M22 = (m * num - m3 * num4 + m4 * num5) * num11;
            result.M32 = (0.0 - (m * num2 - m2 * num4 + m4 * num6)) * num11;
            result.M42 = (m * num3 - m2 * num5 + m3 * num6) * num11;
            double num12 = m7 * m16 - m8 * m15;
            double num13 = m6 * m16 - m8 * m14;
            double num14 = m6 * m15 - m7 * m14;
            double num15 = m5 * m16 - m8 * m13;
            double num16 = m5 * m15 - m7 * m13;
            double num17 = m5 * m14 - m6 * m13;
            result.M13 = (m2 * num12 - m3 * num13 + m4 * num14) * num11;
            result.M23 = (0.0 - (m * num12 - m3 * num15 + m4 * num16)) * num11;
            result.M33 = (m * num13 - m2 * num15 + m4 * num17) * num11;
            result.M43 = (0.0 - (m * num14 - m2 * num16 + m3 * num17)) * num11;
            double num18 = m7 * m12 - m8 * m11;
            double num19 = m6 * m12 - m8 * m10;
            double num20 = m6 * m11 - m7 * m10;
            double num21 = m5 * m12 - m8 * m9;
            double num22 = m5 * m11 - m7 * m9;
            double num23 = m5 * m10 - m6 * m9;
            result.M14 = (0.0 - (m2 * num18 - m3 * num19 + m4 * num20)) * num11;
            result.M24 = (m * num18 - m3 * num21 + m4 * num22) * num11;
            result.M34 = (0.0 - (m * num19 - m2 * num21 + m4 * num23)) * num11;
            result.M44 = (m * num20 - m2 * num22 + m3 * num23) * num11;
            return result;
        }
    }
}
