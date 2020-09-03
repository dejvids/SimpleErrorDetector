using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleDetector.Models;
using ReactiveUI;

namespace SimpleDetector
{
    public class BitStatus:ReactiveUI.ReactiveObject
    {
        private bool _value;
        private ErroCode status;
        private string label;
        public bool Value
        {
            get { return _value; }
            set { this.RaiseAndSetIfChanged(ref _value, value); }
        }
        public ErroCode Status
        {
            get { return status; }
            set { this.RaiseAndSetIfChanged(ref status, value); }
        }

        public string Label
        {
            get { return label; }
            set { label = value; this.RaiseAndSetIfChanged(ref label, value); }
        }
        public bool IsSeparator { get; set; }
    }
}
