namespace ProcessWatcher
{
    using System;
    using System.Windows.Forms;
    using Microsoft.Win32;
    using ProcessWatcher.Properties;

    using Timer = System.Windows.Forms.Timer;

    /// <summary>
    /// The main app form.
    /// </summary>
    public partial class ProcessWatcherApp : Form, ILogger
    {
        #region Fields

        /// <summary>
        /// The process watcher.
        /// </summary>
        private ProcessWatcherCore processWatcher;

        /// <summary>
        /// The timer.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The number of ticks remaining.
        /// </summary>
        private int numTicksRemaining;

        /// <summary>
        /// Whether we're shutting down.
        /// </summary>
        private bool shuttingDown;

        /// <summary>
        /// Whether shutdown is complete.
        /// </summary>
        private bool shutdownComplete;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessWatcherApp"/> class.
        /// </summary>
        /// <param name="path">The process path.</param>
        /// <param name="arguments">The process arguments.</param>
        public ProcessWatcherApp(string path, string arguments)
        {
            this.processWatcher = new ProcessWatcherCore(path, arguments);
            this.processWatcher.Exited += this.OnProcessWatcherExited;
            this.processWatcher.Logger = this;

            this.timer = new Timer();
            this.timer.Interval = 1000;
            this.timer.Tick += this.OnTimerTick;

            this.FormClosing += this.OnFormClosing;
            this.Shown += this.OnFormShown;
            SystemEvents.PowerModeChanged += this.OnPowerModeChanged;

            this.InitializeComponent();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The log message.</param>
        public void Log(string message)
        {
            this.BeginInvoke(new Action(delegate
            {
                this.UpdateStatus(message);
            }));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Launches the application.
        /// </summary>
        private void LaunchApplication()
        {
            this.processWatcher.Launch();
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        private void CloseApplication()
        {
            if (this.timer.Enabled)
            {
                this.timer.Stop();
            }

            this.processWatcher.Close(Settings.Default.CloseTimeout);
        }

        /// <summary>
        /// Update the countdown.
        /// </summary>
        private void UpdateCountdown()
        {
            this.UpdateStatus(string.Format("Restarting in {0} seconds...", this.numTicksRemaining));
        }

        /// <summary>
        /// Update the status.
        /// </summary>
        /// <param name="text">The status text.</param>
        private void UpdateStatus(string text)
        {
            this.statusLabel.Text = text;
            this.WriteLog(DateTime.Now.ToString("u") + ": " + text);
        }

        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <param name="text">The log message.</param>
        private void WriteLog(string text)
        {
            this.textBox1.Text = text + Environment.NewLine + this.textBox1.Text;
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handles the timer tick.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTimerTick(object sender, EventArgs e)
        {
            this.numTicksRemaining--;

            if (this.numTicksRemaining > 0)
            {
                this.UpdateCountdown();
            }
            else
            {
                this.timer.Stop();
                this.LaunchApplication();
            }
        }

        /// <summary>
        /// Handles the process watcher exited event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnProcessWatcherExited(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
                {
                    if (!this.shuttingDown)
                    {
                        this.numTicksRemaining = Settings.Default.CountdownTimer;
                        this.UpdateCountdown();
                        this.timer.Start();
                    }
                    else
                    {
                        this.shutdownComplete = true;
                        this.processWatcher.Dispose();
                        this.Close();
                    }
                }));
        }

        /// <summary>
        /// Handles the quit button click.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnQuitButtonClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the form's initial display.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnFormShown(object sender, EventArgs e)
        {
            this.LaunchApplication();
        }

        /// <summary>
        /// Handles the form's closing.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.shuttingDown)
            {
                this.shuttingDown = true;
                this.quitButton.Enabled = false;
                SystemEvents.PowerModeChanged -= this.OnPowerModeChanged;

                if (this.processWatcher.IsRunning)
                {
                    this.CloseApplication();

                    // Cancel close until the application finishes closing.
                    e.Cancel = true;
                }
            }
            else if (!this.shutdownComplete)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles the power mode change.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    this.CloseApplication();
                    break;
            }
        }

        #endregion
    }
}
