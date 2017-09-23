﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace GlobalHotkeys
{
    public delegate void HotkeyHandler();
    public class Hotkey : IDisposable
    {
        public const int WmHotkeyMsgId = 0x0312;
        private Keys Key { get; }
        private Modifiers Modifiers { get; }
        public HotkeyHandler Handler { get; }
        private int Id { get; }

        private IWin32Window _registeredWindow;
        private bool _registered;

        public Hotkey(Keys key, Modifiers modifiers, HotkeyHandler handler)
        {
            Key = key;
            Modifiers = modifiers;
            Handler = handler;
            Id = GetHashCode();
        }

        /// <summary>
        /// Registers the current hotkey with Windows.
        /// Note! You must override the WndProc method in your window that registers the hotkey, or you will not receive any hotkey notifications.
        /// </summary>
        public void Register(IWin32Window window)
        {
            if (window == null) 
                throw new ArgumentNullException(@"window", "You must provide a form or window to register the hotkey against.");
            if (!RegisterHotKey(window.Handle, Id, (int)Modifiers, (int)Key))
                throw new GlobalHotkeyException("Hotkey " + Modifiers + " + " + (char)Key + " failed to register.");
            _registered = true;
            _registeredWindow = window;
        }

        /// <summary>
        /// Unregisters the current hotkey with Windows.
        /// </summary>
        public void Unregister()
        {
            if (!_registered) return;
            if (!UnregisterHotKey(_registeredWindow.Handle, Id))
                throw new GlobalHotkeyException("Hotkey failed to unregister.");
            _registered = false;
            _registeredWindow = null;
        }
        public void Dispose()
        {
            try
            {
                Unregister();
            }
            catch (GlobalHotkeyException exc) { Debug.WriteLine(exc.Message); }
            GC.SuppressFinalize(this);
        }

        public override sealed int GetHashCode()
        {
            return (int)Modifiers ^ (int)Key;
        }


        public bool IsPressedKeyCombination(IntPtr lParam)
        {
            var lpInt = (int)lParam;
            return Key == (Keys) ((lpInt >> 16) & 0xFFFF) && Modifiers == (Modifiers) (lpInt & 0xFFFF);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    [Flags]
    public enum Modifiers
    {
        NoMod = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        Shift = 0x0004,
        Win = 0x0008
    }
}
