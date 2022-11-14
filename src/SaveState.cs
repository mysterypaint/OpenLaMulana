using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLaMulana
{
    [Serializable]
    public class SaveState
    {
        public int Highscore { get; set; }
        public DateTime HighscoreDate { get; set; }
    }
}
