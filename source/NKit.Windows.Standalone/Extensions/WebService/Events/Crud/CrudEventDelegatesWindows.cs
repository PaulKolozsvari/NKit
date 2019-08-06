namespace NKit.Extensions.WebService.Events.Crud
{
    #region Delegates

    public delegate void OnBeforeWebGetSqlTableWindows(object sender, BeforeWebGetSqlTableArgsWindows e);
    public delegate void OnAfterWebGetSqlTableWindows(object sender, AfterWebGetSqlTableArgsWindows e);
    public delegate void OnBeforeWebInvokeSqlTableWindows(object sender, BeforeWebInvokeSqlTableArgsWindows e);
    public delegate void OnAfterWebInvokeSqlTableWindows(object sender, AfterWebInvokeSqlTableArgsWindows e);
    public delegate void OnBeforeWebInvokeSqlWindows(object sender, BeforeWebInvokeSqlArgsWindows e);
    public delegate void OnAfterWebInvokeSqlWindows(object sender, AfterWebInvokeSqlArgsWindows e);

    #endregion //Delegates
}
