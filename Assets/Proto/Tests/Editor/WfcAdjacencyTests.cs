using NUnit.Framework;
using Proto.Stage.PCG.Wfc;
using Proto.TD.Level;
using Proto.TD.Level.Wfc;

namespace Proto.Tests.Wfc
{
    [TestFixture]
    public class WfcAdjacencyTests
    {
        [Test]
        public void AllAdjacentPairs_SatisfySocketRules()
        {
            var spec = WfcDeterminismTests.DefaultMediumSpec(seed: 0xC0FFEE);
            var result = TdLevelWfcAdapter.Build(spec);
            Assert.IsTrue(result.Success, "generation failed: " + result.Reason);

            var tileSet = Proto.TD.Level.Wfc.TdDefaultTileSet.Build();
            var grid = result.Grid;
            int w = grid.GetLength(0), h = grid.GetLength(1);

            // Check every east-adjacent pair
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    string a = grid[x, y];
                    string b = grid[x + 1, y];
                    bool haveA = tileSet.TryGetIndex(a, out int ia);
                    bool haveB = tileSet.TryGetIndex(b, out int ib);
                    Assert.IsTrue(haveA && haveB,
                        $"missing tile in set: a={a} haveA={haveA}, b={b} haveB={haveB} at ({x},{y})");
                    Assert.IsTrue(
                        tileSet.IsCompatible(ia, Direction.E, ib),
                        $"incompatible east pair at ({x},{y}): {a} -E-> {b}");
                }
            }

            // Check every north-adjacent pair
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    string a = grid[x, y];
                    string b = grid[x, y + 1];
                    bool haveA = tileSet.TryGetIndex(a, out int ia);
                    bool haveB = tileSet.TryGetIndex(b, out int ib);
                    Assert.IsTrue(haveA && haveB,
                        $"missing tile in set: a={a} haveA={haveA}, b={b} haveB={haveB} at ({x},{y})");
                    Assert.IsTrue(
                        tileSet.IsCompatible(ia, Direction.N, ib),
                        $"incompatible north pair at ({x},{y}): {a} -N-> {b}");
                }
            }
        }

        [Test]
        public void BorderCells_AreBorderTile()
        {
            var spec = WfcDeterminismTests.DefaultMediumSpec(seed: 0xC0FFEE);
            var result = TdLevelWfcAdapter.Build(spec);
            Assert.IsTrue(result.Success);

            var grid = result.Grid;
            int w = grid.GetLength(0), h = grid.GetLength(1);
            string border = TdDefaultTileSet.T_BorderVoid;

            for (int x = 0; x < w; x++)
            {
                Assert.AreEqual(border, grid[x, 0],       $"(x={x}, y=0) not border");
                Assert.AreEqual(border, grid[x, h - 1],   $"(x={x}, y={h-1}) not border");
            }
            for (int y = 0; y < h; y++)
            {
                Assert.AreEqual(border, grid[0, y],       $"(x=0, y={y}) not border");
                Assert.AreEqual(border, grid[w - 1, y],   $"(x={w-1}, y={y}) not border");
            }
        }
    }
}
