using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaMDHealth.MobileClient.Common
{
    interface ISecurityService
    {
        void BlockTapJacking(bool blockTapJacking);
        public void ControlScreenSharing(bool isEnabled);
    }
}
