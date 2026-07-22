using System;
using System.Collections.Generic;
using System.Text;

namespace LuwiChess.Core.Enums
{
    public enum MoveType : byte
    {
        Normal,
        Capture,
        EnPassant,
        ShortCastle,
        LongCastle,
        Promotion,
        PromotionCapture
    }
}
