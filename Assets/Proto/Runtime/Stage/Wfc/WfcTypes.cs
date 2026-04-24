using System;
using System.Collections.Generic;

namespace Proto.Stage.PCG.Wfc
{
    // ──────────────────────────────────────────────────────────────
    //  순수 C# WFC 데이터 타입. UnityEngine 참조 금지.
    //  SO 래퍼 (WfcTileSO / WfcTileSetSO) 는 별도 파일.
    // ──────────────────────────────────────────────────────────────

    public enum TileRole : byte
    {
        Void = 0,
        Path = 1,
        Grass = 2,
        Building = 3,
        PowerPad = 4,
        TowerPad = 5,
        CoreBase = 6,
        Border = 7
    }

    public enum Direction : byte { N = 0, E = 1, S = 2, W = 3 }

    public static class DirectionOps
    {
        public static Direction Opposite(Direction d) => (Direction)(((int)d + 2) & 3);
        /// <summary>
        /// 컨벤션: y+1 = 북쪽. Unity world z+ 방향과 일치.
        /// </summary>
        public static (int dx, int dy) Delta(Direction d)
        {
            switch (d)
            {
                case Direction.N: return (0, 1);
                case Direction.E: return (1, 0);
                case Direction.S: return (0, -1);
                case Direction.W: return (-1, 0);
                default: return (0, 0);
            }
        }
    }

    /// <summary>소켓 식별자. 같은 <see cref="SocketId"/> 끼리는 항상 매칭.</summary>
    public readonly struct Socket : IEquatable<Socket>
    {
        public readonly string Id;
        public Socket(string id) { Id = id ?? string.Empty; }
        public bool Equals(Socket other) => string.Equals(Id, other.Id, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is Socket s && Equals(s);
        public override int GetHashCode() => Id == null ? 0 : StringComparer.Ordinal.GetHashCode(Id);
        public override string ToString() => Id ?? "";
    }

    /// <summary>
    /// WFC 타일 정의 (순수 데이터). 4방향 소켓 + 역할 + 가중.
    /// 회전 파생물은 <see cref="WfcTileSetData"/> 빌드 시 자동 생성.
    /// </summary>
    public sealed class WfcTileData
    {
        public readonly string TileId;
        public readonly TileRole Role;
        public readonly Socket[] Sockets; // [N, E, S, W]
        public readonly int Weight;
        public readonly bool AllowRotation;
        /// <summary>회전 파생물일 경우 원본 ID, 아니면 null.</summary>
        public readonly string OriginId;
        /// <summary>회전 횟수 (0~3). 0 = 원본.</summary>
        public readonly int RotationSteps;

        public WfcTileData(
            string tileId,
            TileRole role,
            Socket north, Socket east, Socket south, Socket west,
            int weight,
            bool allowRotation,
            string originId = null,
            int rotationSteps = 0)
        {
            if (string.IsNullOrEmpty(tileId)) throw new ArgumentException("tileId required", nameof(tileId));
            if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight), "weight must be ≥ 0");
            if (rotationSteps < 0 || rotationSteps > 3) throw new ArgumentOutOfRangeException(nameof(rotationSteps));

            TileId = tileId;
            Role = role;
            Sockets = new[] { north, east, south, west };
            Weight = weight;
            AllowRotation = allowRotation;
            OriginId = originId;
            RotationSteps = rotationSteps;
        }

