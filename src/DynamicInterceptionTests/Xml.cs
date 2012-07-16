using Castle.Core.Resource;
using System;
using System.IO;

namespace DynamicInterceptorFacilityTests
{
    public class Xml
    {
        private static readonly string embedded = "assembly://" + typeof(Xml).Assembly.FullName + "/XmlFiles/";

        public static IResource Embedded(string name)
        {
            var uri = new CustomUri(EmbeddedPath(name));
            var resource = new AssemblyResource(uri);
            return resource;
        }

        public static string EmbeddedPath(string name)
        {
            return embedded + name;
        }

        public static IResource File(string name)
        {
            var uri = new CustomUri(FilePath(name));
            var resource = new FileResource(uri);
            return resource;
        }

        public static string FilePath(string name)
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XmlFiles/" + name);
            return fullPath;
        }
    }
}

