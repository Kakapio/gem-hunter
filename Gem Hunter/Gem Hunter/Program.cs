using System;

namespace Gem_Hunter
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GemHunter())
                game.Run();
        }
    }
}
