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

        public const ulong WhiteSquares = 0x55AA55AA55AA55AA;
        public const ulong BlackSquares = 0xAA55AA55AA55AA55;

        public const ulong WhitePawnStart = Rank2;
        public const ulong BlackPawnStart = Rank7;
        public const ulong WhitePromotionRank = Rank8;
        public const ulong BlackPromotionRank = Rank1;
        public const ulong WhitePrePromotionRank = Rank7;
        public const ulong BlackPrePromotionRank = Rank2;
    }
}
