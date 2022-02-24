using System;

namespace PnFDesktop.Classes
{
    public static class LayoutNotificationMessages
    {
        /// <summary>
        /// Identify GetWorkspaceLayout notification by a GUID stored in a string.
        /// 
        /// Both, view and viewmodel register for a notification from MVVM Light and
        /// filter out the save workspace layout notification based on this GUID Id.
        /// </summary>
        public static readonly string GetWorkspaceLayout = Guid.NewGuid().ToString();

        /// <summary>
        /// Identify LoadWorkspaceLayout notification by a GUID stored in a string.
        /// 
        /// Both, view and viewmodel register for a notification from MVVM Light and
        /// filter out the load workspace layout notification based on this GUID Id.
        /// </summary>
        public static readonly string LoadWorkspaceLayout = Guid.NewGuid().ToString();

        public static readonly string SaveWorkspaceLayout = Guid.NewGuid().ToString();

    }
}
