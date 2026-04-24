using System.Collections.Generic;
using NUnit.Framework;
using Proto.TD.Level;
using Proto.TD.Level.Wfc;

namespace Proto.Tests.Wfc
{
    [TestFixture]
    public class WfcConnectivityTests
    {
        [Test]
        public void EachPathReachesCoreBase()
        {
            var spec = WfcDeterminismTests.DefaultMediumSpec(seed: 0xC0FFEE);
            var result = TdLevelWfcAdapter.Build(spec);
            Assert.IsTrue(result.Success, "generation failed: " + result.Reason);

            Assert.IsNotNull(result.Paths);
            Assert.AreEqual(spec.LaneCount, result.Paths.Count, "wrong lane count");

            foreach (var lane in result.Paths)
            {
                Assert.GreaterOrEqual(lane.Count, 2, "lane too short");
                var last = lane[lane.Count - 1];
                Assert.IsTrue(last.Equals(spec.CoreBase),
                    $"lane terminal ({last.X},{last.Y}) != coreBase ({spec.CoreBase.X},{spec.CoreBase.Y})");
            }
        }

        [Test]
        public void FixedCells_ArePreserved()
        {
            var spec = WfcDeterminismTests.DefaultMediumSpec(seed: 0xC0FFEE);
            var result = TdLevelWfcAdapter.Build(spec);
            Assert.IsTrue(result.Success);

            // core base
            Assert.AreEqual(TdDefaultTileSet.T_CoreBase, result.Grid[spec.CoreBase.X, spec.CoreBase.Y]);

            // power sources
            foreach (var p in spec.PowerSources)
                Assert.AreEqual(TdDefaultTileSet.T_PowerPad, result.Grid[p.X, p.Y],
                    $"power source ({p.X},{p.Y}) not preserved");
        }

        [Test]
        public void PathCellsAreRoadTiles()
        {
            var spec = WfcDeterminismTests.DefaultMediumSpec(seed: 0xC0FFEE);
            var result = TdLevelWfcAdapter.Build(spec);
            Assert.IsTrue(result.Success);

            foreach (var lane in result.Paths)
            {
                foreach (var c in lane)
                {
                    if (c.Equals(spec.CoreBase)) continue; // core is special
                    string tile = result.Grid[c.X, c.Y];
                    Assert.IsTrue(
                        tile.StartsWith("road-"),
                        $"path cell ({c.X},{c.Y}) resolved to non-road tile: {tile}");
                }
            }
        }

        [Test]
        public void ManySeeds_AllConverge()
        {
            int successCount = 0;
            int totalRuns = 20;
            var failures = new List<string>();
            for (ulong s = 1; s <= (ulong)totalRuns; s++)
            {
                var spec = WfcDeterminismTests.DefaultMediumSpec(seed: s * 0x1000193UL);
                var r = TdLevelWfcAdapter.Build(spec);
                if (r.Success) successCount++;
                else failures.Add($"seed 0x{spec.Seed:x}: {r.WfcStatus} {r.Reason} @ ({r.ContradictionX},{r.ContradictionY})");
            }

            Assert.GreaterOrEqual(successCount, totalRuns - 2,
                $"convergence rate too low: {successCount}/{totalRuns}\n" + string.Join("\n", failures));
        }
    }
}
