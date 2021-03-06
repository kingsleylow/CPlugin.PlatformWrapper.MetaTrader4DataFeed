﻿using System;
using System.Diagnostics;
using System.Threading;
using CPlugin.PlatformWrapper.MetaTrader4DataFeed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class FeederTest
    {
        private readonly Feeder _feed = new Feeder()
        {
            Name = "test feed",
            Path = @"F:\temp\mt4\mt4feeder.feed",
            Server = "10.200.0.3:10443",
            Login = 1002,
            Password = "RtxD4bo_",
            ReconnectErrorsLimit = 1,
            ReconnectTimeout = 1,
            ReconnectRetryTimeout = 1,
            ReadErrorsLimit = 1
        };

        [TestMethod]
        public void GetQuotesDuringNextFewSeconds()
        {
            "Init".ToConsole();

            // enable if you need only one symbol
            //feed.AdviseSymbol("EURUSD");

            int quotesReceived = 0;
            _feed.Started += (sender, args) => { "Started".ToConsole(); };
            _feed.ChangeStatus += (sender, args) => { $"Change Status: {args.Status}".ToConsole(); };
            _feed.Paused += (sender, args) => { "Paused".ToConsole(); };
            _feed.QuoteReceived += (sender, args) => {
                $"Quote Received: {args.Quote.Symbol} {args.Quote.Bid}/{args.Quote.Ask}".ToConsole();
                ++quotesReceived;

                // receive first N quotes then leave
                if (quotesReceived < 5)
                    return;

                "Stop".ToConsole();
                _feed.Stop();
            };
            _feed.Stopped += (sender, args) => { "Stopped".ToConsole(); };

            "Start".ToConsole();
            var started = DateTime.Now;
            _feed.Start();

            // wait until N ticks came
            // but not too infinite time
            while (quotesReceived < 5 && DateTime.Now - started < TimeSpan.FromSeconds(60))
            {
                //"Sleep...".ToConsole();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                //"Sleep done".ToConsole();
            }

            "Stop".ToConsole();
            _feed.Stop();
        }
    }

    public static class Extensions
    {
        public static void ToConsole(this string message)
        {
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
    }
}