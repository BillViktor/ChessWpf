namespace Chess.Models
{
    public class TimerSetting
    {
        //White
        public bool WhiteEnabled { get; set; }
        public int WhiteTimer { get; set; }
        public int WhiteIncrement { get; set; }

        //Black
        public bool BlackEnabled { get; set; }
        public int BlackTimer { get; set; }
        public int BlackIncrement { get; set; }

    }
}
