//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Functions
    {
        public Functions()
        {
            this.Rolefunctions = new HashSet<Rolefunctions>();
        }
    
        public int ID { get; set; }
        public string Url { get; set; }
        public Nullable<System.DateTime> CommitTime { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
    
        public virtual ICollection<Rolefunctions> Rolefunctions { get; set; }
    }
}
