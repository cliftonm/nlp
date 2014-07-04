using System;

namespace Calais
{
    public static class Enum<T>
    {
        public static T Parse(object value)
        {
            if (value != null)
            {
                Type enumType = GetUnderlyingIfNullable(typeof(T));

                if (!enumType.IsEnum)
                {
                    throw new ArgumentException("Argument type must represent enumeration", "value");
                }

                if (!Enum.IsDefined(enumType, value))
                {
                    throw new ArgumentOutOfRangeException("value", value, "Input object did not match an enum value.");
                }

                return (T)Enum.Parse(enumType, value.ToString());
            }

            throw new ArgumentNullException("value");
        }

        public static bool TryParse(object value, out T result)
        {
            if (value != null)
            {
                Type enumType = GetUnderlyingIfNullable(typeof (T));

                if (!enumType.IsEnum)
                {
                    throw new ArgumentException("Argument type must represent enumeration", "value");
                }

                bool isDefined = (Enum.IsDefined(enumType, value));
                result = isDefined ? (T) Enum.Parse(enumType, value.ToString()) : default(T);
                return isDefined;
            }

            throw new ArgumentNullException("value");
        }

        private static Type GetUnderlyingIfNullable(Type t)
        {
            Type type = t;
            Type underlyingType = Nullable.GetUnderlyingType(t);
            if (underlyingType != null) type = underlyingType;
            return type;
        }
    }
}
