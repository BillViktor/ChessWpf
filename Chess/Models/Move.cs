namespace Chess.Models
{
    public class Move
    {
        public ChessPiece Piece { get; set; }
        public int ToRow { get; set; }
        public int ToColumn { get; set; }
    }
}