        public Socket SocketOf(Direction dir) => Sockets[(int)dir];
    }

    /// <summary>
    /// 타일셋 (빌드된 인덱스 포함, 순수 C#).
    /// 소켓 매칭: 동일 소켓 ID 끼리만 매칭.
    /// 현재 구현은 n ≤ 64 로 제한 (ulong bitmask 경로). 초과 시 생성자에서 예외.
    /// </summary>
    public sealed class WfcTileSetData
    {
        public const int MaxTiles = 64;

        public readonly IReadOnlyList<WfcTileData> Tiles;
        public readonly Dictionary<string, int> IndexById;
        public readonly string BorderTileId;
        public readonly string DefaultGrassTileId;

        // 방향별 호환성 매트릭스. CompatibleMask[tileIndex, (int)dir] = 해당 방향 이웃으로 허용되는 타일 인덱스 비트마스크.
        public readonly ulong[,] CompatibleMask;

        public int TileCount => Tiles.Count;

        public WfcTileSetData(IReadOnlyList<WfcTileData> tiles, string borderTileId, string defaultGrassTileId)
        {
            if (tiles == null || tiles.Count == 0)
                throw new ArgumentException("tiles must be non-empty", nameof(tiles));
            if (tiles.Count > MaxTiles)
                throw new ArgumentException($"tileset exceeds MaxTiles={MaxTiles} (got {tiles.Count})", nameof(tiles));

            Tiles = tiles;
            IndexById = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < tiles.Count; i++)
            {
                if (IndexById.ContainsKey(tiles[i].TileId))
                    throw new ArgumentException($"duplicate tileId: {tiles[i].TileId}", nameof(tiles));
                IndexById[tiles[i].TileId] = i;
            }

            BorderTileId = borderTileId;
            DefaultGrassTileId = defaultGrassTileId;

            int n = tiles.Count;
            CompatibleMask = new ulong[n, 4];

            for (int i = 0; i < n; i++)
            {
                for (int d = 0; d < 4; d++)
                {
                    Socket mySocket = tiles[i].SocketOf((Direction)d);
                    Direction opposite = DirectionOps.Opposite((Direction)d);
                    ulong mask = 0UL;
                    for (int j = 0; j < n; j++)
                    {
                        if (tiles[j].SocketOf(opposite).Equals(mySocket))
                            mask |= 1UL << j;
                    }
                    CompatibleMask[i, d] = mask;
                }
            }
        }

        public bool IsCompatible(int tileIndexA, Direction aToB, int tileIndexB)
        {
            return (CompatibleMask[tileIndexA, (int)aToB] & (1UL << tileIndexB)) != 0UL;
        }

        public bool TryGetIndex(string tileId, out int index) => IndexById.TryGetValue(tileId, out index);

        /// <summary>
        /// 회전 파생물을 포함한 타일 빌더. <paramref name="source"/> 에 원본 타일만 넣고
        /// AllowRotation=true 인 것은 3회전 (90/180/270) 파생물을 자동으로 확장.
        /// 파생물 TileId 는 "<source.TileId>@r{1,2,3}".
        /// </summary>
        public static WfcTileSetData BuildWithRotations(
            IEnumerable<WfcTileData> source,
            string borderTileId,
            string defaultGrassTileId)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var expanded = new List<WfcTileData>();
            foreach (var t in source)
            {
                expanded.Add(t);
                if (!t.AllowRotation) continue;
                for (int r = 1; r <= 3; r++)
                {
                    Socket[] s = t.Sockets;
                    // 90° CW rotation: 새 N = 옛 W, E = N, S = E, W = S
                    // r회전이면 소켓을 r 스텝 rotate-right 한다.
                    Socket nN = s[(0 - r + 4) & 3];
                    Socket nE = s[(1 - r + 4) & 3];
                    Socket nS = s[(2 - r + 4) & 3];
                    Socket nW = s[(3 - r + 4) & 3];
                    expanded.Add(new WfcTileData(
                        tileId: t.TileId + "@r" + r,
                        role: t.Role,
                        north: nN, east: nE, south: nS, west: nW,
                        weight: t.Weight,
                        allowRotation: false,
                        originId: t.TileId,
                        rotationSteps: r));
                }
            }
            return new WfcTileSetData(expanded, borderTileId, defaultGrassTileId);
        }
    }

    public struct WfcFixedCell
    {
        public int X;
        public int Y;
        public string TileId;
        public WfcFixedCell(int x, int y, string tileId) { X = x; Y = y; TileId = tileId; }
    }

    public struct WfcInput
    {
        public int Width;
        public int Height;
        public ulong Seed;
        public WfcTileSetData TileSet;
        public IReadOnlyList<WfcFixedCell> FixedCells;
    }

    public enum WfcStatus : byte { Ok = 0, Contradiction = 1, InvalidInput = 2, TilesetIncomplete = 3 }

    public struct WfcOutput
    {
        public WfcStatus Status;
        public string Reason;
        public string[,] Grid;         // [x, y]
        public int Iterations;
        public int ContradictionCellX;
        public int ContradictionCellY;
    }
}
