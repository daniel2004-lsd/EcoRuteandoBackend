using System.Reflection;
using NpgsqlTypes;

namespace EcoRuteando.Modules.Mobility.Domain.Enums;

/// <summary>
/// Utilidades para traducir entre enums de C# y los labels
/// que usan los tipos ENUM de PostgreSQL (ej. "public_transport").
/// </summary>
public static class PgEnumExtensions
{
    /// <summary>
    /// Obtiene el label de PostgreSQL declarado con [PgName]
    /// (o el nombre del miembro si no tiene atributo).
    /// </summary>
    public static string ToPgName<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        var field = typeof(TEnum).GetField(value.ToString());

        return field?.GetCustomAttribute<PgNameAttribute>()?.PgName
            ?? value.ToString();
    }

    /// <summary>
    /// Convierte un label de PostgreSQL ("under_review") al enum de C#.
    /// Acepta mayúsculas/minúsculas y espacios en lugar de guiones bajos.
    /// </summary>
    public static bool TryFromPgName<TEnum>(
        string? value,
        out TEnum result)
        where TEnum : struct, Enum
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().Replace(' ', '_');

        foreach (var field in typeof(TEnum).GetFields())
        {
            var pgName = field.GetCustomAttribute<PgNameAttribute>()?.PgName;

            if (pgName is not null
                && string.Equals(pgName, normalized, StringComparison.OrdinalIgnoreCase))
            {
                result = (TEnum)field.GetValue(null)!;
                return true;
            }
        }

        return Enum.TryParse(normalized, ignoreCase: true, out result);
    }
}
