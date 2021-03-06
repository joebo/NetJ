﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace J.Wd
{
    public class Parser
    {
        public const int EVDOMAIN = 3;

        public const int LIT = 2;

        public static readonly char[] ws = new char[] { ' ', '\r', '\n', '\t' };
        public static string mainWindowId = "Main Window";
        
        private static bool IsWdSet = false;
        private static bool UseWF = false;

#if WIN64
        private const int iSz = sizeof(long);
        //#define AK(x)           ((x)->k)        /* offset of ravel wrt x           */
        private static long AK(IntPtr a)
        {
            return Marshal.ReadInt64(a, 0 * iSz);
        }
        //#define AFLAG(x)        ((x)->flag)     /* flag                            */
        private static long AFLAG(IntPtr a)
        {
            return Marshal.ReadInt64(a, 1 * iSz);
        }
        //#define AM(x)           ((x)->m)        /* Max # bytes in ravel            */
        private static long AM(IntPtr a)
        {
            return Marshal.ReadInt64(a, 2 * iSz);
        }
        //#define AT(x)           ((x)->t)        /* Type;                           */
        private static long AT(IntPtr a)
        {
            return Marshal.ReadInt64(a, 3 * iSz);
        }
        //#define AC(x)           ((x)->c)        /* Reference count.                */
        private static long AC(IntPtr a)
        {
            return Marshal.ReadInt64(a, 4 * iSz);
        }
        //#define AN(x)           ((x)->n)        /* # elements in ravel             */
        private static long AN(IntPtr a)
        {
            return Marshal.ReadInt64(a, 5 * iSz);
        }
        //#define AR(x)           ((x)->r)        /* Rank                            */
        private static long AR(IntPtr a)
        {
            return Marshal.ReadInt64(a, 6 * iSz);
        }
#else
        private const int iSz = sizeof(int);
        private static int AK(IntPtr a)
        {
            return Marshal.ReadInt32(a, 0 * iSz);
        }
        private static int AFLAG(IntPtr a)
        {
            return Marshal.ReadInt32(a, 1 * iSz);
        }
        private static int AM(IntPtr a)
        {
            return Marshal.ReadInt32(a, 2 * iSz);
        }
        private static int AT(IntPtr a)
        {
            return Marshal.ReadInt32(a, 3 * iSz);
        }
        private static int AC(IntPtr a)
        {
            return Marshal.ReadInt32(a, 4 * iSz);
        }
        private static int AN(IntPtr a)
        {
            return Marshal.ReadInt32(a, 5 * iSz);
        }
        private static int AR(IntPtr a)
        {
            return Marshal.ReadInt32(a, 6 * iSz);
        }
#endif
        //#define AS(x)           ((x)->s)        /* Pointer to shape                */
        private static IntPtr AS(IntPtr a)
        {
            return IntPtr.Add(a, 7 * iSz);
        }
        //#define AV(x)           ( (I*)((C*)(x)+AK(x)))  /* pointer to ravel        */
        private static IntPtr AV(IntPtr a)
        {
            return IntPtr.Add(a, (int)AK(a));
        }
        //#define BAV(x)          (      (C*)(x)+AK(x) )  /* boolean                 */
        private static bool BAV(IntPtr a)
        {
            return (byte)0 != Marshal.ReadByte(a, (int)AK(a));
        }
        //#define CAV(x)          (      (C*)(x)+AK(x) )  /* character               */
        private static string CAV(IntPtr a)
        {
            return Encoding.UTF8.GetString(Parser.BytesFromPtr(AV(a), AN(a)));
        }

        private static byte[] BytesFromPtr(IntPtr ptr, long sz)
        {
            var data = new List<byte>(); var off = 0;
            while (0 < sz--)
            {
                data.Add(Marshal.ReadByte(ptr, off++));
            }
            return data.ToArray();
        }

        public static int Parse(int t, IntPtr w, ref IntPtr z)
        {
            if (0 != t || 0 == AN(w) || AT(w) != Parser.LIT)
            {
                return Parser.EVDOMAIN; // only 11!:0 supported and only literal params please.
            }
            string wdArg = CAV(w);
            if (null == wdArg || string.Empty == (wdArg = wdArg.Trim()))
            {
                return 0;
            }
            string[] argv = wdArg.Split(ws, StringSplitOptions.RemoveEmptyEntries);
            switch (argv[0])
            {
                case "wf":
                    if (!Parser.IsWdSet)
                    {
                        Parser.UseWF = true;
                        Parser.IsWdSet = true;
                    }
                    break;
                case "pc":
                    Parser.IsWdSet = true;
                    if (2 > argv.Length)
                    {
                        return Parser.EVDOMAIN; // should probably be different error, not sure which one though                    
                    }
                    mainWindowId = argv[1];
                    if (Parser.UseWF)
                    {
                        FWd.CreateParent(mainWindowId);
                    }
                    else
                    {
                        XWd.CreateParent(mainWindowId);
                    }
                    break;
                case "pshow":
                    Parser.IsWdSet = true;
                    if (Parser.UseWF)
                    {
                        FWd.ShowParent();
                    }
                    else
                    {
                        XWd.ShowParent();
                    }
                    break;
                default:
                    return Parser.EVDOMAIN; // should probably be different error, not sure which one though
            }
            return 0;
        }
    }
}