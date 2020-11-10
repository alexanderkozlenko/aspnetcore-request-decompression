// © Alexander Kozlenko. Licensed under the MIT License.

using System;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Specifies the encoding name used in the "Content-Encoding" header for a decompression provider.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EncodingNameAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="EncodingNameAttribute" /> class.</summary>
        /// <param name="encodingName">The encoding name used in the "Content-Encoding" header.</param>
        /// <exception cref="ArgumentNullException"><paramref name="encodingName" /> is <see langword="null" />.</exception>
        public EncodingNameAttribute(string encodingName)
        {
            if (encodingName is null)
            {
                throw new ArgumentNullException(nameof(encodingName));
            }

            EncodingName = encodingName;
        }

        internal string EncodingName
        {
            get;
        }
    }
}
