﻿
namespace Reddit.Things.API.Enums
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class Sort
    {
        public readonly string Arg;

        public static readonly Sort New = new Sort("new");
        public static readonly Sort Rising = new Sort("rising");

        private Sort (string Name)
        {
            this.Arg = Name;
        }
    }
}
