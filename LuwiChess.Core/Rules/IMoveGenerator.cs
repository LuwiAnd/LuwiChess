using LuwiChess.Core.Board;
using LuwiChess.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace LuwiChess.Core.Rules
{
    public interface IMoveGenerator
    {
        List<Move> GetLegalMoves(BoardState board);
    }
}
