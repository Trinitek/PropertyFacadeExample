using System.Collections.Generic;
using System;

namespace PropertyFacadeExample
{
    public sealed class NullOrEmptyStringEqualityComparer : EqualityComparer<string>
    {
        public static new NullOrEmptyStringEqualityComparer Default { get; } = new NullOrEmptyStringEqualityComparer(StringComparer.InvariantCulture);

        public IEqualityComparer<string> InnerComparer { get; }

        public NullOrEmptyStringEqualityComparer(IEqualityComparer<string> innerComparer)
        {
            InnerComparer = innerComparer ?? throw new ArgumentNullException(nameof(innerComparer));
        }

        public override bool Equals(string x, string y)
        {
            if (string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y))
            {
                return true;
            }
            else
            {
                return InnerComparer.Equals(x, y);
            }
        }

        public override int GetHashCode(string obj) => InnerComparer.GetHashCode(obj);
    }
}