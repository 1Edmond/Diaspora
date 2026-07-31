using System;

namespace Shared.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class TranslatableAttribute : Attribute
{
}
