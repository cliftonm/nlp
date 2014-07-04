using System;
using System.Reflection;

namespace Calais
{
    /// <summary>
    /// Used to implement string enums.
    /// </summary>
    /// <remarks>
    /// Based on:
    /// http://www.codeproject.com/KB/cs/stringenum.aspx
    /// </remarks>
    public class StringEnum
    {
        public static string GetString(Enum enumValue)
        {
            Type enumType = enumValue.GetType();
            FieldInfo fieldInfo = enumType.GetField(enumValue.ToString());

            EnumStringAttribute[] attributes =
                fieldInfo.GetCustomAttributes(typeof(EnumStringAttribute), false) as EnumStringAttribute[];

            if (attributes != null)
            { 
                return (attributes.Length > 0) ? attributes[0].Value : string.Empty; 
            }

            return string.Empty;
        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value (case sensitive).
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="stringValue">String value.</param>
        /// <returns>Enum value associated with the string value, or null if not found.</returns>
        public static object Parse(Type type, string stringValue)
        {
            return Parse(type, stringValue, false);
        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="stringValue">String value.</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Enum value associated with the string value, or null if not found.</returns>
        public static object Parse(Type type, string stringValue, bool ignoreCase)
        {
            object output = null;
            string enumStringValue = null;

            if (!type.IsEnum)
                throw new ArgumentException(String.Format("Supplied type must be an Enum.  Type was {0}", type));

            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in type.GetFields())
            {
                //Check for our custom attribute
                EnumStringAttribute[] attrs = fi.GetCustomAttributes(typeof(EnumStringAttribute), false) as EnumStringAttribute[];
                if (attrs.Length > 0)
                {
                    enumStringValue = attrs[0].Value;
                }

                //Check for equality then select actual enum value.
                if (string.Compare(enumStringValue, stringValue, ignoreCase) != 0) continue;

                output = Enum.Parse(type, fi.Name);
                break;
            }

            return output;
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <param name="enumType">Type of enum</param>
        /// <returns>Existence of the string value</returns>
        public static bool IsStringDefined(Type enumType, string stringValue)
        {
            return Parse(enumType, stringValue) != null;
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <param name="enumType">Type of enum</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Existence of the string value</returns>
        public static bool IsStringDefined(Type enumType, string stringValue, bool ignoreCase)
        {
            return Parse(enumType, stringValue, ignoreCase) != null;
        }
    }

    /// <summary>
    /// Attributes used to associated a string value with an enumeration
    /// </summary>
    internal class EnumStringAttribute : Attribute
    {
        private string _value;
        public EnumStringAttribute(string value) { _value = value; }
        public string Value { get { return _value; } }
    }
}
