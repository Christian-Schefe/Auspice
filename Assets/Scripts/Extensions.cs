using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T[] GetEnumValues<T>()
    {
        return (T[])System.Enum.GetValues(typeof(T));
    }
}
