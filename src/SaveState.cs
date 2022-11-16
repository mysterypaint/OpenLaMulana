using System;

namespace OpenLaMulana
{
    [Serializable]
    public class SaveState
    {
        public int Highscore { get; set; }
        public DateTime HighscoreDate { get; set; }
    }
}
