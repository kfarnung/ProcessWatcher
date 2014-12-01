namespace ProcessWatcher
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// The main program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Missing required process filename.");
                Environment.Exit(1);
            }

            if (args.Length > 2)
            {
                Console.Error.WriteLine("Too many arguments provided.");
                Environment.Exit(1);
            }

            string filename = args[0];
            string arguments = null;

            if (args.Length >= 2)
            {
                arguments = args[1];
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ProcessWatcherApp(filename, arguments));
        }
    }
}
