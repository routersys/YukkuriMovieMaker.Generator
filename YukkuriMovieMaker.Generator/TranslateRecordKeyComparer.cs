using System.Collections.Generic;

namespace YukkuriMovieMaker.Generator
{
    class TranslateRecordKeyComparer : IEqualityComparer<TranslateRecord>
    {
        public bool Equals(TranslateRecord x, TranslateRecord y) => x.Key == y.Key;
        public int GetHashCode(TranslateRecord obj) => obj?.Key?.GetHashCode() ?? 0;
    }
}
