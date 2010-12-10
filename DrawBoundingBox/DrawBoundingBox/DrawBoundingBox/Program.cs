namespace DrawBoundingBox
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DrawBoundingBoxGame game = new DrawBoundingBoxGame())
            {
                game.Run();
            }
        }
    }
#endif
}

