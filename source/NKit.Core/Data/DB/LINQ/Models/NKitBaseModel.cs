namespace NKit.Data.DB.LINQ.Models
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class NKitBaseModel
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

        public const string SQL_SERVER_VARCHAR_10 = "varchar(10)";
        public const string SQL_SERVER_VARCHAR_20 = "varchar(20)";
        public const string SQL_SERVER_VARCHAR_50 = "varchar(50)";
        public const string SQL_SERVER_VARCHAR_100 = "varchar(100)";
        public const string SQL_SERVER_VARCHAR_200 = "varchar(200)";
        public const string SQL_SERVER_VARCHAR_250 = "varchar(250)";
        public const string SQL_SERVER_VARCHAR_500 = "varchar(500)";
        public const string SQL_SERVER_VARCHAR_MAX = "varchar(MAX)";
        public const string SQL_SERVER_DATE_TIME = "datetime";
        public const string SQL_SERVER_VAR_BINARY_MAX = "varbinary(MAX)";
        public const string SQL_SERVER_IMAGE = "image";

        public const string NVARCHAR_10 = "nvarchar(10)";
        public const string NVARCHAR_20 = "nvarchar(20)";
        public const string NVARCHAR_50 = "nvarchar(50)";
        public const string NVARCHAR_100 = "nvarchar(100)";
        public const string NVARCHAR_200 = "nvarchar(200)";
        public const string NVARCHAR_250 = "nvarchar(250)";
        public const string NVARCHAR_500 = "nvarchar(500)";
        public const string NVARCHAR_2048 = "nvarchar(2048)";

        public const string DATE_TIME = "datetime";

        #endregion //Constants
    }
}