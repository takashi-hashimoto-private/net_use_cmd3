using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace net_use_cmd3
{
    public partial class frmMain : Form
    {

        private int cnt;
        private List<string> OldUserId;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            bool result = InitialCheck();

            if (result == true)
            {
                string _massage = "既に接続されています。" + Environment.NewLine + "それでも接続できない場合は、" + Environment.NewLine + "ITシステム課へご連絡ください。";
                MessageBox.Show(_massage, "ファイルサーバー接続コマンド実行");
                this.Close();
            }

            string[] UserIdList = null;
            OldUserId = new List<string>();

            cnt = 0;

            CsvRead(ref UserIdList);

            string NewUserId = Environment.UserName;
            txtNewUserID.Text = NewUserId;

            OldUserId = GetOldUserId(NewUserId, UserIdList);

            if (OldUserId.Count == 0)
            {

                string _massage = "新ドメインログインユーザーID： " + txtNewUserID.Text + Environment.NewLine;
                _massage += "旧ドメインのユーザーIDがリスに存在しません。" + Environment.NewLine + "ITシステム課へご連絡ください。";
                MessageBox.Show(_massage, "ファイルサーバー接続コマンド実行");
                this.Close();
            }

            SetOldUserID();
            txtPassword.Select();

        }

        private void CsvRead(ref string[] _UserIdList)
        {
            string AppPath = Directory.GetCurrentDirectory();
            string filePath = AppPath + @"\UserID.csv";

            string text = File.ReadAllText(filePath, Encoding.GetEncoding("shift_JIS"));
            string[] rows = text.Trim().Replace("\r", "").Split('\n');

            _UserIdList = rows;
        }

        List<string> GetOldUserId(string _UserName, string[] _UserIdList)
        {
            List<string> _OldUserId = new List<string>();

            try
            {
                int i = 0;

                foreach (string row in _UserIdList)
                {
                    string[] cols = row.Split(',');

                    if (cols[0].ToString() == _UserName)
                    {
                        _OldUserId.Add(cols[1].ToString());
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return _OldUserId;
        }

        private void SetOldUserID()
        {
            if (cnt < OldUserId.Count)
            {
                cboOldUserID.Items.AddRange(OldUserId.ToArray());
                cboOldUserID.SelectedIndex = 0;
            }
            else
            {
                this.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            btnClose.Enabled = false;

            bool result = net_use_cmd();

            if (result == false)
            {
                cnt++;
                btnConnect.Enabled = true;
                btnClose.Enabled = true;
                txtPassword.Text = string.Empty;
                txtPassword.Select();
            }
            else
            {
                Application.Exit();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                btnConnect.PerformClick();
            }
        }

        bool net_use_cmd()
        {
            if (net_use_cmd_singo05() == false)
            {
                return false;
            }

            if (net_use_cmd_singo06() == false)
            {
                return false;
            }

            if (net_use_cmd_singo21() == false)
            {
                return false;
            }

            MessageBox.Show("正常に終了しました。");
            return true;
        }

        bool InitialCheck()
        {
            try
            {
                System.Diagnostics.Process open = new System.Diagnostics.Process();
                open.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");

                open.StartInfo.UseShellExecute = false;
                open.StartInfo.RedirectStandardOutput = true;
                open.StartInfo.RedirectStandardError = true;
                open.StartInfo.RedirectStandardInput = false;
                open.StartInfo.CreateNoWindow = true;
                open.StartInfo.Arguments = "/c";
                open.StartInfo.Arguments += @"net use";

                open.Start();

                string results = open.StandardOutput.ReadToEnd();
                string err_results = open.StandardError.ReadToEnd();

                open.WaitForExit();
                open.Close();

                if (err_results == null || err_results == string.Empty)
                {
                    if (results.Contains("新しい接続は記憶されます。"))
                    {

                        if (results.Contains("一覧にエントリが存在しません。"))
                        {
                            return false;
                        }
                        else
                        {
                            if (results.Contains(@"\\singo06") && results.Contains(@"\\singo05") && results.Contains(@"\\singo21"))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show(err_results);
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        bool net_use_cmd_singo05()
        {
            try
            {
                System.Diagnostics.Process open_singo05 = new System.Diagnostics.Process();
                open_singo05.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");

                open_singo05.StartInfo.UseShellExecute = false;
                open_singo05.StartInfo.RedirectStandardOutput = true;
                open_singo05.StartInfo.RedirectStandardError = true;
                open_singo05.StartInfo.RedirectStandardInput = false;
                open_singo05.StartInfo.CreateNoWindow = true;
                               
                open_singo05.StartInfo.Arguments = "/c";

                if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    open_singo05.StartInfo.Arguments += @"net use \\singo05 """" /user:" + cboOldUserID.Text + "@singo-dom";
                }
                else
                {
                    open_singo05.StartInfo.Arguments += @"net use \\singo05 " + txtPassword.Text + " /user:" + cboOldUserID.Text + "@singo-dom";
                }

                open_singo05.Start();

                string results = open_singo05.StandardOutput.ReadToEnd();
                string err_results = open_singo05.StandardError.ReadToEnd();

                results = results.Replace(Environment.NewLine, "");

                open_singo05.WaitForExit();
                open_singo05.Close();

                if (err_results == null || err_results == string.Empty)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show(err_results);
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        bool net_use_cmd_singo06()
        {
            try
            {
                System.Diagnostics.Process open_singo06 = new System.Diagnostics.Process();
                open_singo06.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");

                open_singo06.StartInfo.UseShellExecute = false;
                open_singo06.StartInfo.RedirectStandardOutput = true;
                open_singo06.StartInfo.RedirectStandardError = true;
                open_singo06.StartInfo.RedirectStandardInput = false;
                open_singo06.StartInfo.CreateNoWindow = true;

                open_singo06.StartInfo.Arguments = "/c"; 

                if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    open_singo06.StartInfo.Arguments += @"net use \\singo06 """" /user:" + cboOldUserID.Text + "@singo-dom";
                }
                else
                {
                    open_singo06.StartInfo.Arguments += @"net use \\singo06 " + txtPassword.Text + " /user:" + cboOldUserID.Text + "@singo-dom";
                }
                
                open_singo06.Start();

                string results = open_singo06.StandardOutput.ReadToEnd();
                string err_results = open_singo06.StandardError.ReadToEnd();

                results = results.Replace(Environment.NewLine, "");
                err_results = err_results.Replace(Environment.NewLine, "");

                open_singo06.WaitForExit();
                open_singo06.Close();

                if (err_results == null || err_results == string.Empty)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show(err_results);
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        bool net_use_cmd_singo21()
        {
            try
            {
                System.Diagnostics.Process open_singo21 = new System.Diagnostics.Process();
                open_singo21.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");

                open_singo21.StartInfo.UseShellExecute = false;
                open_singo21.StartInfo.RedirectStandardOutput = true;
                open_singo21.StartInfo.RedirectStandardError = true;
                open_singo21.StartInfo.RedirectStandardInput = false;
                open_singo21.StartInfo.CreateNoWindow = true;

                open_singo21.StartInfo.Arguments = "/c";

                if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    open_singo21.StartInfo.Arguments += @"net use \\singo21 """" /user:" + cboOldUserID.Text + "@singo-dom";
                }
                else
                {
                    open_singo21.StartInfo.Arguments += @"net use \\singo21 " + txtPassword.Text + " /user:" + cboOldUserID.Text + "@singo-dom";
                }

                open_singo21.Start();

                string results = open_singo21.StandardOutput.ReadToEnd();
                string err_results = open_singo21.StandardError.ReadToEnd();

                results = results.Replace(Environment.NewLine, "");
                err_results = err_results.Replace(Environment.NewLine, "");

                open_singo21.WaitForExit();
                open_singo21.Close();

                if (err_results == null || err_results == string.Empty)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show(err_results);
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
