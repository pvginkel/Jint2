using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Jint.Tests.Support
{
    public abstract class TestBase
    {
        private readonly string _testsPath;

        protected abstract string BasePath { get; }

        public TestBase(string testsPath)
        {
            _testsPath = BasePath;

            if (testsPath != null)
                _testsPath = Path.Combine(_testsPath, testsPath);
        }

        protected abstract string GetInclude(string file);

        protected string GetSpecialInclude(string file)
        {
            return
                file == "environment.js"
                ? GetTimeZoneInfoInclude()
                : null;
        }

        private static string GetTimeZoneInfoInclude()
        {
            var local = TimeZoneInfo.Local;
            var now = DateTime.UtcNow;

            var daylightRule = local.GetAdjustmentRules().Single(a => a.DateEnd > now && a.DateStart <= now);

            var start = daylightRule.DaylightTransitionStart;
            var end = daylightRule.DaylightTransitionEnd;

            var info = new StringBuilder();

            info.AppendLine(string.Format("var $DST_end_hour = {0};", end.TimeOfDay.Hour));
            info.AppendLine(string.Format("var $DST_end_minutes = {0};", end.TimeOfDay.Minute));
            info.AppendLine(string.Format("var $DST_end_month = {0};", end.Month));
            info.AppendLine(string.Format("var $DST_end_sunday = '{0}';", end.Day > 15 ? "last" : "first"));
            info.AppendLine(string.Format("var $DST_start_hour = {0};", start.TimeOfDay.Hour));
            info.AppendLine(string.Format("var $DST_start_minutes = {0};", start.TimeOfDay.Minute));
            info.AppendLine(string.Format("var $DST_start_month = {0};", start.Month));
            info.AppendLine(string.Format("var $DST_start_sunday = '{0}';", start.Day > 15 ? "last" : "first"));
            info.AppendLine(string.Format("var $LocalTZ = {0};", local.BaseUtcOffset.TotalSeconds / 3600));

            return info.ToString();
        }

        protected virtual JintEngine CreateContext(Action<string> errorAction)
        {
            return CreateContext(errorAction, Options.EcmaScript5 | Options.Strict);
        }

        protected JintEngine CreateContext(Action<string> errorAction, Options options)
        {
            var ctx = new JintEngine(options);

            Action<string> failAction = Assert.Fail;
            Action<string> printAction = message => Trace.WriteLine(message);
            Action<string> includeAction = file => ctx.Run(GetInclude(file));

            ctx.SetFunction("$FAIL", failAction);
            ctx.SetFunction("ERROR", errorAction);
            ctx.SetFunction("$ERROR", errorAction);
            ctx.SetFunction("$PRINT", printAction);
            ctx.SetFunction("$INCLUDE", includeAction);

            return ctx;
        }

        public object RunScript(string script)
        {
            var errorText = new StringBuilder();

            object result;

            try
            {
                var ctx = CreateContext(e => errorText.AppendLine(e));

                result = RunScript(ctx, script);
            }
            catch (Exception ex)
            {
                throw new Exception("Test threw an exception.", ex);
            }

            if (errorText.Length > 0)
                Assert.Fail(errorText.ToString());

            return result;
        }

        protected virtual object RunScript(JintEngine ctx, string script)
        {
            return ctx.Run(script);
        }

        public object RunFile(string fileName)
        {
            var errorText = new StringBuilder();

            fileName = Path.Combine(_testsPath, fileName);

            var ctx = CreateContext(e => errorText.AppendLine(e));

            var result = RunFile(ctx, fileName);

            if (errorText.Length > 0)
                Assert.Fail(errorText.ToString());

            return result;
        }

        protected virtual object RunFile(JintEngine ctx, string fileName)
        {
            return ctx.Run(File.ReadAllText(fileName));
        }
    }
}