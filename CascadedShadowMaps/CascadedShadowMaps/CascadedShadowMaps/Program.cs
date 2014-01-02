using System;

namespace CascadedShadowMaps
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ShadowMappingGame game = new ShadowMappingGame())
            {
                game.Run();
            }
        }
    }
#endif
}

