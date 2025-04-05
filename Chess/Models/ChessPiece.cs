namespace Chess.Models
{
    /// <summary>
    /// Abstract class that represents a chess piece.
    /// Implements the IChessPiece interface.
    /// </summary>
    public abstract class ChessPiece : IChessPiece
    {
        //Fields
        private int mValue;
        private ColorEnum mColor;
        private string mImagePath = "";
        private string mAbbreviation = "";

        //Properties
        public int Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }

        public ColorEnum Color
        {
            get
            {
                return mColor;
            }
            set
            {
                mColor = value;
            }
        }

        public string ImagePath
        {
            get
            {
                return mImagePath;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    mImagePath = value;
                }
            }
        }

        public string Abbreviation
        {
            get
            {
                return mAbbreviation;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    mAbbreviation = value;
                }
            }
        }
    }
}
