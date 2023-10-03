using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrollment
    {
        public uint EId { get; set; }
        public uint Student { get; set; }
        public uint Class { get; set; }
        public string Grade { get; set; } = null!;

        public virtual Class ClassNavigation { get; set; } = null!;
        public virtual Student StudentNavigation { get; set; } = null!;
    }
}
