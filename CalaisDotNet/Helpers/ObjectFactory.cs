using System;
using System.Reflection;

namespace Calais
{
    public class ObjectFactory
    {
        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="ofType">Type of the object to create.</param>
        /// <returns></returns>
        public static T Create<T>(string ofType)
        {
            Type type = Type.GetType(ofType, true, true);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { }); //Faster than Activator.CreateInstance
            return (T)constructor.Invoke(null);
        }
    }
}