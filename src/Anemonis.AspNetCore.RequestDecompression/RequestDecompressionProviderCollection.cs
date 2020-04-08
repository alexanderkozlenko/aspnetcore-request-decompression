// © Alexander Kozlenko. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;

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

        internal RequestDecompressionProviderCollection(IList<Type> list)
            : base(list)
        {
        }

        private void ValidateProviderType(Type value)
        {
            if (!typeof(IDecompressionProvider).IsAssignableFrom(value) || (value.GetCustomAttribute<EncodingNameAttribute>() == null))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.GetString("provider_collection.invalid_type"), typeof(IDecompressionProvider), typeof(EncodingNameAttribute)), nameof(value));
            }
        }

        /// <summary>Adds an object to the end of the <see cref="Collection{T}" />.</summary>
        /// <typeparam name="T">The type of the decompression provider.</typeparam>
        public void Add<T>()
            where T : IDecompressionProvider
        {
            Add(typeof(T));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is <see langword="null" />.</exception>
        protected sealed override void InsertItem(int index, Type item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ValidateProviderType(item);

            if (!Contains(item))
            {
                base.InsertItem(index, item);
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is <see langword="null" />.</exception>
        protected sealed override void SetItem(int index, Type item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ValidateProviderType(item);

            if (!Contains(item))
            {
                base.SetItem(index, item);
            }
        }
    }
}
