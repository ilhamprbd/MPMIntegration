//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MPMIntegration
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_participant_status
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_participant_status()
        {
            this.tbl_participant_list = new HashSet<tbl_participant_list>();
        }
    
        public int id { get; set; }
        public string status_desc { get; set; }
        public System.DateTime create_date { get; set; }
        public string create_id { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_participant_list> tbl_participant_list { get; set; }
    }
}
