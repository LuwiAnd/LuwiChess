using System;
using System.Collections.Generic;
using System.Text;

namespace LuwiChess.Core.Board
{
    public class BitboardMasks
    {
        public const ulong FileA = 0x0101010101010101;
        public const ulong FileB = 0x0202020202020202;
        public const ulong FileC = 0x0404040404040404;
        public const ulong FileD = 0x0808080808080808;
        public const ulong FileE = 0x1010101010101010;
        public const ulong FileF = 0x2020202020202020;
        public const ulong FileG = 0x4040404040404040;
        public const ulong FileH = 0x8080808080808080;

        public const ulong Rank1 = 0x00000000000000FF;
        public const ulong Rank2 = 0x000000000000FF00;
        public const ulong Rank3 = 0x0000000000FF0000;
        public const ulong Rank4 = 0x00000000FF000000;
        public const ulong Rank5 = 0x000000FF00000000;
        public const ulong Rank6 = 0x0000FF0000000000;
        public const ulong Rank7 = 0x00FF000000000000;
        public const ulong Rank8 = 0xFF00000000000000;

        public const ulong BlackSquares = 0x55AA55AA55AA55AA;
        public const ulong WhiteSquares = 0xAA55AA55AA55AA55;

        public const ulong WhitePawnStart = Rank2;
        public const ulong BlackPawnStart = Rank7;
        public const ulong WhitePromotionRank = Rank8;
        public const ulong BlackPromotionRank = Rank1;
        public const ulong WhitePrePromotionRank = Rank7;
        public const ulong BlackPrePromotionRank = Rank2;

        private static readonly ulong[] WhitePawnDiagonalMasks = CreateWhitePawnDiagonalMasks();
        private static readonly ulong[] BlackPawnDiagonalMasks = CreateBlackPawnDiagonalMasks();
        private static readonly ulong[] KnightMasks = CreateKnightMasks();
        private static readonly ulong[] KingMasks = CreateKingMasks();
        //private static readonly ulong[] WhitePawnAttacks = CreateWhitePawnAttackMasks();
        //private static readonly ulong[] BlackPawnAttacks = CreateBlackPawnAttackMasks();
        //private static readonly ulong[] WhitePawnPushes = CreateWhitePawnPushMasks();
        //private static readonly ulong[] BlackPawnPushes = CreateBlackPawnPushMasks();
        private static readonly ulong[] RookMasks = CreateRookRayMasks();
        private static readonly ulong[] RookNorthMasks = CreateRookNorthMasks();
        private static readonly ulong[] RookSouthMasks = CreateRookSouthMasks();
        private static readonly ulong[] RookWestMasks = CreateRookWestMasks();
        private static readonly ulong[] RookEastMasks = CreateRookEastMasks();

        private static readonly ulong[] BishopMasks = CreateBishopRayMasks();
        private static readonly ulong[] BishopNorthWestMasks = CreateBishopNorthWestMasks();
        private static readonly ulong[] BishopNorthEastMasks = CreateBishopNorthEastMasks();
        private static readonly ulong[] BishopSouthWestMasks = CreateBishopSouthWestMasks();
        private static readonly ulong[] BishopSouthEastMasks = CreateBishopSouthEastMasks();


        public static ulong GetWhitePawnDiagonalMasks(int square) => WhitePawnDiagonalMasks[square];
        public static ulong GetBlackPawnDiagonalMasks(int square) => BlackPawnDiagonalMasks[square];
        public static ulong GetKnightMasks(int square) => KnightMasks[square];
        public static ulong GetKingMasks(int square) => KingMasks[square];
        //public static ulong GetWhitePawnAttackMask(int square) => WhitePawnAttacks[square];
        //public static ulong GetBlackPawnAttackMask(int square) => BlackPawnAttacks[square];
        //public static ulong GetWhitePawnPushMask(int square) => WhitePawnPushes[square];
        //public static ulong GetBlackPawnPushMask(int square) => BlackPawnPushes[square];
        public static ulong GetRookRayMasks(int square) => RookMasks[square];
        public static ulong GetBishopRayMasks(int square) => BishopMasks[square];

        public static ulong GetRookNorthMasks(int square) => RookNorthMasks[square];
        public static ulong GetRookSouthMasks(int square) => RookSouthMasks[square];
        public static ulong GetRookWestMasks(int square) => RookWestMasks[square];
        public static ulong GetRookEastMasks(int square) => RookEastMasks[square];

