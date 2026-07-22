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

                int ownColorSign = movingSideIsWhite ? 1 : -1;

                if (!IsKingInCheck(nextPosition, ownColorSign))
                {
                    legalMoves.Add(move);
                }
            }

            return legalMoves;
        }


        private List<Move> GeneratePseudoLegalMoves(BoardState board)
        {
            List<Move> pseudoLegalMoves = new List<Move>();

            ulong whitePieces = board.WhitePawns | board.WhiteKnights | board.WhiteBishops | board.WhiteRooks | board.WhiteQueens | board.WhiteKing;
            ulong blackPieces = board.BlackPawns | board.BlackKnights | board.BlackBishops | board.BlackRooks | board.BlackQueens | board.BlackKing;

            // I change this loop, since it is faster to check the bitboard for each piece type for
            // only as many iterations as there are pieces of that type, rather than checking all
            // 64 squares for each piece type.
            for (int i = 0; i < 64; i++)
            {
                PieceType piece = board.GetPieceAtSquare(i);

                if (!IsPieceForSideToMove(piece, board.WhiteToMove))
                    continue;

                bool pieceIsWhite = piece > 0;

                


                switch (Math.Abs((int)piece))
                {
                    case 1: // Pawn
                        pseudoLegalMoves.AddRange(GeneratePawnMoves(board, i, pieceIsWhite, whitePieces, blackPieces));
                        break;
                    case 2: // Knight
                        pseudoLegalMoves.AddRange(GenerateKnightMoves(board, i, pieceIsWhite));
                        break;
                    case 3: // Bishop
                        pseudoLegalMoves.AddRange(BishopMoveGenerator.GenerateBishopMoves(board, i, pieceIsWhite));
                        break;
                    case 4: // Rook
                        pseudoLegalMoves.AddRange(RookMoveGenerator.GenerateRookMoves(board, i, pieceIsWhite));
                        break;
                    case 5: // Queen
                        pseudoLegalMoves.AddRange(QueenMoveGenerator.GenerateQueenMoves(board, i, pieceIsWhite));
                        break;
                    case 6: // Tank
                        pseudoLegalMoves.AddRange(TankMoveGenerator.GenerateTankMoves(board, i, pieceIsWhite));
                        break;
                    case 7: // King
                        pseudoLegalMoves.AddRange(KingMoveGenerator.GenerateKingMoves(board, i, pieceIsWhite));
                        break;
                }
            }

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

            pseudoLegalMoves.AddRange(GenerateKingMoves(board, board.WhiteToMove, whitePieces, blackPieces));
            


            return pseudoLegalMoves;
        }

        private IEnumerable<Move> GenerateKingMoves(BoardState board, bool whiteToMove, ulong whitePieces, ulong blackPieces)
        {
            List<Move> moves = new List<Move>();
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
                    Type = targetPiece == PieceType.Empty ? MoveType.Normal : MoveType.Capture,
                    CapturedPiece = targetPiece
                };
                moves.Add(move);
                
            }

            if (canCastleKingSide)
            {

            }
            if (canCastleQueenSide)
            {

            }
            return moves;
        }

        private bool IsSquareAttacked(bool byWhite, BoardState board, int square)
        {
            // Check if the square is attacked by any piece of the given color
            ulong whitePieces = board.WhitePawns | board.WhiteKnights | board.WhiteBishops | board.WhiteRooks | board.WhiteQueens | board.WhiteKing;
            ulong blackPieces = board.BlackPawns | board.BlackKnights | board.BlackBishops | board.BlackRooks | board.BlackQueens | board.BlackKing;
            
            if(IsAttackedByPawn(byWhite, board, square))
                return true;

            return false; // Placeholder return value; replace with actual attack detection logic
        }

        private bool IsAttackedByPawn(bool byWhite, BoardState board, int square)
        {
            int rank = square / 8;
            int file = square % 8;
            ulong squareBitboard = 1UL << square;

            if (byWhite)
            {
                if (rank > 1)
                {
                    if (file > 0 && (board.WhitePawns & (squareBitboard << 7)) != 0) return true;
                    if (file < 7 && (board.WhitePawns & (squareBitboard << 9)) != 0) return true;
                }
            }
            else
            {
                if (rank < 6)
                {
                    if (file > 0 && (board.WhitePawns & (squareBitboard >> 9)) != 0) return true;
                    if (file < 7 && (board.WhitePawns & (squareBitboard >> 7)) != 0) return true;
                }
            }

            return false;
        }


        private bool IsPieceForSideToMove(PieceType piece, bool whiteToMove)
        {
            if (piece == PieceType.Empty)
                return false;

            bool pieceIsWhite = piece > 0;

            return pieceIsWhite == whiteToMove;
        }


        private List<Move> GeneratePawnMoves(BoardState board, int i, bool pieceIsWhite, ulong whitePieces, ulong blackPieces)
        {
            List<Move> pawnMoves = new List<Move>();
            ulong square = 1UL << i;
            ulong occupied = whitePieces | blackPieces;
            ulong empty = ~occupied;
            bool checkForDoublePush = false;

            if (pieceIsWhite)
            {
                int north = 8;
                if ((square << north & empty) != 0)
                {
                    if ((square & BitboardMasks.WhitePrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i + north, Type = MoveType.Promotion, PromotionPiece = PieceType.WhiteQueen });
                        pawnMoves.Add(new Move { From = i, To = i + north, Type = MoveType.Promotion, PromotionPiece = PieceType.WhiteRook });
                        pawnMoves.Add(new Move { From = i, To = i + north, Type = MoveType.Promotion, PromotionPiece = PieceType.WhiteBishop });
                        pawnMoves.Add(new Move { From = i, To = i + north, Type = MoveType.Promotion, PromotionPiece = PieceType.WhiteKnight });

                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i + north, Type = MoveType.Normal });
                        checkForDoublePush = true;
                    }
                }

                if(checkForDoublePush && (square & BitboardMasks.WhitePawnStart) != 0 && (square << (2 * north) & empty) != 0)
                {
                    pawnMoves.Add(new Move { From = i, To = i + (2 * north), Type = MoveType.Normal });
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
                        pawnMoves.Add(new Move { From = i, To = i + northWest, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteQueen , CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northWest, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteRook  , CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northWest, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteBishop, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northWest, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteKnight, CapturedPiece = capturedPiece });
                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i + northWest, Type = MoveType.Capture, CapturedPiece = capturedPiece });
                    }
                }

                if((square & ~BitboardMasks.FileH) != 0 && (square << northEast & blackPieces) != 0)
                {
                    capturedPiece = board.GetPieceAtSquare(i + northEast);

                    if ((square & BitboardMasks.WhitePrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i + northEast, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteQueen , CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northEast, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteRook  , CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northEast, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteBishop, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i + northEast, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.WhiteKnight, CapturedPiece = capturedPiece });
                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i + northEast, Type = MoveType.Capture, CapturedPiece = capturedPiece });
                    }
                }

                if(board.EnPassantSquare != -1)
                {
                    int enPassantSquare = board.EnPassantSquare;
                    if ((square & ~BitboardMasks.FileA) != 0 && (i + northWest == enPassantSquare))
                    {
                        pawnMoves.Add(new Move { From = i, To = enPassantSquare, Type = MoveType.EnPassant, CapturedPiece = PieceType.BlackPawn });
                    }
                    if ((square & ~BitboardMasks.FileH) != 0 && (i + northEast == enPassantSquare))
                    {
                        pawnMoves.Add(new Move { From = i, To = enPassantSquare, Type = MoveType.EnPassant, CapturedPiece = PieceType.BlackPawn });
                    }
                }
            }



            if (!pieceIsWhite)
            {
                int south = 8;
                if ((square >> south & empty) != 0)
                {
                    if ((square & BitboardMasks.BlackPrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i - south, Type = MoveType.Promotion, PromotionPiece = PieceType.BlackQueen });
                        pawnMoves.Add(new Move { From = i, To = i - south, Type = MoveType.Promotion, PromotionPiece = PieceType.BlackRook });
                        pawnMoves.Add(new Move { From = i, To = i - south, Type = MoveType.Promotion, PromotionPiece = PieceType.BlackBishop });
                        pawnMoves.Add(new Move { From = i, To = i - south, Type = MoveType.Promotion, PromotionPiece = PieceType.BlackKnight });

                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i - south, Type = MoveType.Normal });
                        checkForDoublePush = true;
                    }
                }

                if (checkForDoublePush && (square & BitboardMasks.BlackPawnStart) != 0 && (square >> (2 * south) & empty) != 0)
                {
                    pawnMoves.Add(new Move { From = i, To = i - (2 * south), Type = MoveType.Normal });
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
                        pawnMoves.Add(new Move { From = i, To = i - southWest, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackQueen, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southWest, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackRook, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southWest, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackBishop, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southWest, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackKnight, CapturedPiece = capturedPiece });
                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i - southWest, Type = MoveType.Capture, CapturedPiece = capturedPiece });
                    }
                }

                if ((square & ~BitboardMasks.FileH) != 0 && (square >> southEast & whitePieces) != 0)
                {
                    capturedPiece = board.GetPieceAtSquare(i - southEast);

                    if ((square & BitboardMasks.BlackPrePromotionRank) != 0)
                    {
                        pawnMoves.Add(new Move { From = i, To = i - southEast, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackQueen, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southEast, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackRook, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southEast, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackBishop, CapturedPiece = capturedPiece });
                        pawnMoves.Add(new Move { From = i, To = i - southEast, Type = MoveType.PromotionCapture, PromotionPiece = PieceType.BlackKnight, CapturedPiece = capturedPiece });
                    }
                    else
                    {
                        pawnMoves.Add(new Move { From = i, To = i - southEast, Type = MoveType.Capture, CapturedPiece = capturedPiece });
                    }
                }

                if(board.EnPassantSquare != -1)
                {
                    int enPassantSquare = board.EnPassantSquare;
                    if ((square & ~BitboardMasks.FileA) != 0 && (i - southWest == enPassantSquare))
                    {
                        pawnMoves.Add(new Move { From = i, To = enPassantSquare, Type = MoveType.EnPassant, CapturedPiece = PieceType.WhitePawn });
                    }
                    if ((square & ~BitboardMasks.FileH) != 0 && (i - southEast == enPassantSquare))
                    {
                        pawnMoves.Add(new Move { From = i, To = enPassantSquare, Type = MoveType.EnPassant, CapturedPiece = PieceType.WhitePawn });
                    }
                }
            }

            return pawnMoves;
        }


    }
}
