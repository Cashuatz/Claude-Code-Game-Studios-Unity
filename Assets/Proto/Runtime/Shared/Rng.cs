using System;

namespace Proto.TD.Sim
{
    /// <summary>
    /// HR-TD-2 공식 RNG. Xorshift64 — 순수 C#, UnityEngine 참조 금지.
    /// Seed=0은 자동으로 1로 치환해 all-zero lock 상태를 회피한다.
    /// 동일 seed + 동일 호출 순서 → 동일 결과.
    /// </summary>
    public sealed class Rng
    {
        private ulong _state;

        public Rng(ulong seed)
        {
            _state = seed == 0UL ? 1UL : seed;
        }

        public ulong Seed => _state;

        public ulong NextULong()
        {
            ulong x = _state;
            x ^= x << 13;
            x ^= x >> 7;
            x ^= x << 17;
            _state = x;
            return x;
        }

        public int NextInt()
        {
            return (int)(NextULong() & 0x7fffffffUL);
        }

        /// <summary>
        /// [0, maxExclusive) 에서 균등 정수 샘플. maxExclusive &lt;= 0 은 예외.
        /// Lemire 의 bias-corrected multiply 기법.
        /// </summary>
        public int NextIntRange(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "must be > 0");

            uint m = (uint)maxExclusive;
            uint x = (uint)(NextULong() & 0xffffffffUL);
            ulong product = (ulong)x * m;
            uint low = (uint)product;
            if (low < m)
            {
                uint threshold = (uint)(-(int)m) % m;
                while (low < threshold)
                {
                    x = (uint)(NextULong() & 0xffffffffUL);
                    product = (ulong)x * m;
                    low = (uint)product;
                }
            }
            return (int)(product >> 32);
        }

        /// <summary>[0.0, 1.0) 균등 double.</summary>
        public double NextDouble()
        {
            return (NextULong() >> 11) * (1.0 / (1UL << 53));
        }

        /// <summary>
        /// 가중치 목록에서 하나 선택. weights 는 모두 ≥ 0 이어야 하고 총합 &gt; 0.
        /// HR-TD-2 결정론 유지 — NextIntRange 와 NextDouble 조합.
        /// </summary>
        public int WeightedPick(ReadOnlySpan<int> weights)
        {
            long total = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] < 0)
                    throw new ArgumentException("negative weight", nameof(weights));
                total += weights[i];
            }
            if (total <= 0)
                throw new ArgumentException("total weight must be > 0", nameof(weights));

            long roll = (long)(NextDouble() * total);
            if (roll >= total) roll = total - 1;

            long acc = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (roll < acc) return i;
            }
            return weights.Length - 1;
        }
    }
}