        public static ulong GetBishopNorthWestMasks(int square) => BishopNorthWestMasks[square];
        public static ulong GetBishopNorthEastMasks(int square) => BishopNorthEastMasks[square];
        public static ulong GetBishopSouthWestMasks(int square) => BishopSouthWestMasks[square];
        public static ulong GetBishopSouthEastMasks(int square) => BishopSouthEastMasks[square];

        private static ulong[] CreateKnightMasks()
        {
            ulong[] masks = new ulong[64];

            (int rank, int file)[] knightMoves = new (int, int)[]
            {
                (2, 1), 
                (2, -1), 
                (1, 2), 
                (1, -2), 
                (-1, 2), 
                (-1, -2),
                (-2, 1), 
                (-2, -1)
            };

            for(int i = 0; i < 64; i++)
            {
                int rank = i / 8;
                int file = i % 8;
                foreach (var move in knightMoves)
                {
                    int targetRank = rank + move.rank;
                    int targetFile = file + move.file;
                    if (0 <= targetRank && targetRank < 8 && 0 <= targetFile && targetFile < 8)
                    {
                        int targetSquare = targetRank * 8 + targetFile;
                        masks[i] |= (1UL << targetSquare);
                    }
                }
            }

            return masks;
        }

        private static ulong[] CreateKingMasks()
        {
            ulong[] masks = new ulong[64];

            (int rank, int file)[] kingMoves = new (int, int)[]
            {
                (1, 1),
                (1, 0),
                (1, -1),

                (0, 1),
                (0, -1),

                (-1, 1),
                (-1, 0),
                (-1, -1)
            };

            for (int i = 0; i < 64; i++)
            {
                int rank = i / 8;
                int file = i % 8;
                foreach (var move in kingMoves)
                {
                    int targetRank = rank + move.rank;
                    int targetFile = file + move.file;
                    if (0 <= targetRank && targetRank < 8 && 0 <= targetFile && targetFile < 8)
                    {
                        int targetSquare = targetRank * 8 + targetFile;
                        masks[i] |= (1UL << targetSquare);
                    }
                }
            }

            return masks;
        }

        private static ulong[] CreateRookRayMasks()
        {
            ulong[] masks = new ulong[64];

            for (int i = 0; i < 64; i++)
            {
                ulong squareMask = 1UL << i;

                int rankIndex = i / 8;
                int fileIndex = i % 8;

                //Rank1 = 0x00000000000000FFUL = 0xFFUL;
                ulong rankMask = Rank1 << (rankIndex * 8);
                ulong fileMask = FileA << fileIndex;

                masks[i] = (rankMask | fileMask) & ~squareMask;
            }
            return masks;
        }


