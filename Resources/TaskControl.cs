using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using ConvenienceMethods;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImgFlip;
using NepBot.Core.Data;
using NepBot.Data;
using NepBot.Resources.Code_Implements;
using NepBot.Resources.Extensions;
using Discord.Rest;
using System.Timers;

namespace NepBot.Resources
{
    /// <summary>
    /// For controlling anything in code that requires a one off delay rather than using Task.Delay. Can be run indefinitely if needed.
    /// </summary>
    public class TaskControl
    {
        Timer timer = new Timer();
        static Timer nullAdd = new Timer(10000);
        ElapsedEventHandler f;
        int delay;
        bool runForever;
        IUserMessage toDelete = null;

        public TaskControl(ElapsedEventHandler f, int delay, bool runForever = false)
        {
            this.f = f;
            this.delay = delay;
            this.runForever = runForever;
            nullAdd.AutoReset = true;
            nullAdd.Enabled = true;
            SetTimer();
        }

        public bool IsEnabled
        {
            get { return timer.Enabled; }
        }

        /// <summary>
        /// Set to run on permanent timer that is static
        /// </summary>
        /// <param name="eh"></param>
        public static void AddToElapsed(ElapsedEventHandler eh)
        {
            nullAdd.Elapsed += eh;
        }

        public void AddDeletion(IUserMessage rum)
        {
            toDelete = rum;
            runForever = false;
        }

        private void SetTimer()
        {
            timer = new System.Timers.Timer();
            timer.Interval = delay;

            if (f != null)
                timer.Elapsed += f;
            if (!runForever)
                timer.Elapsed += EndTimer;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void EndTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            timer.AutoReset = false;
            timer.Enabled = false;
            if (toDelete != null)
                toDelete.DeleteAsync();
        }

    }
}
