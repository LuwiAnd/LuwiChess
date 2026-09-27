using LuwiChess.Core.Board;
using LuwiChess.Core.Entities;
using LuwiChess.Core.Enums;
using LuwiChess.Core.Moves;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LuwiChess.Core.Rules
{
    public class MoveGenerator : IMoveGenerator
    {
        private readonly IMoveApplier _moveApplier;

        public MoveGenerator(IMoveApplier moveApplier)
        {
            _moveApplier = moveApplier;
        }

        public List<Move> GetLegalMoves(BoardState board)
        {
            List<Move> legalMoves = new List<Move>();
            List<Move> pseudoLegalMoves = GeneratePseudoLegalMoves(board);

            bool movingSideIsWhite = board.WhiteToMove;

            foreach (Move move in pseudoLegalMoves)
            {
                BoardState nextPosition = _moveApplier.MakeMove(board, move);

                //int ownColorSign = movingSideIsWhite ? 1 : -1;

                if (!IsKingInCheck(nextPosition, movingSideIsWhite))
                {
                    legalMoves.Add(move);
                }
            }

            return legalMoves;
        }

        private bool IsKingInCheck(BoardState board, bool kingIsWhite)
        {
            ulong king = kingIsWhite ? board.WhiteKing : board.BlackKing;

            if (king == 0)
            {
                throw new InvalidOperationException(
                    kingIsWhite
                        ? "White king is missing."
                        : "Black king is missing.");
            }

            int kingSquare = BitOperations.TrailingZeroCount(king);
            return IsSquareAttacked(byWhite: !kingIsWhite, board, kingSquare);
        }


        private List<Move> GeneratePseudoLegalMoves(BoardState board)
        {
            List<Move> pseudoLegalMoves = new List<Move>();

            ulong whitePieces = board.WhitePawns | board.WhiteKnights | board.WhiteBishops | board.WhiteRooks | board.WhiteQueens | board.WhiteKing;
            ulong blackPieces = board.BlackPawns | board.BlackKnights | board.BlackBishops | board.BlackRooks | board.BlackQueens | board.BlackKing;

            

            ulong pawns;
            ulong knights;
            ulong bishops;
            ulong rooks;
            ulong queens;
            //ulong tanks;
            ulong king;

            if (board.WhiteToMove)
            {
                pawns = board.WhitePawns;
                knights = board.WhiteKnights;
                bishops = board.WhiteBishops;
                rooks = board.WhiteRooks;
                queens = board.WhiteQueens;
                king = board.WhiteKing;
            }
            else
            {
                pawns = board.BlackPawns;
                knights = board.BlackKnights;
                bishops = board.BlackBishops;
                rooks = board.BlackRooks;
                queens = board.BlackQueens;
                king = board.BlackKing;
            }



            // There is only one king, so we can generate its moves directly without a loop.
            pseudoLegalMoves.AddRange(GenerateKingMoves(board, board.WhiteToMove, whitePieces, blackPieces));

            while (pawns != 0)
            {
                // ulong lowestBit = pawns & (ulong)-(long)pawns; is the same as
                // ulong lowestBit = pawns & (~pawns + 1); which is the same as
                // ulong lowestBit = pawns & (pawns ^ (pawns - 1)); which is the same as
                // ulong lowestBit = pawns & (0UL - pawns); which is the same as
                // these two lines:
                // int square = BitOperations.TrailingZeroCount(pawns);
                // ulong lowestBit = 1UL << square;

                //int square = BitOperations.TrailingZeroCount(lowestBit);
                int square = BitOperations.TrailingZeroCount(pawns);
                pseudoLegalMoves.AddRange(GeneratePawnMoves(board, square, board.WhiteToMove, whitePieces, blackPieces));

                //pawns = pawns - lowestBit;
                pawns = pawns & (pawns - 1); // Clear the lowest (least significant) set bit
            }

            
            while(knights != 0)
            {
                int square = BitOperations.TrailingZeroCount(knights);
                pseudoLegalMoves.AddRange(GenerateKnightMoves(board, square, board.WhiteToMove, whitePieces, blackPieces));
                knights = knights & (knights - 1);
            }


            //(BoardState board, int square, bool whiteToMove, ulong whitePieces, ulong blackPieces)
            while(bishops != 0)
            {
                int square = BitOperations.TrailingZeroCount(bishops);
                pseudoLegalMoves.AddRange(GenerateBishopMoves(board, square, board.WhiteToMove, whitePieces, blackPieces));
                bishops = bishops & (bishops - 1);
            }

            while(rooks != 0)
            {
                int square = BitOperations.TrailingZeroCount(rooks);
                pseudoLegalMoves.AddRange(GenerateRookMoves(board, square, board.WhiteToMove, whitePieces, blackPieces));
                rooks = rooks & (rooks - 1);
            }

            while(queens != 0)
            {
                int square = BitOperations.TrailingZeroCount(queens);
                pseudoLegalMoves.AddRange(GenerateRookMoves(board, square, board.WhiteToMove, whitePieces, blackPieces));
                pseudoLegalMoves.AddRange(GenerateBishopMoves(board, square, board.WhiteToMove, whitePieces, blackPieces));
                queens = queens & (queens - 1);
            }

            return pseudoLegalMoves;
        }

        private IEnumerable<Move> GenerateKingMoves(BoardState board, bool whiteToMove, ulong whitePieces, ulong blackPieces)
        {
            List<Move> moves = new List<Move>();
            PieceType movingPiece = whiteToMove ? PieceType.WhiteKing : PieceType.BlackKing;
            ulong kingBitboard = whiteToMove ? board.WhiteKing : board.BlackKing;
            ulong ownPieces = whiteToMove ? whitePieces : blackPieces;
            bool canCastleKingSide = whiteToMove ? board.WhiteCanCastleKingSide : board.BlackCanCastleKingSide;
            bool canCastleQueenSide = whiteToMove ? board.WhiteCanCastleQueenSide : board.BlackCanCastleQueenSide;

            int[] directions = { -9, -8, -7, -1, 1, 7, 8, 9 };
            if ((kingBitboard & BitboardMasks.FileA) != 0)
                directions = directions.Where(d => d != -9 && d != -1 && d != 7).ToArray(); // Remove moves that go left
            if ((kingBitboard & BitboardMasks.FileH) != 0)
                directions = directions.Where(d => d != -7 && d !=  1 && d != 9).ToArray(); // Remove moves that go right
            if((kingBitboard & BitboardMasks.Rank1) != 0)
                directions = directions.Where(d => d != -9 && d != -8 && d != -7).ToArray(); // Remove moves that go down
            if((kingBitboard & BitboardMasks.Rank8) != 0)
                directions = directions.Where(d => d != 7 && d != 8 && d != 9).ToArray(); // Remove moves that go up


            int fromSquare = BitOperations.TrailingZeroCount(kingBitboard);
            foreach(var dir in directions)
            {
                int toSquare = fromSquare + dir;

                ulong toSquareBitboard = 1UL << toSquare;
                if ((toSquareBitboard & ownPieces) != 0)
                    continue;

                PieceType targetPiece = board.GetPieceAtSquare(toSquare);
                
                Move move = new Move
                {
                    From = fromSquare,
                    To = toSquare,
                    MovingPiece = movingPiece,
                    Type = targetPiece == PieceType.Empty ? MoveType.Normal : MoveType.Capture,
                    CapturedPiece = targetPiece
                };
                moves.Add(move);
                
            }

            int square = board.BitToSquare(kingBitboard);
            if (
                canCastleKingSide && 
                !IsSquareAttacked(byWhite: !whiteToMove, board, square) && 
                !IsSquareAttacked(byWhite: !whiteToMove, board, square + 1) && 
                !IsSquareAttacked(byWhite: !whiteToMove, board, square + 2) && 
                board.GetPieceAtSquare(square + 1) == PieceType.Empty &&
                board.GetPieceAtSquare(square + 2) == PieceType.Empty
            )
            {
                Move move = new Move
                {
                    From = fromSquare,
                    To = fromSquare + 2,
                    MovingPiece = movingPiece,
                    Type = MoveType.ShortCastle
                };
                moves.Add(move);
            }
            if (
                canCastleQueenSide &&
                !IsSquareAttacked(byWhite: !whiteToMove, board, square) &&
                !IsSquareAttacked(byWhite: !whiteToMove, board, square - 1) &&
                !IsSquareAttacked(byWhite: !whiteToMove, board, square - 2) && 
                board.GetPieceAtSquare(square - 1) == PieceType.Empty &&
                board.GetPieceAtSquare(square - 2) == PieceType.Empty &&
                board.GetPieceAtSquare(square - 3) == PieceType.Empty
            )
            {
                Move move = new Move
                {
                    From = fromSquare,
                    To = fromSquare - 2,
                    MovingPiece = movingPiece,
                    Type = MoveType.LongCastle
                };
                moves.Add(move);
            }
            return moves;
        }

        private bool IsSquareAttacked(bool byWhite, BoardState board, int square)
        {
            // Check if the square is attacked by any piece of the given color
            ulong whitePieces = board.WhitePawns | board.WhiteKnights | board.WhiteBishops | board.WhiteRooks | board.WhiteQueens | board.WhiteKing;
            ulong blackPieces = board.BlackPawns | board.BlackKnights | board.BlackBishops | board.BlackRooks | board.BlackQueens | board.BlackKing;

            if (IsAttackedByKing(byWhite, board, square))
                return true;

            if (IsAttackedByPawn(byWhite, board, square))
                return true;

            if(IsAttackedByKnight(byWhite, board, square))
                return true;

            if (IsAttackedByRookRayPiece(byWhite, board, square, whitePieces, blackPieces))
                return true;

            if(IsAttackedByBishopRayPiece(byWhite, board, square, whitePieces, blackPieces))
                return true;

            

            return false;
        }

        //private bool IsAttackedByPawn_old(bool byWhite, BoardState board, int square)
        //{
        //    int rank = square / 8;
        //    int file = square % 8;
        //    ulong squareBitboard = 1UL << square;

        //    if (byWhite)
        //    {
        //        if (rank > 1)
        //        {
        //            if (file > 0 && (board.WhitePawns & (squareBitboard >> 9)) != 0) return true;
        //            if (file < 7 && (board.WhitePawns & (squareBitboard >> 7)) != 0) return true;
        //        }
        //    }
        //    else
        //    {
        //        if (rank < 6)
        //        {
        //            if (file > 0 && (board.BlackPawns & (squareBitboard << 7)) != 0) return true;
        //            if (file < 7 && (board.BlackPawns & (squareBitboard << 9)) != 0) return true;
        //        }
        //    }

        //    return false;
        //}

        private bool IsAttackedByPawn(bool byWhite, BoardState board, int square)
        {
            ulong squareBitboard = 1UL << square;

            if (byWhite)
            {
                ulong pawnsThatCanAttackTowardsA = ~BitboardMasks.FileA & board.WhitePawns;
                ulong pawnsThatCanAttackTowardsH = ~BitboardMasks.FileH & board.WhitePawns;
                ulong canAttackTowardsA = (squareBitboard & (pawnsThatCanAttackTowardsA << 7));
                ulong canAttackTowardsH = (squareBitboard & (pawnsThatCanAttackTowardsH << 9));

                return (canAttackTowardsA | canAttackTowardsH) != 0;
            }
            else
            {
                ulong pawnsThatCanAttackTowardsA = ~BitboardMasks.FileA & board.BlackPawns;
                ulong pawnsThatCanAttackTowardsH = ~BitboardMasks.FileH & board.BlackPawns;
                ulong canAttackTowardsA = (squareBitboard & (pawnsThatCanAttackTowardsA >> 9));
                ulong canAttackTowardsH = (squareBitboard & (pawnsThatCanAttackTowardsH >> 7));

                return (canAttackTowardsA | canAttackTowardsH) != 0;
            }
        }

        /* private bool IsAttackedByKnight_OldVersion(bool byWhite, BoardState board, int square)
        {
            int rank = square / 8;
            int file = square % 8;
            ulong squareBitboard = 1UL << square;
            ulong knightBitboard = byWhite ? board.WhiteKnights : board.BlackKnights;

            int[] directions = { -17, -15, -10, -6, 6, 10, 15, 17 };

            if (rank == 0)
                directions = directions.Where(d => d > 0).ToArray();
            if (rank == 1)
                directions = directions.Where(d => d != -17 && d != -15).ToArray();
            if (rank == 6)
                directions = directions.Where(d => d != 15 && d != 17).ToArray();
            if (rank == 7)
                directions = directions.Where(d => d < 0).ToArray();
            

            if(file == 0)
                directions = directions.Where(d => d != -17 && d != -10 && d != 6 && d != 15).ToArray();
            if(file == 1)
                directions = directions.Where(d => d != -10 && d !=  6).ToArray();
            if(file == 6)
                directions = directions.Where(d => d !=  10 && d != -6).ToArray();
            if(file == 7)
                directions = directions.Where(d => d != -15 && d != -6 && d != 10 && d != 17).ToArray();

            //ulong knightMask = 0;
            foreach (var dir in directions)
            {
                int targetSquare = square + dir;
                ulong targetBitboard = 1UL << targetSquare;
                if ((knightBitboard & targetBitboard) != 0)
                    return true;
            }

            return false;
        }
        */

        private bool IsAttackedByKnight(bool byWhite, BoardState board, int square)
        {
            ulong knightBitboard = byWhite ? board.WhiteKnights : board.BlackKnights;
            ulong knightMoves = BitboardMasks.GetKnightMasks(square);

            return ((knightBitboard & knightMoves) != 0);
        }

        private bool IsAttackedByKing(bool byWhite, BoardState board, int square)
        {
            ulong kingBitboard = byWhite ? board.WhiteKing : board.BlackKing;
            ulong kingMoves = BitboardMasks.GetKingMasks(square);
            return ((kingBitboard & kingMoves) != 0);
        }

        private bool IsAttackedByRookRayPiece(bool byWhite, BoardState board, int square, ulong whitePieces, ulong blackPieces)
        {
            ulong rookBitboard = byWhite ? board.WhiteRooks : board.BlackRooks;
            ulong queenBitboard = byWhite ? board.WhiteQueens : board.BlackQueens;

            ulong allPieces = whitePieces | blackPieces;

            ulong rqBitboard = rookBitboard | queenBitboard;

            ulong rookRays = BitboardMasks.GetRookRayMasks(square);
            ulong rookNorthRays = BitboardMasks.GetRookNorthMasks(square);
            ulong rookSouthRays = BitboardMasks.GetRookSouthMasks(square);
            ulong rookEastRays = BitboardMasks.GetRookEastMasks(square);
            ulong rookWestRays = BitboardMasks.GetRookWestMasks(square);

            if((rookRays & rqBitboard) == 0)
                return false;

            ulong northRookQueen = rookNorthRays & rqBitboard;
            ulong southRookQueen = rookSouthRays & rqBitboard;
            ulong westRookQueen  = rookWestRays  & rqBitboard;
            ulong eastRookQueen  = rookEastRays  & rqBitboard;

            ulong northPieces = rookNorthRays & allPieces;
            ulong southPieces = rookSouthRays & allPieces;
            ulong westPieces  = rookWestRays  & allPieces;
            ulong eastPieces  = rookEastRays  & allPieces;

            if(northRookQueen != 0)
            {
                int firstNorthRookQueenSquare = BitOperations.TrailingZeroCount(northRookQueen);
                int firstNorthPieceSquare = BitOperations.TrailingZeroCount(northPieces);
                if (firstNorthRookQueenSquare == firstNorthPieceSquare)
                    return true;
            }

            if (southRookQueen != 0)
            {
                int firstSouthRookQueenSquare = 63 - BitOperations.LeadingZeroCount(southRookQueen);
                int firstSouthPieceSquare = 63 - BitOperations.LeadingZeroCount(southPieces);
                if (firstSouthRookQueenSquare == firstSouthPieceSquare)
                    return true;
            }

            if(westRookQueen != 0)
            {
                int firstWestRookQueenSquare = 63 - BitOperations.LeadingZeroCount(westRookQueen);
                int firstWestPieceSquare = 63 - BitOperations.LeadingZeroCount(westPieces);
                if (firstWestRookQueenSquare == firstWestPieceSquare)
                    return true;
            }

            if(eastRookQueen != 0)
            {
                int firstEastRookQueenSquare = BitOperations.TrailingZeroCount(eastRookQueen);
                int firstEastPieceSquare = BitOperations.TrailingZeroCount(eastPieces);
                if (firstEastRookQueenSquare == firstEastPieceSquare)
                    return true;
            }

            return false;
        }

        private bool IsAttackedByBishopRayPiece(bool byWhite, BoardState board, int square, ulong whitePieces, ulong blackPieces)
        {
            ulong bishopBitboard = byWhite ? board.WhiteBishops : board.BlackBishops;
            ulong queenBitboard = byWhite ? board.WhiteQueens : board.BlackQueens;

            ulong allPieces = whitePieces | blackPieces;

            ulong bqBitboard = bishopBitboard | queenBitboard;

            ulong bishopRays      = BitboardMasks.GetBishopRayMasks(square);
            if ((bishopRays & bqBitboard) == 0)
                return false;


            ulong bishopNorthWestRays = BitboardMasks.GetBishopNorthWestMasks(square);
            ulong bishopNorthEastRays = BitboardMasks.GetBishopNorthEastMasks(square);
            ulong bishopSouthWestRays = BitboardMasks.GetBishopSouthWestMasks(square);
            ulong bishopSouthEastRays = BitboardMasks.GetBishopSouthEastMasks(square);


            ulong northWestBishopQueen = bishopNorthWestRays & bqBitboard;
            ulong northEastBishopQueen = bishopNorthEastRays & bqBitboard;
            ulong southWestBishopQueen = bishopSouthWestRays & bqBitboard;
            ulong southEastBishopQueen = bishopSouthEastRays & bqBitboard;

            ulong northWestPieces = bishopNorthWestRays & allPieces;
            ulong northEastPieces = bishopNorthEastRays & allPieces;
            ulong southWestPieces = bishopSouthWestRays & allPieces;
            ulong southEastPieces = bishopSouthEastRays & allPieces;

            if (northWestBishopQueen != 0)
            {
                int firstNorthWestBishopQueenSquare = BitOperations.TrailingZeroCount(northWestBishopQueen);
                int firstNorthWestPieceSquare = BitOperations.TrailingZeroCount(northWestPieces);
                if (firstNorthWestBishopQueenSquare == firstNorthWestPieceSquare)
                    return true;
            }

            if (northEastBishopQueen != 0)
            {
                int firstNorthEastBishopQueenSquare = BitOperations.TrailingZeroCount(northEastBishopQueen);
                int firstNorthEastPieceSquare = BitOperations.TrailingZeroCount(northEastPieces);
                if (firstNorthEastBishopQueenSquare == firstNorthEastPieceSquare)
                    return true;
            }

            if (southWestBishopQueen != 0)
            {
                int firstSouthWestBishopQueenSquare = 63 - BitOperations.LeadingZeroCount(southWestBishopQueen);
                int firstSouthWestPieceSquare       = 63 - BitOperations.LeadingZeroCount(southWestPieces);
                if (firstSouthWestBishopQueenSquare == firstSouthWestPieceSquare)
                    return true;
            }

            if (southEastBishopQueen != 0)
            {
                int firstSouthEastBishopQueenSquare = 63 - BitOperations.LeadingZeroCount(southEastBishopQueen);
                int firstSouthEastPieceSquare       = 63 - BitOperations.LeadingZeroCount(southEastPieces);
                if (firstSouthEastBishopQueenSquare == firstSouthEastPieceSquare)
                    return true;
            }

            return false;
        }

        //private bool IsAttackedByRookRay(bool byWhite, BoardState board, int square)
        //{
        //    ulong rookBitboard = byWhite ? board.WhiteRooks : board.BlackRooks;
        //    ulong queenBitboard = byWhite ? board.WhiteQueens : board.BlackQueens;
        //    PieceType rook = byWhite ? PieceType.WhiteRook : PieceType.BlackRook;
        //    PieceType queen = byWhite ? PieceType.WhiteQueen : PieceType.BlackQueen;

        //    ulong rookMoves = BitboardMasks.GetRookRayMasks(square);
        //    bool rookAimAtSquare = ((rookBitboard & rookMoves) != 0);
        //    bool queenAimAtSquare = ((queenBitboard & rookMoves) != 0);

        //    //int rank = square / 8;
        //    //int file = square % 8;
        //    if (rookAimAtSquare || queenAimAtSquare)
        //    {
        //        for (int i = square - 8; i >= 0; i = i - 8)
        //        {
        //            PieceType piece = board.GetPieceAtSquare(i);
        //            if (piece == PieceType.Empty) continue;
        //            if (piece == rook || piece == queen) return true;
        //            else break;
        //        }

        //        for (int i = square + 8; i <= 63; i = i + 8)
        //        {
        //            PieceType piece = board.GetPieceAtSquare(i);
        //            if (piece == PieceType.Empty) continue;
        //            if (piece == rook || piece == queen) return true;
        //            else break;
        //        }

        //        for(int i = 1; (square % 8) - i >= 0; i++)
        //        {
        //            PieceType piece = board.GetPieceAtSquare(square - i);
        //            if (piece == PieceType.Empty) continue;
        //            if (piece == rook || piece == queen) return true;
        //            else break;
        //        }

        //        for(int i = 1; (square % 8) + i <= 7; i++)
        //        {
        //            PieceType piece = board.GetPieceAtSquare(square + i);
        //            if (piece == PieceType.Empty) continue;
        //            if (piece == rook || piece == queen) return true;
        //            else break;
        //        }

        //    }

        //    return false;
        //}

        //private bool IsAttackedByBishopRay(bool byWhite, BoardState board, int square)
        //{
        //    ulong bishopBitboard = byWhite ? board.WhiteBishops : board.BlackBishops;
        //    ulong queenBitboard = byWhite ? board.WhiteQueens : board.BlackQueens;
        //    PieceType bishop = byWhite ? PieceType.WhiteBishop : PieceType.BlackBishop;
        //    PieceType queen = byWhite ? PieceType.WhiteQueen : PieceType.BlackQueen;

        //    ulong bishopMoves = BitboardMasks.GetBishopRayMasks(square);

        //    bool bishopAimAtSquare = ((bishopBitboard & bishopMoves) != 0);
        //    bool queenAimAtSquare = ((queenBitboard & bishopMoves) != 0);

        //    if (bishopAimAtSquare || queenAimAtSquare)
        //    {
        //        int rank = square / 8;
        //        int file = square % 8;
        //        for (int i = 1; rank - i >= 0 && file - i >= 0; i++)
        //        {
        //            PieceType piece = board.GetPieceAtSquare(square - 9*i);
        //            if (piece == PieceType.Empty) continue;
        //            if (piece == bishop || piece == queen) return true;
        //            else break;
        //        }

        //        for (int i = 1; rank - i >= 0 && file + i < 8; i++)
        //        {
        //            PieceType piece = board.GetPieceAtSquare(square - 7*i);
        //            if (piece == PieceType.Empty) continue;
        //            if (piece == bishop || piece == queen) return true;
        //            else break;
        //        }

        //        for (int i = 1; rank + i < 8 && file - i >= 0; i++)
        //        {
        //            PieceType piece = board.GetPieceAtSquare(square + 7*i);
        //            if (piece == PieceType.Empty) continue;
        //            if (piece == bishop || piece == queen) return true;
        //            else break;
        //        }

        //        for (int i = 1; rank + i < 8 && file + i < 8; i++)
        //        {
        //            PieceType piece = board.GetPieceAtSquare(square + 9*i);
        //            if (piece == PieceType.Empty) continue;
        //            if (piece == bishop || piece == queen) return true;
        //            else break;
        //        }

        //    }

        //    return false;
        //}


        private bool IsPieceForSideToMove(PieceType piece, bool whiteToMove)
        {
            if (piece == PieceType.Empty)
                return false;

            bool pieceIsWhite = piece > 0;

            return pieceIsWhite == whiteToMove;
        }


        // I should make a new version of this funktion that doesn't take an int for a square as
        // input, but instead takes the bitboard for all pawns as input and generates all pawn
        // moves at once.
        private List<Move> GeneratePawnMoves(BoardState board, int i, bool whiteToMove, ulong whitePieces, ulong blackPieces)
        {
            List<Move> pawnMoves = new List<Move>();
            ulong square = 1UL << i;
            ulong occupied = whitePieces | blackPieces;
            ulong empty = ~occupied;
            bool checkForDoublePush = false;
            PieceType movingPiece = whiteToMove
                ? PieceType.WhitePawn
                : PieceType.BlackPawn;

            if (whiteToMove)
            {
                int north = 8;
                if ((square << north & empty) != 0)
                {
                    if ((square & BitboardMasks.WhitePrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i + north, MovingPiece = movingPiece, Type = MoveType.Promotion, PromotionPiece = PieceType.WhiteQueen });
                        pawnMoves.Add(new Move { From = i, To = i + north, MovingPiece = movingPiece, Type = MoveType.Promotion, PromotionPiece = PieceType.WhiteRook });
                        pawnMoves.Add(new Move { From = i, To = i + north, MovingPiece = movingPiece, Type = MoveType.Promotion, PromotionPiece = PieceType.WhiteBishop });
                        pawnMoves.Add(new Move { From = i, To = i + north, MovingPiece = movingPiece, Type = MoveType.Promotion, PromotionPiece = PieceType.WhiteKnight });

                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i + north, MovingPiece = movingPiece, Type = MoveType.Normal });
                        checkForDoublePush = true;
                    }
                }

                if(checkForDoublePush && (square & BitboardMasks.WhitePawnStart) != 0 && (square << (2 * north) & empty) != 0)
                {
                    pawnMoves.Add(new Move { From = i, To = i + (2 * north), MovingPiece = movingPiece, Type = MoveType.Normal });
                }

                // Capture moves
                int northWest = 7;
                int northEast = 9;
                PieceType capturedPiece = PieceType.Empty;

                if ((square & ~BitboardMasks.FileA) != 0 && (square << northWest & blackPieces) != 0)
                {
                    capturedPiece = board.GetPieceAtSquare(i + northWest);

                    if ((square & BitboardMasks.WhitePrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i + northWest, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteQueen , CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northWest, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteRook  , CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northWest, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteBishop, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northWest, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteKnight, CapturedPiece = capturedPiece });
                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i + northWest, MovingPiece = movingPiece, Type = MoveType.Capture, CapturedPiece = capturedPiece });
                    }
                }

                if((square & ~BitboardMasks.FileH) != 0 && (square << northEast & blackPieces) != 0)
                {
                    capturedPiece = board.GetPieceAtSquare(i + northEast);

                    if ((square & BitboardMasks.WhitePrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i + northEast, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteQueen , CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northEast, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteRook  , CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northEast, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteBishop, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northEast, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteKnight, CapturedPiece = capturedPiece });
                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i + northEast, MovingPiece = movingPiece, Type = MoveType.Capture, CapturedPiece = capturedPiece });
                    }
                }

                if(board.EnPassantSquare != -1)
                {
                    int enPassantSquare = board.EnPassantSquare;
                    if ((square & ~BitboardMasks.FileA) != 0 && (i + northWest == enPassantSquare))
                    {
                        pawnMoves.Add(new Move { From = i, To = enPassantSquare, MovingPiece = movingPiece, Type = MoveType.EnPassant, CapturedPiece = PieceType.BlackPawn });
                    }
                    if ((square & ~BitboardMasks.FileH) != 0 && (i + northEast == enPassantSquare))
                    {
                        pawnMoves.Add(new Move { From = i, To = enPassantSquare, MovingPiece = movingPiece, Type = MoveType.EnPassant, CapturedPiece = PieceType.BlackPawn });
                    }
                }
            }



            if (!whiteToMove)
            {
                int south = 8;
                if ((square >> south & empty) != 0)
                {
                    if ((square & BitboardMasks.BlackPrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i - south, MovingPiece = movingPiece, Type = MoveType.Promotion, PromotionPiece = PieceType.BlackQueen });
                        pawnMoves.Add(new Move { From = i, To = i - south, MovingPiece = movingPiece, Type = MoveType.Promotion, PromotionPiece = PieceType.BlackRook });
                        pawnMoves.Add(new Move { From = i, To = i - south, MovingPiece = movingPiece, Type = MoveType.Promotion, PromotionPiece = PieceType.BlackBishop });
                        pawnMoves.Add(new Move { From = i, To = i - south, MovingPiece = movingPiece, Type = MoveType.Promotion, PromotionPiece = PieceType.BlackKnight });

                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i - south, MovingPiece = movingPiece, Type = MoveType.Normal });
                        checkForDoublePush = true;
                    }
                }

                if (checkForDoublePush && (square & BitboardMasks.BlackPawnStart) != 0 && (square >> (2 * south) & empty) != 0)
                {
                    pawnMoves.Add(new Move { From = i, To = i - (2 * south), MovingPiece = movingPiece, Type = MoveType.Normal });
                }

                // Capture moves
                int southEast = 7;
                int southWest = 9;
                PieceType capturedPiece = PieceType.Empty;

                if ((square & ~BitboardMasks.FileA) != 0 && (square >> southWest & whitePieces) != 0)
                {
                    capturedPiece = board.GetPieceAtSquare(i - southWest);

                    if ((square & BitboardMasks.BlackPrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i - southWest, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackQueen, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southWest, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackRook, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southWest, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackBishop, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southWest, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackKnight, CapturedPiece = capturedPiece });
                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i - southWest, MovingPiece = movingPiece, Type = MoveType.Capture, CapturedPiece = capturedPiece });
                    }
                }

                if ((square & ~BitboardMasks.FileH) != 0 && (square >> southEast & whitePieces) != 0)
                {
                    capturedPiece = board.GetPieceAtSquare(i - southEast);

                    if ((square & BitboardMasks.BlackPrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i - southEast, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackQueen, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southEast, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackRook, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southEast, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackBishop, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southEast, MovingPiece = movingPiece, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackKnight, CapturedPiece = capturedPiece });
                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i - southEast, MovingPiece = movingPiece, Type = MoveType.Capture, CapturedPiece = capturedPiece });
                    }
                }

                if(board.EnPassantSquare != -1)
                {
                    int enPassantSquare = board.EnPassantSquare;
                    if ((square & ~BitboardMasks.FileA) != 0 && (i - southWest == enPassantSquare))
                    {
                        pawnMoves.Add(new Move { From = i, To = enPassantSquare, MovingPiece = movingPiece, Type = MoveType.EnPassant, CapturedPiece = PieceType.WhitePawn });
                    }
                    if ((square & ~BitboardMasks.FileH) != 0 && (i - southEast == enPassantSquare))
                    {
                        pawnMoves.Add(new Move { From = i, To = enPassantSquare, MovingPiece = movingPiece, Type = MoveType.EnPassant, CapturedPiece = PieceType.WhitePawn });
                    }
                }
            }

            return pawnMoves;
        }

        private List<Move> GenerateKnightMoves(BoardState board, int square, bool whiteToMove, ulong whitePieces, ulong blackPieces)
        {
            List<Move> moves = new List<Move>();
            ulong ownPieces = whiteToMove ? whitePieces : blackPieces;

            ulong knightmoves = BitboardMasks.GetKnightMasks(square);

            PieceType movingPiece = whiteToMove
                ? PieceType.WhiteKnight
                : PieceType.BlackKnight;

            knightmoves &= ~ownPieces; // Remove squares occupied by own pieces

            while (knightmoves != 0)
            {
                int toSquare = BitOperations.TrailingZeroCount(knightmoves);
                
                PieceType capturedPiece = board.GetPieceAtSquare(toSquare);
                moves.Add(new Move
                {
                    From = square,
                    To = toSquare,
                    MovingPiece = movingPiece,
                    Type = capturedPiece == PieceType.Empty ? MoveType.Normal : MoveType.Capture,
                    CapturedPiece = capturedPiece
                });
                knightmoves = knightmoves & (knightmoves - 1);
            }

            return moves;
        }

        private List<Move> GenerateBishopMoves(BoardState board, int square, bool whiteToMove, ulong whitePieces, ulong blackPieces)
        {
            List<Move> moves = new List<Move>();
            ulong ownPieces = whiteToMove ? whitePieces : blackPieces;
            //ulong opponentsPieces = whiteToMove ? blackPieces : whitePieces;

            PieceType movingPiece = board.GetPieceAtSquare(square);

            ulong allPieces = whitePieces | blackPieces;

            ulong bishopNorthWestMoves = BitboardMasks.GetBishopNorthWestMasks(square);
            ulong bishopNorthEastMoves = BitboardMasks.GetBishopNorthEastMasks(square);
            ulong bishopSouthWestMoves = BitboardMasks.GetBishopSouthWestMasks(square);
            ulong bishopSouthEastMoves = BitboardMasks.GetBishopSouthEastMasks(square);

            { 
                ulong blockingPieces = bishopNorthWestMoves & allPieces;
                // If blockingPieces == 0, then TrailingZeroCount(blockingPieces) == 64.
                if (blockingPieces != 0)
                {
                    int firstBlockingPieceSquare = BitOperations.TrailingZeroCount(blockingPieces);
                    ulong blockedSquares = BitboardMasks.GetBishopNorthWestMasks(firstBlockingPieceSquare);  // direction specific.
                    bishopNorthWestMoves = bishopNorthWestMoves & ~blockedSquares;
                }
                bishopNorthWestMoves = bishopNorthWestMoves & ~ownPieces;
            }


            {
                ulong blockingPieces = bishopNorthEastMoves & allPieces;
                if (blockingPieces != 0)
                {
                    int firstBlockingPieceSquare = BitOperations.TrailingZeroCount(blockingPieces);
                    ulong blockedSquares = BitboardMasks.GetBishopNorthEastMasks(firstBlockingPieceSquare);  // direction specific.
                    bishopNorthEastMoves = bishopNorthEastMoves & ~blockedSquares;
                }
                bishopNorthEastMoves = bishopNorthEastMoves & ~ownPieces;
            }

            {
                ulong blockingPieces = bishopSouthWestMoves & allPieces;
                if (blockingPieces != 0)
                {
                    int firstBlockingPieceSquare = 63 - BitOperations.LeadingZeroCount(blockingPieces);
                    ulong blockedSquares = BitboardMasks.GetBishopSouthWestMasks(firstBlockingPieceSquare);  // direction specific.
                    bishopSouthWestMoves = bishopSouthWestMoves & ~blockedSquares;
                }
                bishopSouthWestMoves = bishopSouthWestMoves & ~ownPieces;

            }
            {
                ulong blockingPieces = bishopSouthEastMoves & allPieces;
                if (blockingPieces != 0)
                {
                    int firstBlockingPieceSquare = 63 - BitOperations.LeadingZeroCount(blockingPieces);
                    ulong blockedSquares = BitboardMasks.GetBishopSouthEastMasks(firstBlockingPieceSquare);  // direction specific.
                    bishopSouthEastMoves = bishopSouthEastMoves & ~blockedSquares;
                }
                bishopSouthEastMoves = bishopSouthEastMoves & ~ownPieces;
            }

            // Earlier attempt to remove blocked squares. This code will not be used, but it might
            // be interesting to compare my original approach to the one I ended up using.
            //
            //if (true)
            //{
            //    ulong ownBlockingPieces = bishopNorthWestMoves & ~ownPieces;
            //    int firstOwnBlockingPieceSquare = BitOperations.TrailingZeroCount(ownBlockingPieces);
            //    ulong ownBlockedSquares = BitboardMasks.GetBishopNorthWestMasks(firstOwnBlockingPieceSquare);

            //    ulong opponentsBlockingPieces = bishopNorthWestMoves & opponentsPieces;
            //    int firstOpponentBlockingPieceSquare = BitOperations.TrailingZeroCount(opponentsBlockingPieces);

            //    ulong opponentBlockedSquares = 0;
            //    if (firstOpponentBlockingPieceSquare + 7 < 64)
            //        opponentBlockedSquares = BitboardMasks.GetBishopNorthWestMasks(firstOpponentBlockingPieceSquare + 7);

            //    ulong movesToRemove = ownBlockedSquares | opponentBlockedSquares;

            //    bishopNorthWestMoves = bishopNorthWestMoves & ~movesToRemove;

            //}


            ulong bishopMoves = bishopNorthWestMoves | bishopNorthEastMoves | bishopSouthWestMoves | bishopSouthEastMoves;

            while(bishopMoves != 0) { 
                int toSquare = BitOperations.TrailingZeroCount(bishopMoves);
                PieceType pieceAtToSquare = board.GetPieceAtSquare(toSquare);
                moves.Add(new Move
                {
                    From = square,
                    To = toSquare,
                    MovingPiece = movingPiece,
                    Type = pieceAtToSquare == PieceType.Empty ? MoveType.Normal : MoveType.Capture,
                    CapturedPiece = pieceAtToSquare
                });
                bishopMoves = bishopMoves & (bishopMoves - 1);
            }

            
            return moves;
        }


        private List<Move> GenerateRookMoves(BoardState board, int square, bool whiteToMove, ulong whitePieces, ulong blackPieces)
        {
            List<Move> moves = new List<Move>();
            ulong ownPieces = whiteToMove ? whitePieces : blackPieces;
            ulong allPieces = whitePieces | blackPieces;
            PieceType movingPiece = board.GetPieceAtSquare(square);

            ulong rookNorthMoves = BitboardMasks.GetRookNorthMasks(square);
            ulong rookSouthMoves = BitboardMasks.GetRookSouthMasks(square);
            ulong rookWestMoves = BitboardMasks.GetRookWestMasks(square);
            ulong rookEastMoves = BitboardMasks.GetRookEastMasks(square);
            {
                ulong blockingPieces = rookNorthMoves & allPieces;
                if (blockingPieces != 0)
                {
                    int firstBlockingPieceSquare = BitOperations.TrailingZeroCount(blockingPieces);
                    ulong blockedSquares = BitboardMasks.GetRookNorthMasks(firstBlockingPieceSquare);
                    rookNorthMoves = rookNorthMoves & ~blockedSquares;
                }
                rookNorthMoves = rookNorthMoves & ~ownPieces;
            }

            {
                ulong blockingPieces = rookSouthMoves & allPieces;
                if (blockingPieces != 0)
                {
                    int firstBlockingPieceSquare = 63 - BitOperations.LeadingZeroCount(blockingPieces);
                    ulong blockedSquares = BitboardMasks.GetRookSouthMasks(firstBlockingPieceSquare);
                    rookSouthMoves = rookSouthMoves & ~blockedSquares;
                }
                rookSouthMoves = rookSouthMoves & ~ownPieces;

            }

            {
                ulong blockingPieces = rookWestMoves & allPieces;
                if (blockingPieces != 0)
                {
                    int firstBlockingPieceSquare = 63 - BitOperations.LeadingZeroCount(blockingPieces);
                    ulong blockedSquares = BitboardMasks.GetRookWestMasks(firstBlockingPieceSquare);
                    rookWestMoves = rookWestMoves & ~blockedSquares;
                }
                rookWestMoves = rookWestMoves & ~ownPieces;
            }

            {
                ulong blockingPieces = rookEastMoves & allPieces;
                if (blockingPieces != 0)
                {
                    int firstBlockingPieceSquare = BitOperations.TrailingZeroCount(blockingPieces);
                    ulong blockedSquares = BitboardMasks.GetRookEastMasks(firstBlockingPieceSquare);
                    rookEastMoves = rookEastMoves & ~blockedSquares;
                }
                rookEastMoves = rookEastMoves & ~ownPieces;
            }

            ulong rookMoves = rookNorthMoves | rookSouthMoves | rookWestMoves | rookEastMoves;

            while (rookMoves != 0)
            {
                int toSquare = BitOperations.TrailingZeroCount(rookMoves);
                PieceType pieceAtToSquare = board.GetPieceAtSquare(toSquare);
                moves.Add(new Move
                {
                    From = square,
                    To = toSquare,
                    MovingPiece = movingPiece,
                    Type = pieceAtToSquare == PieceType.Empty ? MoveType.Normal : MoveType.Capture,
                    CapturedPiece = pieceAtToSquare
                });

                rookMoves = rookMoves & (rookMoves - 1);
            }

            return moves;
        }

    }
}
