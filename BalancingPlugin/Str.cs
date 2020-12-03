using System;
using System.Runtime.CompilerServices;

namespace AllInOnePlugin
{
    public class Str
    {
        [DecimalConstant(0, 0, 0, 0, 1024)]
        public readonly static decimal KB;

        public readonly static decimal MB;

        public readonly static decimal GB;

        public readonly static decimal TB;

        public readonly static decimal PB;

        static Str()
        {
            Str.KB = new decimal(1024);
            Str.MB = Convert.ToDecimal(Math.Pow(1024, 2));
            Str.GB = Convert.ToDecimal(Math.Pow(1024, 3));
            Str.TB = Convert.ToDecimal(Math.Pow(1024, 4));
            Str.PB = Convert.ToDecimal(Math.Pow(1024, 5));
        }

        public Str()
        {
        }

        public static string ToHumanSize(long TheBytes)
        {
            if (TheBytes >= (long)0)
            {
                return Str.ToHumanSize(Convert.ToUInt64(TheBytes));
            }
            return string.Concat("-", Str.ToHumanSize(Convert.ToUInt64(-TheBytes)));
        }

        public static string ToHumanSize(ulong TheBytes)
        {
            return Str.ToHumanSizeResult(TheBytes).ToString();
        }

        public static Str.HumanSizeResult ToHumanSizeResult(ulong TheBytes)
        {
            decimal theBytes;
            if (TheBytes < new decimal(1024))
            {
                return new Str.HumanSizeResult(TheBytes.ToString("#,0"), "B");
            }
            if (TheBytes < new decimal(10240))
            {
                theBytes = TheBytes / new decimal(1024);
                return new Str.HumanSizeResult(theBytes.ToString("#,0.00"), "KB");
            }
            if (TheBytes < new decimal(102400))
            {
                theBytes = TheBytes / new decimal(1024);
                return new Str.HumanSizeResult(theBytes.ToString("#,0.0"), "KB");
            }
            if (TheBytes < Str.MB)
            {
                theBytes = TheBytes / new decimal(1024);
                return new Str.HumanSizeResult(theBytes.ToString("#,0"), "KB");
            }
            if (TheBytes < (Str.MB * new decimal(10)))
            {
                theBytes = TheBytes / Str.MB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0.00"), "MB");
            }
            if (TheBytes < (Str.MB * new decimal(100)))
            {
                theBytes = TheBytes / Str.MB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0.0"), "MB");
            }
            if (TheBytes < Str.GB)
            {
                theBytes = TheBytes / Str.MB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0"), "MB");
            }
            if (TheBytes < (Str.GB * new decimal(10)))
            {
                theBytes = TheBytes / Str.GB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0.00"), "GB");
            }
            if (TheBytes < (Str.GB * new decimal(100)))
            {
                theBytes = TheBytes / Str.GB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0.0"), "GB");
            }
            if (TheBytes < Str.TB)
            {
                theBytes = TheBytes / Str.GB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0"), "GB");
            }
            if (TheBytes < (Str.TB * new decimal(10)))
            {
                theBytes = TheBytes / Str.TB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0.00"), "TB");
            }
            if (TheBytes < (Str.TB * new decimal(100)))
            {
                theBytes = TheBytes / Str.TB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0.0"), "TB");
            }
            if (TheBytes < Str.PB)
            {
                theBytes = TheBytes / Str.TB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0"), "TB");
            }
            if (TheBytes < (Str.PB * new decimal(10)))
            {
                theBytes = TheBytes / Str.PB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0.00"), "PB");
            }
            if (TheBytes < (Str.PB * new decimal(100)))
            {
                theBytes = TheBytes / Str.PB;
                return new Str.HumanSizeResult(theBytes.ToString("#,0.0"), "PB");
            }
            theBytes = TheBytes / Str.PB;
            return new Str.HumanSizeResult(theBytes.ToString("#,0"), "PB");
        }

        public struct HumanSizeResult
        {
            public string Value;

            public string Unit;

            public HumanSizeResult(string TheValue, string TheUnit)
            {
                this.Value = TheValue;
                this.Unit = TheUnit;
            }

            public override string ToString()
            {
                return string.Concat(this.Value, " ", this.Unit);
            }
        }
    }
}