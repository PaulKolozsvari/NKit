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
        //To ensure a model has string fields set to varchar instead nvarchar: modelBuilder.Properties<string>().Configure(c => c.HasColumnType("varchar"));
        //https://stackoverflow.com/questions/20961869/configure-ef6-to-use-varchar-as-default-instead-of-nvarchar

        #region Properties

        [Required]
        [Key]
        public Guid NKitLogEntryId { get; set; }

        [Required]
        [Column(TypeName = SQL_SERVER_VARCHAR_MAX)]
        public string Message { get; set; }

        [StringLength(200)]
        public string Source { get; set; }

        [StringLength(200)]
        public string ClassName { get; set; }

        [StringLength(200)]
        public string FunctionName { get; set; }

        [Column(TypeName = SQL_SERVER_VARCHAR_MAX)]
        public string StackTrace { get; set; }

        [Required]
        public int EventId { get; set; }

        [StringLength(200)]
        public string EventName { get; set; }

        [Required]
        [Column(TypeName = DATE_TIME)]
        public DateTime DateCreated { get; set; }

        #endregion //Properties
    }
}
