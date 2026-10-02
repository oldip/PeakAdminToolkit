using System;
using System.Reflection;

namespace PeakAdminToolkit.Items
{
    internal static class ItemApiAccess
    {
        internal const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        internal const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        internal static bool IsValidToSpawn(object item)
        {
            bool valid;
            string reason;
            return TryIsValidToSpawn(item, out valid, out reason) && valid;
        }

        internal static bool TryIsValidToSpawn(object item, out bool valid, out string reason)
        {
            valid = false;
            reason = null;
            if (item == null) { reason = "item unavailable"; return false; }
            try
            {
                MethodInfo method = item.GetType().GetMethod("IsValidToSpawn", InstanceFlags, null, Type.EmptyTypes, null);
                if (method == null) { reason = "IsValidToSpawn missing"; return false; }
                if (method.ReturnType != typeof(bool)) { reason = "IsValidToSpawn must return Boolean"; return false; }
                valid = (bool)method.Invoke(item, null);
                return true;
            }
            catch (Exception ex)
            {
                var invocation = ex as TargetInvocationException;
                reason = (invocation != null && invocation.InnerException != null ? invocation.InnerException : ex).GetType().Name;
                return false;
            }
        }

        internal static string InvokeStringMethod(object target, string name)
        {
            MethodInfo method = target.GetType().GetMethod(name, InstanceFlags, null, Type.EmptyTypes, null);
            if (method == null || method.ReturnType != typeof(string)) return string.Empty;
            try { return method.Invoke(target, null) as string ?? string.Empty; }
            catch (Exception) { return string.Empty; }
        }

        internal static bool ReadStaticBoolean(Type type, string name)
        {
            object value = ReadStaticMember(type, name);
            return value is bool && (bool)value;
        }

        internal static object ReadMember(object target, string name)
        {
            if (target == null) return null;
            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(name, InstanceFlags);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                try { return property.GetValue(target, null); }
                catch (Exception) { return null; }
            }
            FieldInfo field = type.GetField(name, InstanceFlags);
            return field == null ? null : field.GetValue(target);
        }

        internal static object ReadStaticMember(Type type, string name)
        {
            if (type == null) return null;
            PropertyInfo property = type.GetProperty(name, StaticFlags);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                try { return property.GetValue(null, null); }
                catch (Exception) { return null; }
            }
            FieldInfo field = type.GetField(name, StaticFlags);
            return field == null ? null : field.GetValue(null);
        }

        internal static Type FindGameType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name != "Assembly-CSharp") continue;
                return assembly.GetType(name, false);
            }
            return null;
        }

        internal static Type FindLoadedType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name, false);
                if (type != null) return type;
            }
            return null;
        }

        internal static bool HasStaticBooleanMember(Type type, string name)
        {
            if (type == null) return false;
            PropertyInfo property = type.GetProperty(name, StaticFlags);
            if (property != null && property.PropertyType == typeof(bool) && property.GetIndexParameters().Length == 0)
            {
                MethodInfo getter = property.GetGetMethod(true);
                if (getter != null && getter.IsStatic) return true;
            }
            FieldInfo field = type.GetField(name, StaticFlags);
            return field != null && field.FieldType == typeof(bool) && field.IsStatic;
        }
    }
}
