//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TDMUOJ.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Follower
    {
        public int id { get; set; }
        public int userId { get; set; }
        public int followerId { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual Account Account1 { get; set; }
    }
}