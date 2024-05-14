﻿using SAC9.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAC9.Parser
{


    public record Node
    {
        public int[] Members { get; set; } = new int[2];
        public string Type { get; set; } = null!;
        public List<Node> Childs { get; } = new List<Node>();

    }
    public record NoTermReturn
    {
        public int rightIndex { get; set; }
        public string error;
        public Node? node { get; set; }
    }
}