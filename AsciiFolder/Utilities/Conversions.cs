namespace AsciiFolder.Utilities;

public static partial class Conversions
{
    private static readonly char[] _enumSeparators = [',', ';', '+', '|', ' '];
    private static readonly Lazy<MethodInfo> _tryChangeType = new(() => typeof(Conversions).GetMethods().First(m => m.Name == nameof(TryChangeType) && m.GetParameters().Length == 3));
    private static readonly Lazy<MethodInfo> _tryParseEnum = new(() => typeof(Conversions).GetMethods().First(m => m.Name == nameof(TryParseEnum) && m.GetParameters().Length == 2));

    public static string? FormatByteSize(long size)
    {
        var sizeInChars = 256 * 2;
        var pwstr = Marshal.AllocCoTaskMem(sizeInChars);
        try
        {
            StrFormatByteSizeW(size, pwstr, sizeInChars);
            return Marshal.PtrToStringUni(pwstr);
        }
        finally
        {
            Marshal.FreeCoTaskMem(pwstr);
        }
    }

    [LibraryImport("SHLWAPI", StringMarshalling = StringMarshalling.Utf16)]
    [PreserveSig]
    private static partial nint StrFormatByteSizeW(long qdw, [MarshalUsing(CountElementName = nameof(cchBuf))] nint pszBuf, int cchBuf);

