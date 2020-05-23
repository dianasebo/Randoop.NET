//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Apache License, Version 2.0.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************



using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Randoop
{
    /// <summary>
    /// Represents the top-level command-line arguments that the
    /// user gave to Randoop, and provides methods for accessing them.
    /// </summary>
    public class CommandLineArguments
    {
        private string[] args;

        public int timeLimitSeconds = -1;
        public int restartTimeSeconds = -1;

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (string s in args)
                b.Append(" " + s);
            return b.ToString();
        }

        private static bool IsDllName(string s)
        {
            return s.ToLower().EndsWith(".dll") || s.ToLower().EndsWith(".exe");
        }

        public Collection<string> AssemblyNames
        {
            get
            {
                Collection<string> retval = new Collection<string>();
                foreach (string s in args)
                {
                    if (IsDllName(s))
                        retval.Add(s);
                }
                return retval;
            }
        }

        public bool UseRandoopContracts
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/randoopcontracts"))
                        return true;
                return false;
            }
        }

        public bool TrueRandom
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/truerandom"))
                        return true;
                return false;
            }
        }

        public bool NoExplorer
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/noexplorer"))
                        return true;
                return false;
            }
        }

        public bool KeepStatLogs
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/keepstatlogs"))
                        return true;
                return false;
            }
        }


        public bool Verbose
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/verbose"))
                        return true;
                return false;
            }
        }

        public bool PageHeap
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/pageheap"))
                        return true;
                return false;
            }
        }

        public string RandomSeed
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().StartsWith("/randomseed:"))
                        return s.Substring("/randomseed:".Length);
                return null;
            }
        }


        public string DirectoryStrategy
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/directorystrategy"))
                        return s.Substring("/outputdir:".Length);
                return null;
            }
        }

        public string OutputDir
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().StartsWith("/outputdir:"))
                        return s.Substring("/outputdir:".Length);
                return null;
            }
        }

        public string ConfigFilesDir
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().StartsWith("/configfiles:"))
                        return s.Substring("/configfiles:".Length);
                return null;
            }
        }


        public bool OutputNormal
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/outputnormal"))
                        return true;
                return false;
            }
        }

        public bool AllowNull
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/allownull"))
                        return true;
                return false;
            }
        }

        public bool NoStatic
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/nostatic"))
                        return true;
                return false;
            }
        }

        public bool ExploreInternal
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/internal"))
                        return true;
                return false;
            }
        }

        public bool UseDHandler
        {
            get
            {
                //foreach (string s in args)
                //    if (s.ToLower().Equals("/dhandler"))
                //        return true;
                return false; //NOT PART OF THE RELEASE
            }
        }

        public int TimeLimitSeconds
        {
            get
            {
                return this.timeLimitSeconds;
            }
        }

        public TimeSpan TimeLimit
        {
            get
            {
                return new TimeSpan(0, 0, this.TimeLimitSeconds);
            }
        }

        public string TimeLimitString
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().StartsWith("/timelimit:"))
                        return s;
                return null;
            }
        }

        public int RestartTimeSeconds
        {
            get
            {
                return this.restartTimeSeconds;
            }
        }

        public string RestartString
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().StartsWith("/restart:"))
                        return s;
                return null;
            }
        }

        /// <summary>
        /// NOT USER-VISIBLE.
        /// This option is used by RandoopTests project.
        /// </summary>
        public bool DontExecute
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/dontexecute"))
                        return true;
                return false;
            }
        }

        public bool Fair
        {
            get
            {
                foreach (string s in args)
                    if (s.ToLower().Equals("/fairexploration"))
                        return true;
                return false;
            }
        }


        public CommandLineArguments(string[] args, int defaultTimeLimit, int defaultRestartTimeSeconds, out string error)
        {
            this.args = new string[args.Length];
            Array.Copy(args, this.args, args.Length);

            if (defaultTimeLimit < 0)
                throw new ArgumentException();

            bool atLeastOneDll = false;
            foreach (string s in args)
                if (IsDllName(s))
                    atLeastOneDll = true;

            if (!atLeastOneDll)
            {
                error = "No assembly specified.";
                return;
            }

            // Determine time limit.
            if (TimeLimitString != null)
            {
                if (!int.TryParse(TimeLimitString.Substring("/timelimit:".Length), out this.timeLimitSeconds))
                {
                    error = "Invalid time limit: " + TimeLimitString;
                    return;
                }
                if (this.timeLimitSeconds <= 0)
                {
                    error = "Invalid time limit: " + TimeLimitString;
                    return;
                }
            }
            else
            {
                this.timeLimitSeconds = defaultTimeLimit;
            }

            // Determine restart time.
            if (RestartString != null)
            {
                if (!int.TryParse(RestartString.Substring("/restart:".Length), out this.restartTimeSeconds))
                {
                    error = "Invalid restart time: " + RestartString;
                    return;
                }
                if (this.restartTimeSeconds <= 0)
                {
                    error = "Invalid time limit: " + RestartString;
                    return;
                }
            }
            else
            {
                this.restartTimeSeconds = defaultRestartTimeSeconds;
            }

            if (this.TimeLimitSeconds < 0)
            {
                error = "Negative time limit given (" + this.timeLimitSeconds
                + "). Will use default time limit of " + this.timeLimitSeconds + " seconds.";
                return;
            }

            error = null;

        }
    }
}
