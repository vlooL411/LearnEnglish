//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LearnEnglish
{
    using System;
    using System.Collections.Generic;
    
    public partial class Word
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Word()
        {
            this.Story_Word = new HashSet<Story_Word>();
        }
    
        public long ID { get; set; }
        public string Text { get; set; }
        public string Translate { get; set; }
        public Nullable<byte> TypeID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Story_Word> Story_Word { get; set; }
        public virtual Type Type { get; set; }
    }
}