    public static bool EqualsIgnoreCase(this string? thisString, string? text, bool trim = true)
    {
        if (trim)
        {
            thisString = thisString.Nullify();
            text = text.Nullify();
        }

        if (thisString == null)
            return text == null;

        if (text == null)
            return false;

        if (thisString.Length != text.Length)
            return false;

        return string.Compare(thisString, text, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static string? Nullify(this string? text)
    {
        if (text == null)
            return null;

        if (string.IsNullOrWhiteSpace(text))
            return null;

        var t = text.Trim();
        return t.Length == 0 ? null : t;
    }

    public static T? ChangeType<T>(object? input, T? defaultValue = default, IFormatProvider? provider = null)
    {
        if (!TryChangeType(input, provider, out T? value))
            return defaultValue;

        return value;
    }

    public static bool TryChangeType<T>(object? input, out T? value) => TryChangeType(input, null, out value);
    public static bool TryChangeType<T>(object? input, IFormatProvider? provider, out T? value)
    {
        var conversionType = typeof(T);
        ArgumentNullException.ThrowIfNull(conversionType);
        if (conversionType == typeof(object))
        {
            value = (T?)input;
            return true;
        }

        value = default;
        if (input == null)
            return !conversionType.IsValueType;

        var inputType = input.GetType();
        if (conversionType.IsAssignableFrom(inputType))
        {
            value = (T?)input;
            return true;
        }

        if (conversionType.IsEnum)
        {
            if (TryParseEnum(typeof(T), input, out var evalue))
            {
                value = (T?)evalue;
                return true;
            }
            return false;
        }

        if (conversionType == typeof(Guid))
        {
            var svalue = string.Format(provider, "{0}", input).Nullify();
            if (svalue != null && Guid.TryParse(svalue, out Guid guid))
            {
                value = (T?)(object?)guid;
                return true;
            }
            return false;
        }

        if (conversionType == typeof(nint))
        {
            if (nint.Size == 8)
            {
                if (TryChangeType(input, provider, out long l))
                {
                    value = (T?)(object?)new nint(l);
                    return true;
                }
            }
            else if (TryChangeType(input, provider, out int i2))
            {
                value = (T?)(object?)new nint(i2);
                return true;
            }
            return false;
        }

        if (conversionType == typeof(int))
        {
            if (inputType == typeof(uint))
            {
                value = (T?)(object?)unchecked((int)(uint)input);
                return true;
            }

            if (inputType == typeof(ulong))
            {
                value = (T?)(object?)unchecked((int)(ulong)input);
                return true;
            }

            if (inputType == typeof(ushort))
            {
                value = (T?)(object?)unchecked((int)(ushort)input);
                return true;
            }

            if (inputType == typeof(byte))
            {
                value = (T?)(object?)unchecked((int)(byte)input);
                return true;
            }

            if (input is string s)
            {
                if (int.TryParse(s, NumberStyles.Any, provider, out var si))
                {
                    value = (T?)(object?)si;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(long))
        {
            if (inputType == typeof(uint))
            {
                value = (T?)(object?)unchecked((long)(uint)input);
                return true;
            }

            if (inputType == typeof(ulong))
            {
                value = (T?)(object?)unchecked((long)(ulong)input);
                return true;
            }

            if (inputType == typeof(ushort))
            {
                value = (T?)(object?)unchecked((long)(ushort)input);
                return true;
            }

            if (inputType == typeof(byte))
            {
                value = (T?)(object?)unchecked((long)(byte)input);
                return true;
            }

            if (input is string s)
            {
                if (long.TryParse(s, NumberStyles.Any, provider, out var sl))
                {
                    value = (T?)(object?)sl;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(short))
        {
            if (inputType == typeof(uint))
            {
                value = (T?)(object?)unchecked((short)(uint)input);
                return true;
            }

            if (inputType == typeof(ulong))
            {
                value = (T?)(object?)unchecked((short)(ulong)input);
                return true;
            }

            if (inputType == typeof(ushort))
            {
                value = (T?)(object?)unchecked((short)(ushort)input);
                return true;
            }

            if (inputType == typeof(byte))
            {
                value = (T?)(object?)unchecked((short)(byte)input);
                return true;
            }

            if (input is string s)
            {
                if (short.TryParse(s, NumberStyles.Any, provider, out var ss))
                {
                    value = (T?)(object?)ss;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(sbyte))
        {
            if (inputType == typeof(uint))
            {
                value = (T?)(object?)unchecked((sbyte)(uint)input);
                return true;
            }

            if (inputType == typeof(ulong))
            {
                value = (T?)(object?)unchecked((sbyte)(ulong)input);
                return true;
            }

            if (inputType == typeof(ushort))
            {
                value = (T?)(object?)unchecked((sbyte)(ushort)input);
                return true;
            }

            if (inputType == typeof(byte))
            {
                value = (T?)(object?)unchecked((sbyte)(byte)input);
                return true;
            }

            if (input is string s)
            {
                if (sbyte.TryParse(s, NumberStyles.Any, provider, out var sb))
                {
                    value = (T?)(object?)sb;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(uint))
        {
            if (inputType == typeof(int))
            {
                value = (T?)(object?)unchecked((uint)(int)input);
                return true;
            }

            if (inputType == typeof(long))
            {
                value = (T?)(object?)unchecked((uint)(long)input);
                return true;
            }

            if (inputType == typeof(short))
            {
                value = (T?)(object?)unchecked((uint)(short)input);
                return true;
            }

            if (inputType == typeof(sbyte))
            {
                value = (T?)(object?)unchecked((uint)(sbyte)input);
                return true;
            }

            if (input is string s)
            {
                if (uint.TryParse(s, NumberStyles.Any, provider, out var ui))
                {
                    value = (T?)(object?)ui;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(ulong))
        {
            if (inputType == typeof(int))
            {
                value = (T?)(object?)unchecked((ulong)(int)input);
                return true;
            }

            if (inputType == typeof(long))
            {
                value = (T?)(object?)unchecked((ulong)(long)input);
                return true;
            }

            if (inputType == typeof(short))
            {
                value = (T?)(object?)unchecked((ulong)(short)input);
                return true;
            }

            if (inputType == typeof(sbyte))
            {
                value = (T?)(object?)unchecked((ulong)(sbyte)input);
                return true;
            }

            if (input is string s)
            {
                if (ulong.TryParse(s, NumberStyles.Any, provider, out var ul))
                {
                    value = (T?)(object?)ul;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(ushort))
        {
            if (inputType == typeof(int))
            {
                value = (T?)(object?)unchecked((ushort)(int)input);
                return true;
            }

            if (inputType == typeof(long))
            {
                value = (T?)(object?)unchecked((ushort)(long)input);
                return true;
            }

            if (inputType == typeof(short))
            {
                value = (T?)(object?)unchecked((ushort)(short)input);
                return true;
            }

            if (inputType == typeof(sbyte))
            {
                value = (T?)(object?)unchecked((ushort)(sbyte)input);
                return true;
            }

            if (input is string s)
            {
                if (ushort.TryParse(s, NumberStyles.Any, provider, out var us))
                {
                    value = (T?)(object?)us;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(byte))
        {
            if (inputType == typeof(int))
            {
                value = (T?)(object?)unchecked((byte)(int)input);
                return true;
            }

            if (inputType == typeof(long))
            {
                value = (T?)(object?)unchecked((byte)(long)input);
                return true;
            }

            if (inputType == typeof(short))
            {
                value = (T?)(object?)unchecked((byte)(short)input);
                return true;
            }

            if (inputType == typeof(sbyte))
            {
                value = (T?)(object?)unchecked((byte)(sbyte)input);
                return true;
            }

            if (input is string s)
            {
                if (byte.TryParse(s, NumberStyles.Any, provider, out var b))
                {
                    value = (T?)(object?)b;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(float))
        {
            if (input is string s)
            {
                if (float.TryParse(s, NumberStyles.Any, provider, out var fl))
                {
                    value = (T?)(object?)fl;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(double))
        {
            if (input is string s)
            {
                if (double.TryParse(s, NumberStyles.Any, provider, out var dbl3))
                {
                    value = (T?)(object?)dbl3;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(decimal))
        {
            if (input is string s)
            {
                if (decimal.TryParse(s, NumberStyles.Any, provider, out var dec))
                {
                    value = (T?)(object?)dec;
                    return true;
                }
                return false;
            }
        }

        if (conversionType == typeof(DateTime) && input is double dbl)
        {
            try
            {
                value = (T?)(object?)DateTime.FromOADate(dbl);
                return true;
            }
            catch
            {
                value = (T?)(object?)DateTime.MinValue;
                return false;
            }
        }

        if (conversionType == typeof(DateTimeOffset) && input is double dbl2)
        {
            try
            {
                value = (T?)(object?)new DateTimeOffset(DateTime.FromOADate(dbl2));
                return true;
            }
            catch
            {
                value = (T?)(object?)DateTimeOffset.MinValue;
                return false;
            }
        }

        if (conversionType == typeof(bool) && TryChangeType<long>(input, out var i))
        {
            value = (T?)(object?)(i != 0);
            return true;
        }

        var nullable = conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>);
        if (nullable)
        {
            if (string.Empty.Equals(input))
            {
                value = (T?)(object?)null;
                return true;
            }

            var type = conversionType.GetGenericArguments()[0];
            var array = new object?[] { input, provider, null };
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning disable IL2060 // Call to 'System.Reflection.MethodInfo.MakeGenericMethod' can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic method.
            if ((bool)_tryChangeType.Value.MakeGenericMethod(type).Invoke(null, array)!)
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(type);
                value = (T?)Activator.CreateInstance(nullableType, array[2]);
                return true;
            }
#pragma warning restore IL2060 // Call to 'System.Reflection.MethodInfo.MakeGenericMethod' can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic method.
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
            value = (T?)(object?)null;
            return false;
        }

        if (input is IConvertible convertible)
        {
            try
            {
                value = (T?)(object?)convertible.ToType(conversionType, provider);
                return true;
            }
            catch
            {
                // do nothing
                return false;
            }
        }

        return false;
    }

    public static object ToEnum<T>(string text) where T : struct, Enum
    {
        if (!TryParseEnum<T>(text, out var value))
            return default(T);

        return value;
    }

    public static bool TryParseEnum(Type type, object input, out object value)
    {
        ArgumentNullException.ThrowIfNull(type);
#pragma warning disable IL2067 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.
        if (!type.IsEnum)
        {
            value = Activator.CreateInstance(type)!;
            return false;
        }

        var parameters = new object?[] { input, null };
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning disable IL2060 // Call to 'System.Reflection.MethodInfo.MakeGenericMethod' can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic method.
        if ((bool)_tryParseEnum.Value.MakeGenericMethod(type).Invoke(null, parameters)!)
        {
            value = parameters[1]!;
            return true;
        }
#pragma warning restore IL2060 // Call to 'System.Reflection.MethodInfo.MakeGenericMethod' can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic method.
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

        value = Activator.CreateInstance(type)!;
#pragma warning restore IL2067 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.
        return false;
    }

    public static bool TryParseEnum<T>(object input, out object value) where T : struct, Enum
    {
        if (input == null)
        {
            value = Activator.CreateInstance<T>();
            return false;
        }

        var stringInput = string.Format(CultureInfo.InvariantCulture, "{0}", input);
        stringInput = stringInput.Nullify();
        if (stringInput == null)
        {
            value = Activator.CreateInstance<T>();
            return false;
        }

        if (stringInput.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && ulong.TryParse(stringInput.AsSpan(2), NumberStyles.HexNumber, null, out var ulx))
        {
            value = ToEnum<T>(ulx.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        var names = Enum.GetNames<T>();
        if (names.Length == 0)
        {
            value = Activator.CreateInstance<T>();
            return false;
        }

        // this is ok for enums
        var values = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).ToArray();
        // some enums like System.CodeDom.MemberAttributes *are* flags but are not declared with Flags...
        if (!typeof(T).IsDefined(typeof(FlagsAttribute), true) && stringInput.IndexOfAny(_enumSeparators) < 0)
            return StringToEnum<T>(names, values, stringInput, out value);

        // multi value enum
        var tokens = stringInput.Split(_enumSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            value = Activator.CreateInstance<T>();
            return false;
        }

        ulong ul = 0;
        foreach (var tok in tokens)
        {
            var token = tok.Nullify(); // NOTE: we don't consider empty tokens as errors
            if (token == null)
                continue;

            if (!StringToEnum<T>(names, values, token, out var tokenValue))
            {
                value = Activator.CreateInstance<T>();
                return false;
            }

            var tokenUl = Convert.GetTypeCode(tokenValue) switch
            {
                TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.SByte => (ulong)Convert.ToInt64(tokenValue, CultureInfo.InvariantCulture),
                _ => Convert.ToUInt64(tokenValue, CultureInfo.InvariantCulture),
            };
            ul |= tokenUl;
        }

        value = EnumToObject<T>(ul);
        return true;
    }

    private static bool StringToEnum<T>(string[] names, Array values, string input, out object value) where T : struct, Enum
    {
        for (var i = 0; i < names.Length; i++)
        {
            if (names[i].EqualsIgnoreCase(input))
            {
                value = values.GetValue(i)!;
                return true;
            }
        }

        for (var i = 0; i < values.GetLength(0); i++)
        {
            var valuei = values.GetValue(i)!;
            if (input.Length > 0 && input[0] == '-')
            {
                var ul = (long)EnumToUInt64(valuei);
                if (ul.ToString().EqualsIgnoreCase(input))
                {
                    value = valuei;
                    return true;
                }
            }
            else
            {
                var ul = EnumToUInt64(valuei);
                if (ul.ToString().EqualsIgnoreCase(input))
                {
                    value = valuei;
                    return true;
                }
            }
        }

        if (char.IsDigit(input[0]) || input[0] == '-' || input[0] == '+')
        {
            var obj = EnumToObject<T>(input);
            if (obj == null)
            {
                value = Activator.CreateInstance<T>();
                return false;
            }

            value = obj;
            return true;
        }

        value = Activator.CreateInstance<T>();
        return false;
    }

    public static ulong EnumToUInt64(object value)
    {
        var typeCode = Convert.GetTypeCode(value);
        return typeCode switch
        {
            TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 => (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture),
            TypeCode.Byte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 => Convert.ToUInt64(value, CultureInfo.InvariantCulture),
            _ => ChangeType<ulong>(value, 0, CultureInfo.InvariantCulture),
        };
    }

    public static object EnumToObject<T>(object value) where T : struct, Enum
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        if (underlyingType == typeof(long))
            return Enum.ToObject(typeof(T), ChangeType<long>(value));

        if (underlyingType == typeof(ulong))
            return Enum.ToObject(typeof(T), ChangeType<ulong>(value));

        if (underlyingType == typeof(int))
            return Enum.ToObject(typeof(T), ChangeType<int>(value));

        if (underlyingType == typeof(uint))
            return Enum.ToObject(typeof(T), ChangeType<uint>(value));

        if (underlyingType == typeof(short))
            return Enum.ToObject(typeof(T), ChangeType<short>(value));

        if (underlyingType == typeof(ushort))
            return Enum.ToObject(typeof(T), ChangeType<ushort>(value));

        if (underlyingType == typeof(byte))
            return Enum.ToObject(typeof(T), ChangeType<byte>(value));

        if (underlyingType == typeof(sbyte))
            return Enum.ToObject(typeof(T), ChangeType<sbyte>(value));

        throw new NotSupportedException();
    }
}

