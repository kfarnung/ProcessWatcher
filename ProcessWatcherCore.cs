namespace ProcessWatcher
{
    using System;
    using System.Diagnostics;
    using System.Timers;

    /// <summary>
    /// The core class for watching the process.
    /// </summary>
    public class ProcessWatcherCore : IDisposable
    {
        #region Fields

        /// <summary>
        /// The current process.
        /// </summary>
        private Process process;

        /// <summary>
        /// The close timer.
        /// </summary>
        private Timer closeTimer;

        /// <summary>
        /// Whether the process has ever been started.
        /// </summary>
        private bool started;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessWatcherCore"/> class.
        /// </summary>
        /// <param name="path">The process path.</param>
        /// <param name="arguments">The process arguments.</param>
        public ProcessWatcherCore(string path, string arguments)
        {
            this.process = new Process();
            this.process.EnableRaisingEvents = true;
            this.process.StartInfo = new ProcessStartInfo(path, arguments);
            this.process.Exited += this.OnProcessExited;

            this.closeTimer = new Timer(5000);
            this.closeTimer.Elapsed += this.OnCloseTimerElapsed;
            this.closeTimer.AutoReset = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the process exits.
        /// </summary>
        public event EventHandler Exited;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILogger Logger
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the process is currently running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return !this.process.HasExited;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Launches the application.
        /// </summary>
        public void Launch()
        {
            if (!this.started || this.process.HasExited)
            {
                this.started = true;
                this.Log("The process is starting...");
                this.process.Start();
            }
            else
            {
                this.Log("The process is already running.");
            }
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        /// <param name="timeout">The timeout in seconds.</param>
        public void Close(int timeout)
        {
            if (!this.process.HasExited)
            {
                this.Log("The process is closing...");
                this.closeTimer.Interval = timeout * 1000;
                this.closeTimer.Start();

                this.process.CloseMainWindow();
            }
            else
            {
                this.Log("The process is not running.");
            }
        }

        /// <summary>
        /// Kills the process.
        /// </summary>
        public void Kill()
        {
            if (!this.process.HasExited)
            {
                this.Log("The prociess is being killed...");
                this.process.Kill();
            }
        }

        /// <summary>
        /// Dispose the process watcher.
        /// </summary>
        public void Dispose()
        {
            if (this.closeTimer != null)
            {
                this.closeTimer.Dispose();
                this.closeTimer = null;
            }

            if (this.process != null)
            {
                this.process.Dispose();
                this.process = null;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="message">The log message text.</param>
        private void Log(string message)
        {
            if (this.Logger != null)
            {
                this.Logger.Log(message);
            }
        }

        /// <summary>
        /// Invokes the <see cref="ProcessWatcherCore.Exited"/> event.
        /// </summary>
        private void OnExited()
        {
            this.Log("The process has exited.");

            if (this.Exited != null)
            {
                this.Exited.Invoke(this, new EventArgs());
            }
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handles the process exited event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnProcessExited(object sender, EventArgs e)
        {
            this.closeTimer.Stop();
            this.OnExited();
        }

        /// <summary>
        /// Handles the close timer event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCloseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.Log("The process failed to close within the specified interval.");
            this.closeTimer.Stop();

            this.Kill();
        }

        #endregion
    }
}
