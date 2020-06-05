using System;
using System.Collections.Generic;

namespace AssemblyDebugger
{
    [Serializable]
    internal class CompilationReport
    {
        public CompilationReport()
        {
            assemblyCompilations = new List<AssemblyCompilation>();
        }

        public double compilationTotalTime;
        public List<AssemblyCompilation> assemblyCompilations;
        public long reloadEventTimes;
    }

    [Serializable]
    internal class AssemblyCompilation
    {
        public AssemblyCompilation(string assemblyName, double compilationTime)
        {
            this.assemblyName = assemblyName;
            this.compilationTime = compilationTime;
        }

        public double compilationTime;
        public string assemblyName;
    }
}