using UnityEngine;

namespace PeakAdminToolkit.UI
{
    internal sealed class CursorLease
    {
        private bool acquired;
        private CursorLockMode previousLock;
        private bool previousVisibility;

        public void Acquire()
        {
            if (!acquired)
            {
                previousLock = Cursor.lockState;
                previousVisibility = Cursor.visible;
                acquired = true;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Release(bool anotherWindowNeedsCursor)
        {
            if (!acquired) return;
            acquired = false;
            if (anotherWindowNeedsCursor) return;
            Cursor.lockState = previousLock;
            Cursor.visible = previousVisibility;
        }
    }
}
