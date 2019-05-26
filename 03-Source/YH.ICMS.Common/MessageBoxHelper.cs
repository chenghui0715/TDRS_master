using System.Windows.Forms;

namespace YH.ICMS.Common
{

    public  class MessageBoxHelper
    {
        public static DialogResult ShowConfirmMessageBox(string message)
        {
            return MessageBox.Show(message, "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        }

        public static DialogResult ShowInformationMessageBox(string message)
        {
            return MessageBox.Show(message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
