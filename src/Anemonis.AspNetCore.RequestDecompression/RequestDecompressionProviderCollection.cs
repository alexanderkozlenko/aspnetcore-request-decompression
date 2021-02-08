// © Alexander Kozlenko. Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Globalization;

using Anemonis.AspNetCore.RequestDecompression.Resources;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>A collection of request decompression provider types.</summary>
    public sealed class RequestDecompressionProviderCollection : Collection<Type>
    {
        internal RequestDecompressionProviderCollection()
            : base()
        {
        }

        /// <summary>Adds an object to the end of the <see cref="Collection{T}" />.</summary>
        /// <typeparam name="T">The type of the decompression provider.</typeparam>
        public void Add<T>()
            where T : IDecompressionProvider
        {
            base.Add(typeof(T));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is <see langword="null" />.</exception>
        protected sealed override void InsertItem(int index, Type item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (!typeof(IDecompressionProvider).IsAssignableFrom(item))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.GetString("provider_collection.invalid_type"), typeof(IDecompressionProvider)), nameof(item));
            }

            if (!Contains(item))
            {
                base.InsertItem(index, item);
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is <see langword="null" />.</exception>
        protected sealed override void SetItem(int index, Type item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (!typeof(IDecompressionProvider).IsAssignableFrom(item))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.GetString("provider_collection.invalid_type"), typeof(IDecompressionProvider)), nameof(item));
            }

            if (!Contains(item))
            {
                base.SetItem(index, item);
            }
        }
    }
}
