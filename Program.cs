using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using EventHook;
using EventHook.Hooks;
using NAudio.Wave;
using MouseEventArgs = EventHook.MouseEventArgs;

namespace FartPoop {

    internal static class Program
    {
        private static Stopwatch _runWatch;
        private static WaveFileReader _runFile;
        private static WaveOutEvent _runOutput;

        // These have to be fields, otherwise the GC thanos snaps them and crashes the app.
        private static MouseWatcher _mouseWatcher;
        private static ClipboardWatcher _clipboardWatcher;

        private static void Main(string[] args)
        {
            _runWatch = new Stopwatch();
            _runFile = new WaveFileReader(GetResourceStream("scroll.wav"));
            _runOutput = new WaveOutEvent();

            _runWatch.Start();
            _runOutput.Init(_runFile);
            _runOutput.PlaybackStopped += RunOutputOnPlaybackStopped;

            var eventHookFactory = new EventHookFactory();

            using (eventHookFactory) {

                _mouseWatcher = eventHookFactory.GetMouseWatcher();
                _mouseWatcher.Start();
                _mouseWatcher.OnMouseInput += MouseWatcherOnOnMouseInput;

                _clipboardWatcher = eventHookFactory.GetClipboardWatcher();
                _clipboardWatcher.Start();
                _clipboardWatcher.OnClipboardModified += ClipboardWatcherOnOnClipboardModified;
            }

            PlaySound("windows.wav");

            MessageBox.Show(
                "hello im bill gator im inside ur comptr. to stop me middle click the top left corner of the screen.\n\nshoutouts to mors",
                "bill gator");

            while (true)
            {
            }
        }

        private static void RunOutputOnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_runWatch.ElapsedMilliseconds >= 500)
                return;

            _runFile.Position = 0;
            _runOutput.Play();
        }

        private static void MouseWatcherOnOnMouseInput(object sender, MouseEventArgs e)
        {
            switch (e.Message)
            {
                case MouseMessages.WM_LBUTTONDOWN:
                    PlaySound("button_left.wav");
                    break;
                case MouseMessages.WM_RBUTTONDOWN:
                    PlaySound("button_right.wav");
                    break;
                case MouseMessages.WM_WHEELBUTTONDOWN:
                    if ((e.Point.x == 0 || e.Point.x == -1) && (e.Point.y == 0 || e.Point.y == -1))
                    {
                        PlaySound("windows.wav");
                        Thread.Sleep(1000);
                        Environment.Exit(0);
                    }
                    else
                        PlaySound("button_middle.wav");
                    break;
                case MouseMessages.WM_XBUTTONDOWN:
                    PlaySound("button_other.wav");
                    break;
                case MouseMessages.WM_MOUSEWHEEL:
                    if (_runWatch.ElapsedMilliseconds > 50)
                        PlayRunSound();
                    break;
            }
        }

        private static void ClipboardWatcherOnOnClipboardModified(object sender, ClipboardEventArgs e)
        {
            PlaySound("clipboard.wav");
        }

        private static void PlayRunSound()
        {
            new Thread(() =>
            {
                _runWatch.Restart();

                if (_runOutput.PlaybackState != PlaybackState.Playing)
                    _runOutput.Play();
                else
                    return;

                while (_runWatch.ElapsedMilliseconds < 500)
                    Thread.Sleep(1);
                _runOutput.Pause();
            }).Start();
        }

        private static void PlaySound(string soundFile)
        {
            new Thread(() =>
            {
                var audioFile = new WaveFileReader(GetResourceStream(soundFile));
                using (audioFile) {
                    var outputDevice = new WaveOutEvent();
                    using (outputDevice)
                    {

                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                            Thread.Sleep(1000);
                    }
                }
            }).Start();
        }

        private static Stream GetResourceStream(string filename)
        {
            var resName = Path.Combine("sounds", filename);
            var fileStream = new FileStream(resName, FileMode.Open, FileAccess.Read);
            return fileStream;
        }
    }
}