namespace NKit.Core.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.EntityFrameworkCore;
    using NKit.Web.Service.RestApi.Models;

    #endregion //Using Directives

    public class NKitDbContext : DbContext
    {
        #region Constructors

        public NKitDbContext(DbContextOptions options) : base(options)
        {
        }

        #endregion //Constructors

        #region Db Sets

        public virtual DbSet<NKitLogEntry> NKitLogEntry { get; set; }

        #endregion //Db Sets
    }
}
