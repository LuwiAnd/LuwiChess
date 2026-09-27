using LuwiChess.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LuwiChess.Core.Moves
{
    public readonly record struct Move
    (
        int From,
        int To,
        PieceType MovingPiece,
        MoveType Type = MoveType.Normal,
        PieceType PromotionPiece = PieceType.Empty,
        PieceType CapturedPiece = PieceType.Empty
    );
}