        private static ulong[] CreateRookNorthMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                int rankIndex = i / 8;
                int fileIndex = i % 8;
                ulong northMask = 0UL;
                for (int step = 1; step < 8; step++)
                {
                    if (rankIndex + step >= 8)
                        break;

                    northMask |= 1UL << ((rankIndex + step) * 8 + fileIndex);
                }
                masks[i] = northMask;
            }
            return masks;
        }

        private static ulong[] CreateRookSouthMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                int rankIndex = i / 8;
                int fileIndex = i % 8;
                ulong southMask = 0UL;
                for (int step = 1; step < 8; step++)
                {
                    if (rankIndex - step < 0)
                        break;

                    southMask |= 1UL << ((rankIndex - step) * 8 + fileIndex);
                }
                masks[i] = southMask;
            }
            return masks;
        }

        private static ulong[] CreateRookWestMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                int rankIndex = i / 8;
                int fileIndex = i % 8;
                ulong westMask = 0UL;
                for (int step = 1; step < 8; step++)
                {
                    if (fileIndex - step < 0)
                        break;

                    westMask |= 1UL << (rankIndex * 8 + (fileIndex - step));
                }
                masks[i] = westMask;
            }
            return masks;
        }

        private static ulong[] CreateRookEastMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                int rankIndex = i / 8;
                int fileIndex = i % 8;
                ulong eastMask = 0UL;
                for (int step = 1; step < 8; step++)
                {
                    if (fileIndex + step >= 8)
                        break;
                    eastMask |= 1UL << (rankIndex * 8 + (fileIndex + step));
                }
                masks[i] = eastMask;
            }
            return masks;
        }


        private static ulong[] CreateBishopRayMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                ulong squareMask = 1UL << i;
                int rankIndex = i / 8;
                int fileIndex = i % 8;
                //ulong diagonalMask = 0UL;
                //ulong antiDiagonalMask = 0UL;
                ulong bishopMask = 0UL;
                for (int step = 1; step < 8; step++)
                {
                    if(rankIndex + step < 8 && fileIndex + step < 8)
                        bishopMask |= 1UL << ((rankIndex + step) * 8 + (fileIndex + step));

                    if(rankIndex + step < 8 && fileIndex - step >= 0)
                        bishopMask |= 1UL << ((rankIndex + step) * 8 + (fileIndex - step));

                    if(rankIndex - step >= 0 && fileIndex + step < 8)
                        bishopMask |= 1UL << ((rankIndex - step) * 8 + (fileIndex + step));

                    if(rankIndex - step >= 0 && fileIndex - step >= 0)
                        bishopMask |= 1UL << ((rankIndex - step) * 8 + (fileIndex - step));
                }
                //masks[i] = (diagonalMask | antiDiagonalMask) & ~squareMask;
                //masks[i] = bishopMask & ~squareMask;
                masks[i] = bishopMask;
            }

            return masks;
        }



        private static ulong[] CreateBishopNorthWestMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                int rankIndex = i / 8;
                int fileIndex = i % 8;

                ulong nwMask = 0UL;

                for(int step = 1; step < 8; step++)
                {
                    if (rankIndex + step < 8 && fileIndex - step >= 0)
                        nwMask |= 1UL << ((rankIndex + step) * 8 + (fileIndex - step));
                }

                masks[i] = nwMask;
            }
            return masks;
        }

        private static ulong[] CreateBishopNorthEastMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                int rankIndex = i / 8;
                int fileIndex = i % 8;

                ulong neMask = 0UL;
                for (int step = 1; step < 8; step++)
                {
                    if (rankIndex + step < 8 && fileIndex + step < 8)
                        neMask |= 1UL << ((rankIndex + step) * 8 + (fileIndex + step));
                }

                masks[i] = neMask;
            }
            return masks;
        }

        private static ulong[] CreateBishopSouthWestMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                int rankIndex = i / 8;
                int fileIndex = i % 8;
                ulong swMask = 0UL;
                for (int step = 1; step < 8; step++)
                {
                    if (rankIndex - step >= 0 && fileIndex - step >= 0)
                        swMask |= 1UL << ((rankIndex - step) * 8 + (fileIndex - step));
                }

                masks[i] = swMask;
            }
            return masks;
        }


        private static ulong[] CreateBishopSouthEastMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                int rankIndex = i / 8;
                int fileIndex = i % 8;
                
                ulong seMask = 0UL;
                for (int step = 1; step < 8; step++)
                {
                    if (rankIndex - step >= 0 && fileIndex + step < 8)
                        seMask |= 1UL << ((rankIndex - step) * 8 + (fileIndex + step));
                }

                masks[i] = seMask;
            }
            return masks;
        }



        // Masks for each square that represent from which squares that square can be attacked by a white pawn.
        // For example, the mask for square e4 will have bits set for d3 and f3.
        private static ulong[] CreateWhitePawnDiagonalMasks()
        {
            ulong[] masks = new ulong[64];
            for(int i = (8*2); i < 64; i++)
            {
                int file = i % 8;
                ulong attackTowardsHFile = 0UL;
                ulong attackTowardsAFile = 0UL;

                if (file < 7)
                    attackTowardsHFile = 1UL << (i - 7);
                
                if (file > 0)
                    attackTowardsAFile = 1UL << (i - 9);

                masks[i] = attackTowardsHFile | attackTowardsAFile;
            }

            return masks;
        }

        private static ulong[] CreateBlackPawnDiagonalMasks()
        {
            ulong[] masks = new ulong[64];
            for (int i = 0; i < (8 * 6); i++)
            {
                int file = i % 8;
                ulong attackTowardsHFile = 0UL;
                ulong attackTowardsAFile = 0UL;
                if (file < 7)
                    attackTowardsHFile = 1UL << (i + 9);

                if (file > 0)
                    attackTowardsAFile = 1UL << (i + 7);
                masks[i] = attackTowardsHFile | attackTowardsAFile;
            }
            return masks;
        }
    }
}
