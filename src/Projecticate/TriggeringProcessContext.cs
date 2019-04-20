using System;
using System.Diagnostics;

namespace Projecticate
{
    public class TriggeringProcessContext
    {
        private Lazy<Process> _process;

        public int ProcessId { get; }
        public string ImageFileName { get; }
        public Process Process => _process.Value;

        public TriggeringProcessContext(int processId, string imageFileName)
        {
            ProcessId = processId;
            ImageFileName = imageFileName;
            _process = new Lazy<Process>(() => Process.GetProcessById(processId));
        }
    }
}
