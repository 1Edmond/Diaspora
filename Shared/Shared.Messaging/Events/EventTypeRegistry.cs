using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events;

public static class EventTypeRegistry
{
    private static readonly Dictionary<string, Type> _types = new();

    public static void Register<T>() =>
        _types[typeof(T).AssemblyQualifiedName!] = typeof(T);

    public static Type? Resolve(string typeName) =>
        _types.TryGetValue(typeName, out var t) ? t : null;
}