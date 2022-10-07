﻿using System;
using System.Diagnostics;
using System.IO;

namespace RM2ExCoop
{
    internal class Logger
    {
        static Logger Instance
        {
            get
            {
                _instance ??= new Logger();
                return _instance;
            }
        }

        static Logger? _instance;

        public static void Setup(TextWriter output)
        {
            _instance = new(output);
        }

        public static void Info(string text)
        {
            if (Debugger.IsAttached)
                System.Diagnostics.Debug.WriteLine("[INFO]" + GetTimestamp() + " " + text);
            else
                Instance.WriteInfo(text);
        }

        public static void Warn(string text)
        {
            if (Debugger.IsAttached)
                System.Diagnostics.Debug.WriteLine("[WARN]" + GetTimestamp() + " " + text);
            else
                Instance.WriteWarn(text);
        }

        public static void Error(string text)
        {
            if (Debugger.IsAttached)

                System.Diagnostics.Debug.WriteLine("[ERROR]" + GetTimestamp() + " " + text);
            else
                Instance.WriteError(text);
        }

        public static void Debug(string text)
        {
            if (Debugger.IsAttached)
                System.Diagnostics.Debug.WriteLine(text);
            else
                Instance.WriteDebug(text);
        }

        static string GetTimestamp() => DateTime.Now.ToString("[ddd, dd MMM yyy HH':'mm':'ss]");

        readonly TextWriter _output;

        public Logger(TextWriter? output = null)
        {
            _output = output ?? Console.Out;
        }

        void WriteInfo(string text) => _output.WriteLine("[INFO]" + GetTimestamp() + " " + text);
        void WriteWarn(string text) => _output.WriteLine("[WARN]" + GetTimestamp() + " " + text);
        void WriteError(string text) => _output.WriteLine("[ERROR]" + GetTimestamp() + " " + text);
        void WriteDebug(string text) => _output.WriteLine("[DEBUG]" + GetTimestamp() + " " + text);
    }
}
