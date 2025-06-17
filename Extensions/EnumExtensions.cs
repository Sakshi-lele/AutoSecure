// Extensions/EnumExtensions.cs
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Auto_Insurance_Management_System.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            return value.GetType()
                .GetMember(value.ToString())
                .First()
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description ?? value.ToString();
        }
    }
}