// © Alexander Kozlenko. Licensed under the MIT License.

using System.Globalization;
using System.Resources;

namespace Anemonis.AspNetCore.RequestDecompression.Resources
{
    internal static class Strings
    {
        private static readonly ResourceManager s_resourceManager = new(typeof(Strings).Namespace + "." + nameof(Strings), typeof(Strings).Assembly);

        public static string GetString(string name)
        {
            return s_resourceManager.GetString(name, CultureInfo.CurrentCulture);
        }
    }
}
