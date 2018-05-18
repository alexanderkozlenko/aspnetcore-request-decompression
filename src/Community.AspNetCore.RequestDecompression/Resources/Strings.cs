﻿using System.Resources;

namespace Community.AspNetCore.RequestDecompression.Resources
{
    internal static class Strings
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager(typeof(Strings).Namespace + "." + nameof(Strings), typeof(Strings).Assembly);

        public static string GetString(string name)
        {
            return _resourceManager.GetString(name);
        }
    }
}