using System;
using System.Reflection;

namespace PeakAdminToolkit.Players
{
    internal static class NativeReviveStatusRequest
    {
        internal static bool TryPrepare(object character, out float[] deltas)
        {
            deltas = null;
            try
            {
                object afflictions = PlayerDirectory.ReadMember(PlayerDirectory.ReadMember(character, "refs"), "afflictions");
                Type type = afflictions.GetType();
                MethodInfo read = type.GetMethod("GetCurrentStatus", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo receiver = type.GetMethod("RPC_ApplyStatusesFromFloatArray", BindingFlags.Public | BindingFlags.Instance);
                if (read == null || read.ReturnType != typeof(float) || read.GetParameters().Length != 1 ||
                    receiver == null || receiver.ReturnType != typeof(void) || receiver.GetParameters().Length != 2 ||
                    receiver.GetParameters()[0].ParameterType != typeof(float[])) return false;
                Type status = read.GetParameters()[0].ParameterType;
                if (!status.IsEnum) return false;
                MethodInfo curable = type.GetMethod("StatusIsCurable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { status, typeof(bool), typeof(bool) }, null);
                if (curable == null || curable.ReturnType != typeof(bool)) return false;
                Array values = Enum.GetValues(status);
                int length = 0;
                foreach (object value in values) length = Math.Max(length, Convert.ToInt32(value) + 1);
                float[] result = new float[length];
                foreach (object value in values)
                {
                    if (!(bool)curable.Invoke(afflictions, new object[] { value, true, true })) continue;
                    float amount = (float)read.Invoke(afflictions, new[] { value });
                    if (float.IsNaN(amount) || float.IsInfinity(amount)) return false;
                    result[Convert.ToInt32(value)] = -Math.Max(1f, amount);
                }
                deltas = result;
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
