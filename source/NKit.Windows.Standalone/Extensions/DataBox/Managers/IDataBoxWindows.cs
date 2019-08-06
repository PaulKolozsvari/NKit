namespace NKit.Extensions.DataBox.Managers
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public interface IDataBoxWindows
    {
        #region Events

        event OnDataBoxPropertiesChangedWindows OnDataBoxPropertiesChanged;

        #endregion //Events

        #region Methods

        bool RefreshDataBox(bool fromServer, bool presentLoadDataBoxForm, bool refreshInputControls);

        void UpdateEntity();

        void CancelEntityUpdate();

        void PrepareForEntityUpdate();

        void AddEntity();

        void DeleteEntity();

        void Save();

        void SetFiltersEnabled(bool enable);

        void SelectEntity(Guid dataBoxEntityId);

        #endregion //Methods
    }
}