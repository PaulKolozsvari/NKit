namespace NKit.Web.Service.CoreRest.Models
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class NKitBaseModelCore
    {
        /*
         ******DATABASE ANNOTATIONS (Attributes)
         * -using System.ComponentModel.DataAnnotations;
         * [Key] - Set as Primary key
         * [DatabaseGenerated(DatabaseGeneratedOption.Identity)] - AutoIncrement Identity
         * [Required] - Non Nullable
         * [StringLength(50)] - String Length
         * 
         ******MIGRATION  COMMANDS
         * -Add migration file
         *      PM> Add-Migration *MigrationDescription*
         * -Update Database
         *      PM> Update-Database
         *  -Remove Migration 
         *      PM> Remove-Migration
         */

        #region Constants

        public const string VARCHAR_10 = "varchar(10)";
        public const string VARCHAR_20 = "varchar(20)";
        public const string VARCHAR_50 = "varchar(50)";
        public const string VARCHAR_100 = "varchar(100)";
        public const string VARCHAR_200 = "varchar(200)";
        public const string VARCHAR_250 = "varchar(250)";
        public const string VARCHAR_500 = "varchar(500)";
        public const string VARCHAR_MAX = "varchar(MAX)";
        public const string DATE_TIME = "datetime";

        #endregion //Constants
    }
}
