using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Web.Mvc;

namespace Terminal.Core.ExtensionMethods
{
    public static class GenericExtensions
    {
        public static string Serialize<T>(this T toSerialize)
        {
            var serializer = new MvcSerializer();
            return serializer.Serialize(toSerialize, MvcSerializer.DefaultSerializationMode);
        }

        public static T Deserialize<T>(this string toDeserialize)
        {
            var serializer = new MvcSerializer();
            return (T)serializer.Deserialize(toDeserialize, MvcSerializer.DefaultSerializationMode);
        }
    }
}
