using NUnit.Framework;
using Proto.Stage.PCG.Wfc;
using Proto.TD.Level;
using Proto.TD.Level.Wfc;
using Proto.TD.Sim;

namespace Proto.Tests.Wfc
{
    [TestFixture]
    public class WfcDeterminismTests
    {
        [Test]
        public void Rng_SameSeed_ProducesSameSequence()
        {
            var a = new Rng(0xC0FFEE);
            var b = new Rng(0xC0FFEE);
            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(a.NextULong(), b.NextULong(), $"mismatch at i={i}");
        }

        [Test]
        public void Rng_DifferentSeed_ProducesDifferentSequence()
        {
            var a = new Rng(1);
            var b = new Rng(2);
            bool anyDiff = false;
            for (int i = 0; i < 10; i++)
            {
                if (a.NextULong() != b.NextULong()) { anyDiff = true; break; }
            }
            Assert.IsTrue(anyDiff, "different seeds produced identical first 10 values");
        }

        [Test]
        public void Solver_SameInput_ProducesSameGrid()
        {
            var spec1 = DefaultMediumSpec(seed: 0xC0FFEE);
            var spec2 = DefaultMediumSpec(seed: 0xC0FFEE);
            var r1 = TdLevelWfcAdapter.Build(spec1);
            var r2 = TdLevelWfcAdapter.Build(spec2);

            Assert.IsTrue(r1.Success, $"run 1 failed: {r1.Reason} wfc={r1.WfcStatus}");
            Assert.IsTrue(r2.Success, $"run 2 failed: {r2.Reason} wfc={r2.WfcStatus}");
            AssertGridsEqual(r1.Grid, r2.Grid);
            Assert.AreEqual(r1.Iterations, r2.Iterations, "iteration count diverged");
        }

        [Test]
        public void Solver_DifferentSeed_ProducesDifferentGrid()
        {
            var r1 = TdLevelWfcAdapter.Build(DefaultMediumSpec(seed: 0xC0FFEE));
            var r2 = TdLevelWfcAdapter.Build(DefaultMediumSpec(seed: 0xDEADBEEF));

            Assert.IsTrue(r1.Success && r2.Success, "both runs should succeed");
            bool anyDiff = false;
            int w = r1.Grid.GetLength(0), h = r1.Grid.GetLength(1);
            for (int y = 0; y < h && !anyDiff; y++)
                for (int x = 0; x < w && !anyDiff; x++)
                    if (r1.Grid[x, y] != r2.Grid[x, y]) anyDiff = true;
            Assert.IsTrue(anyDiff, "different seeds produced identical grid");
        }

        [Test]
        public void Solver_SameSeed_ProducesStableJson()
        {
            var r1 = TdLevelWfcAdapter.Build(DefaultMediumSpec(seed: 0xC0FFEE));
            var r2 = TdLevelWfcAdapter.Build(DefaultMediumSpec(seed: 0xC0FFEE));
            Assert.IsTrue(r1.Success && r2.Success);
            Assert.AreEqual(r1.LevelJson, r2.LevelJson, "JSON output not byte-stable");
        }

        // ── helpers ────────────────────────────────────────────
        internal static TdLevelWfcAdapter.Spec DefaultMediumSpec(ulong seed)
        {
            return new TdLevelWfcAdapter.Spec
            {
                GridSize = 40,
                Seed = seed,
                LaneCount = 2,
                CoreBase = new AStarGrid.Cell(20, 20),
                PowerSources = new[] { new AStarGrid.Cell(5, 5), new AStarGrid.Cell(34, 34) },
                TileSet = null,
                LinkBudget = 100,
                LevelId = "test-level"
            };
        }

        internal static void AssertGridsEqual(string[,] a, string[,] b)
        {
            Assert.AreEqual(a.GetLength(0), b.GetLength(0), "width differs");
            Assert.AreEqual(a.GetLength(1), b.GetLength(1), "height differs");
            int w = a.GetLength(0), h = a.GetLength(1);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    Assert.AreEqual(a[x, y], b[x, y], $"cell ({x},{y}) differs");
        }
    }
}
