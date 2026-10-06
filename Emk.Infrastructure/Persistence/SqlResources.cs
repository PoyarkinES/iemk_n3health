using System;
using System.Reflection;
using System.Resources;

namespace Emk.Infrastructure.Persistence
{
    internal static class SqlResources
    {
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("Emk.Properties.Resources", typeof(Emk.Factory).GetTypeInfo().Assembly);

        public static string Get(string name)
        {
            var value = ResourceManager.GetString(name);
            if (value == null)
                throw new MissingManifestResourceException("SQL resource '" + name + "' was not found.");

            return value;
        }
    }
}
