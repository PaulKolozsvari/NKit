namespace NKit.Data.DB.LINQ.Models
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    #endregion //Using Directives

    public class NKitLogEntry : NKitBaseModel
    {
        #region Properties

        [Required]
        [Key]
        public Guid NKitLogEntryId { get; set; }

        [Required]
        public string Message { get; set; }

        [StringLength(200)]
        public string Source { get; set; }

        [StringLength(200)]
        public string ClassName { get; set; }

        [StringLength(200)]
        public string FunctionName { get; set; }

        public string StackTrace { get; set; }

        [Required]
        public int EventId { get; set; }

        public string EventName { get; set; }

        [Required]
        [Column(TypeName = DATE_TIME)]
        public DateTime DateCreated { get; set; }

        #endregion //Properties
    }
}
