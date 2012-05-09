using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPUExample
{


    class BenchmarkCPU
    {
        public delegate void ProgressEventHandler(object sender, EventArgs e);
        public event ProgressEventHandler ProgressEvent;

        public void Start()
        {
            if (ProgressEvent != null)
            {
                ProgressEvent(this, EventArgs.Empty);
            }
        }

    }
}
