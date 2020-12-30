namespace NKit.Web.Service.CoreRest.Models
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    #endregion //Using Directives

    public class NKitLogEntryCore : NKitBaseModelCore
    {
        #region Properties

        [Required]
        [Key]
        public Guid NKitLogEntryId { get; set; }

        [Required]
        [Column(TypeName = VARCHAR_MAX)]
        public string Message { get; set; }

        [StringLength(200)]
        [Column(TypeName = VARCHAR_200)]
        public string Source { get; set; }

        [StringLength(200)]
        [Column(TypeName = VARCHAR_200)]
        public string ClassName { get; set; }

        [StringLength(200)]
        public string FunctionName { get; set; }

        [Column(TypeName = VARCHAR_MAX)]
        public string StackTrace { get; set; }

        [Required]
        [Column(TypeName = DATE_TIME)]
        public DateTime DateCreated { get; set; }

        #endregion //Properties
    }
}
