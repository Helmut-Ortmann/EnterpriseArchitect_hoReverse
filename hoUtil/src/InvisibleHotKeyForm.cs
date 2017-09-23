﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace GlobalHotkeys
{
    public partial class InvisibleHotKeyForm : Form
    {
        private const int WM_ACTIVATEAPP = 0x001C;
        private const int WM_NCACTIVATE = 0x0086;
        private readonly IEnumerable<Hotkey> _hotkeys;
        private readonly int _thisProcessId = Process.GetCurrentProcess().Id;
        private int _lastActiveProcessId;
        //private readonly BackgroundWorker _worker = new BackgroundWorker();

        public InvisibleHotKeyForm(IEnumerable<Hotkey> hotkeys)
        {
            InitializeComponent();
            _hotkeys = hotkeys;
            _lastActiveProcessId = _thisProcessId;
            //_worker.DoWork += worker_DoWork;
            //_worker.RunWorkerCompleted += (sender, eventArgs) => _worker.RunWorkerAsync();
            Closing += (sender, eventArgs) => ((List<Hotkey>) _hotkeys).ForEach(key => key.Dispose());
        }

        //private void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    int activeId = ActiveProcess.GetActiveProcess().Id;
        //    if (_lastActiveProcessId == activeId)
        //        return;
        //    if (_thisProcessId == activeId) //This EA instance got focus
        //    {
        //        BeginInvoke((MethodInvoker)RegisterHotKeys);
        //    }
        //    else if (_thisProcessId != activeId) //This EA instance lost focus
        //    {
        //        BeginInvoke((MethodInvoker)UnregisterHotKeys);
        //    }
        //    _lastActiveProcessId = activeId;
        //}

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            //_worker.RunWorkerAsync();
            base.OnLoad(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Hotkey.WmHotkeyMsgId)
            {
                foreach (Hotkey key in _hotkeys)
                {
                    if (key.IsPressedKeyCombination(m.LParam))
                    {
                        key.Handler();
                        break;
                    }
                }
            }
            else if (m.Msg == WM_ACTIVATEAPP || m.Msg == WM_NCACTIVATE)
            {
                bool windowGotFocus = (m.WParam.ToInt32() == 1);
                if (windowGotFocus)
                {
                    RegisterHotKeys();
                }
                else
                {
                    UnregisterHotKeys();
                }
            }
            base.WndProc(ref m);
        } 

        private void UnregisterHotKeys()
        {
            if (!_hotkeys.Any()) return;
            foreach (Hotkey key in _hotkeys)
            {
                key.Unregister();
            }
        }

        private void RegisterHotKeys()
        {
            UnregisterHotKeys();
            foreach (Hotkey key in _hotkeys)
            {
                try
                {
                    key.Register(window:this);
                }
                catch (GlobalHotkeyException exc) { Debug.WriteLine(exc.Message); }
            }
        }
    }
}
