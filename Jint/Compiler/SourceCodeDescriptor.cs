﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.Compiler
{
    [Serializable]
    internal class SourceCodeDescriptor
    {
        [Serializable]
        public class Location
        {
            public Location(int line, int c)
            {
                Line = line;
                Char = c;
            }

            public int Line { get; set; }
            public int Char { get; set; }
        }

        public Location Start { get; set; }
        public Location Stop { get; set; }

        public SourceCodeDescriptor(int startLine, int startChar, int stopLine, int stopChar)
        {
            Start = new Location(startLine, startChar);
            Stop = new Location(stopLine, stopChar);
        }

        public override string ToString()
        {
            return "Line: " + Start.Line + " Char: " + Start.Char;
        }
    }
}