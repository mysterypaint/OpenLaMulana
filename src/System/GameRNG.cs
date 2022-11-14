using System;

namespace OpenLaMulana.System
{
    // Special thanks to worsety for info on this class!
    public class GameRNG
    {
        public long State;

        public GameRNG()
        {
            DateTime bootTime = DateTime.Now;
            State = new DateTimeOffset(bootTime).ToUnixTimeMilliseconds();
        }

        public void Advance()
        {
            State = State * 109 + 1021 & 32767;
        }
        public long RollDice(int diceNumber)
        {
            return (State * 109 + 1021) % 32767 % diceNumber;
        }
    }
}
