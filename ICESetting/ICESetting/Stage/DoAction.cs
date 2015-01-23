using ICESetting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using UserMessageBox;

namespace ICESetting.Stage
{
    public partial class SettingStage
    {
        MessageWin CurrentMessageWin;
        public static bool isOrdersValid = true;
        public void DealCmd(string cmd)
        {
            if (!isReadyReboot  && isOrdersValid)
            {
                Msg msg = Utility.ConvertFromStringToMsg(cmd);
                DealMsg(msg);
            }
        }
        public static void OrdersIntervial(double intervialSceond)
        {
            isOrdersValid = false;
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(intervialSceond);
            dt.Tick += delegate
            {
                isOrdersValid = true;
                dt.Stop();
            };
            dt.Start();
        }
    }
}
