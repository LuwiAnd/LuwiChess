using LuwiChess.Core.Board;
using LuwiChess.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace LuwiChess.Core.Rules
{
    public interface IMoveApplier
    {
        BoardState MakeMove(BoardState boardState, Move move);
    }
}
