using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EcomCourse.Application
{
    public sealed class AssemblyReference
    {
        private AssemblyReference()
        {

        }

        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}
