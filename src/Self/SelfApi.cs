using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PeakAdminToolkit.Self
{
    // No game types in signatures: one missing capability need not prevent the UI loading.
    internal sealed class SelfApi
    {
        internal const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly Func<string, Type> resolve;
        private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();
        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> members = new Dictionary<Type, Dictionary<string, MemberInfo>>();
        private FieldInfo localCharacter;
        internal SelfApi(Func<string, Type> resolve = null) { this.resolve = resolve ?? FindGameType; }
        internal Type Type(string name)
        {
            Type type;
            if (types.TryGetValue(name, out type)) return type;
            type = resolve(name);
            if (type != null) types.Add(name, type);
            return type;
        }

        internal object Local()
        {
            Type type = Type("Character");
            if (type == null) return null;
            if (localCharacter == null) localCharacter = type.GetField("localCharacter", BindingFlags.Public | BindingFlags.Static);
            if (localCharacter == null) return null;
            object character = localCharacter.GetValue(null);
            if (!Alive(character)) return null;
            object data = Read(character, "data");
            object view = Read(Read(character, "refs"), "view");
            return Alive(view) && (bool)Read(view, "IsMine") && (bool)Read(data, "fullyConscious") ? character : null;
        }

        internal static bool Alive(object value)
        {
            if (value == null) return false;
            UnityEngine.Object unity = value as UnityEngine.Object;
            return ReferenceEquals(unity, null) || unity;
        }

        internal static object Read(object target, string name)
        {
            if (!Alive(target)) throw new InvalidOperationException("Unavailable object: " + name);
            Type type = target.GetType();
            Dictionary<string, MemberInfo> byName;
            if (!members.TryGetValue(type, out byName))
            {
                byName = new Dictionary<string, MemberInfo>();
                members.Add(type, byName);
            }
            MemberInfo member;
            if (!byName.TryGetValue(name, out member))
            {
                member = type.GetField(name, Instance);
                if (member == null)
                {
                    PropertyInfo property = type.GetProperty(name, Instance);
                    if (property == null || !property.CanRead) throw new MissingMemberException(type.Name, name);
                    member = property;
                }
                byName.Add(name, member);
            }
            FieldInfo field = member as FieldInfo;
            return field != null ? field.GetValue(target) : ((PropertyInfo)member).GetValue(target, null);
        }

        internal static MethodInfo Method(Type type, string name, Type result, params Type[] arguments)
        {
            if (type == null) throw new MissingMemberException("Missing type for " + name);
            MethodInfo method = type.GetMethod(name, Instance, null, arguments, null);
            if (method == null || method.ReturnType != result) throw new MissingMethodException(type.Name, name);
            return method;
        }

        private static Type FindGameType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetName().Name == "Assembly-CSharp") return assembly.GetType(name, false);
            return null;
        }
    }
}
