using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace IrcBotFramework
{
    public class ChanServ
    {
        public bool EnableOpCache;

        private IrcBot Bot;
        private Action PendingAction, CompletedAction;
        private DateTime DeopTime;
        private Timer DeopTimer;
        private string DeopChannel;

        public ChanServ(IrcBot Bot)
        {
            this.Bot = Bot;
            this.EnableOpCache = true;
            this.DeopTimer = new Timer(DeopTimerFire, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Op(string channel)
        {
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            Bot.SendMessage("ChanServ", "op " + channel);
        }

        public void Op(string channel, Action action)
        {
            PendingAction = action;
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            Bot.SendMessage("ChanServ", "op " + channel);
        }

        public void Op(string channel, string user)
        {
            Bot.SendMessage("ChanServ", "op " + channel + " " + user);
        }

        public void Op(string channel, string user, Action action)
        {
            PendingAction = action;
            Bot.SendMessage("ChanServ", "op " + channel + " " + user);
        }

        public void DeOp(string channel)
        {
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            Bot.SendMessage("ChanServ", "deop " + channel);
        }

        public void DeOp(string channel, Action action)
        {
            PendingAction = action;
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            Bot.SendMessage("ChanServ", "deop " + channel);
        }

        public void DeOp(string channel, string user)
        {
            Bot.SendMessage("ChanServ", "deop " + channel + " " + user);
        }

        public void DeOp(string channel, string user, Action action)
        {
            PendingAction = action;
            Bot.SendMessage("ChanServ", "deop " + channel + " " + user);
        }

        public void Voice(string channel)
        {
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            Bot.SendMessage("ChanServ", "voice " + channel);
        }

        public void Voice(string channel, Action action)
        {
            PendingAction = action;
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            Bot.SendMessage("ChanServ", "voice " + channel);
        }

        public void Voice(string channel, string user)
        {
            Bot.SendMessage("ChanServ", "voice " + channel + " " + user);
        }

        public void Voice(string channel, string user, Action action)
        {
            PendingAction = action;
            Bot.SendMessage("ChanServ", "voice " + channel + " " + user);
        }

        public void DeVoice(string channel)
        {
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            Bot.SendMessage("ChanServ", "devoice " + channel);
        }

        public void DeVoice(string channel, Action action)
        {
            PendingAction = action;
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            Bot.SendMessage("ChanServ", "devoice " + channel);
        }

        public void DeVoice(string channel, string user)
        {
            Bot.SendMessage("ChanServ", "devoice " + channel + " " + user);
        }

        public void DeVoice(string channel, string user, Action action)
        {
            PendingAction = action;
            Bot.SendMessage("ChanServ", "devoice " + channel + " " + user);
        }

        public void TempOp(string channel, Action action)
        {
            if (DeopChannel == channel)
            {
                action();
                return;
            }
            PendingAction = action;
            Bot.ChannelModeChanged += Bot_ChannelModeChanged;
            CompletedAction = () =>
            {
                if (!EnableOpCache)
                {
                    PendingAction();
                    PendingAction = null;
                    Bot.ChangeMode(channel, "-o " + Bot.User.Nick);
                }
                else
                {
                    DeopTime = DateTime.Now.AddSeconds(30);
                    DeopChannel = channel;
                    DeopTimer.Change((int)((DeopTime - DateTime.Now).TotalSeconds), Timeout.Infinite);
                }
            };
            Bot.SendMessage("ChanServ", "op " + channel);
        }

        private void DeopTimerFire(object o)
        {
            if (DateTime.Now >= DeopTime)
            {
                Bot.ChangeMode(DeopChannel, "-o " + Bot.User.Nick);
                DeopChannel = null;
            }
            else
                DeopTimer.Change((int)((DeopTime - DateTime.Now).TotalSeconds), Timeout.Infinite);
        }

        void Bot_ChannelModeChanged(object sender, ChannelModeChangeEventArgs e)
        {
            Bot.ChannelModeChanged -= Bot_ChannelModeChanged;
            if (PendingAction != null)
                PendingAction();
            if (CompletedAction != null)
                CompletedAction();
        }
    }
}
