namespace NKit.Extensions.DataBox.Events.Crud
{
    #region Delegates

    public delegate void OnBeforeRefreshFromServerWindows(object sender, BeforeRefreshFromServerArgsWindows e);
    public delegate void OnAfterRefreshFromServerWindows(object sender, AfterRefreshFromServerArgsWindows e);

    public delegate void OnBeforeGridRefreshWindows(object sender, BeforeGridRefreshArgsWindows e);
    public delegate void OnAfterGridRefreshWindows(object sender, AfterGridRefreshArgsWindows e);

    public delegate void OnBeforeAddInputControlsWindows(object sender, BeforeAddInputControlsArgsWindows e);
    public delegate void OnAfterAddInputControlsWindows(object sender, AfterAddInputControlsArgsWindows e);

    public delegate void OnBeforeCrudOperationWindows(object sender, BeforeCrudOperationArgsWindows e);
    public delegate void OnAfterCrudOperationWindows(object sender, AfterCrudOperationArgsWindows e);

    public delegate void OnBeforeSaveWindows(object sender, BeforeSaveArgsWindows e);
    public delegate void OnAfterSaveWindows(object sender, AfterSaveArgsWindows e);

    #endregion //Delegates
}