using System;
using UnityEngine;

public class EnumNamedArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public EnumNamedArrayAttribute(System.Type type) { this.names = System.Enum.GetNames(type); }
}
