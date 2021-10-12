using System;

namespace OpenLaMulana
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new OpenLaMulanaGame())
                game.Run();
        }
    }
}
