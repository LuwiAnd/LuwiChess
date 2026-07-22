using LuwiChess.Core.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LuwiChess.Core.Board
{
    public class BoardState
    {
        public ulong WhitePawns { get; set; }
        public ulong WhiteKnights { get; set; }
        public ulong WhiteBishops { get; set; }
        public ulong WhiteRooks { get; set; }
        public ulong WhiteQueens { get; set; }
        public ulong WhiteKing { get; set; }

        public ulong BlackPawns { get; set; }
        public ulong BlackKnights { get; set; }
        public ulong BlackBishops { get; set; }
        public ulong BlackRooks { get; set; }
        public ulong BlackQueens { get; set; }
        public ulong BlackKing { get; set; }

        public bool WhiteToMove { get; set; }

        public int EnPassantSquare { get; set; } = -1;

        public bool WhiteCanCastleKingSide { get; set; }
        public bool WhiteCanCastleQueenSide { get; set; }
        public bool BlackCanCastleKingSide { get; set; }
        public bool BlackCanCastleQueenSide { get; set; }

        public PieceType GetPieceAtSquare(int square)
        {
            ulong mask = 1UL << square;
            if ((WhitePawns & mask) != 0) return PieceType.WhitePawn;
            if ((WhiteKnights & mask) != 0) return PieceType.WhiteKnight;
            if ((WhiteBishops & mask) != 0) return PieceType.WhiteBishop;
            if ((WhiteRooks & mask) != 0) return PieceType.WhiteRook;
            if ((WhiteQueens & mask) != 0) return PieceType.WhiteQueen;
            if ((WhiteKing & mask) != 0) return PieceType.WhiteKing;
            if ((BlackPawns & mask) != 0) return PieceType.BlackPawn;
            if ((BlackKnights & mask) != 0) return PieceType.BlackKnight;
            if ((BlackBishops & mask) != 0) return PieceType.BlackBishop;
            if ((BlackRooks & mask) != 0) return PieceType.BlackRook;
            if ((BlackQueens & mask) != 0) return PieceType.BlackQueen;
            if ((BlackKing & mask) != 0) return PieceType.BlackKing;
            return PieceType.Empty;
        }

        public int SquareToRank(int square)
        {
            return square / 8;
        }

        public int SquareToFile(int square)
        {
            return square % 8;
        }

        public int RankFileToSquare(int rank, int file)
        {
            return rank * 8 + file;
        }

        public int BitToSquare(ulong bitboard)
        {
            if (bitboard == 0) return -1;
            return BitOperations.TrailingZeroCount(bitboard);
        }

    }

}
