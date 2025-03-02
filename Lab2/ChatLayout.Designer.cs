using System.ComponentModel;

namespace Lab2;

partial class ChatLayout
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        p_chat = new System.Windows.Forms.Panel();
        p_topBar = new System.Windows.Forms.Panel();
        tb_dialog = new System.Windows.Forms.TextBox();
        p_bottomBar = new System.Windows.Forms.Panel();
        btn_send = new System.Windows.Forms.Button();
        tb_message = new System.Windows.Forms.TextBox();
        p_rightBar = new System.Windows.Forms.Panel();
        p_otherUsers = new System.Windows.Forms.Panel();
        lb_users = new System.Windows.Forms.Label();
        p_leaveButton = new System.Windows.Forms.Panel();
        btn_leave = new System.Windows.Forms.Button();
        p_userInfo = new System.Windows.Forms.Panel();
        lb_info = new System.Windows.Forms.Label();
        p_chat.SuspendLayout();
        p_topBar.SuspendLayout();
        p_bottomBar.SuspendLayout();
        p_rightBar.SuspendLayout();
        p_otherUsers.SuspendLayout();
        p_leaveButton.SuspendLayout();
        p_userInfo.SuspendLayout();
        SuspendLayout();
        // 
        // p_chat
        // 
        p_chat.BackColor = System.Drawing.SystemColors.ButtonShadow;
        p_chat.Controls.Add(p_topBar);
        p_chat.Controls.Add(p_bottomBar);
        p_chat.Controls.Add(p_rightBar);
        p_chat.Dock = System.Windows.Forms.DockStyle.Fill;
        p_chat.Location = new System.Drawing.Point(0, 0);
        p_chat.Name = "p_chat";
        p_chat.Size = new System.Drawing.Size(510, 480);
        p_chat.TabIndex = 0;
        // 
        // p_topBar
        // 
        p_topBar.Controls.Add(tb_dialog);
        p_topBar.Dock = System.Windows.Forms.DockStyle.Fill;
        p_topBar.Location = new System.Drawing.Point(0, 0);
        p_topBar.Margin = new System.Windows.Forms.Padding(0);
        p_topBar.Name = "p_topBar";
        p_topBar.Size = new System.Drawing.Size(331, 403);
        p_topBar.TabIndex = 2;
        // 
        // tb_dialog
        // 
        tb_dialog.Dock = System.Windows.Forms.DockStyle.Fill;
        tb_dialog.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        tb_dialog.Location = new System.Drawing.Point(0, 0);
        tb_dialog.Multiline = true;
        tb_dialog.Name = "tb_dialog";
        tb_dialog.ReadOnly = true;
        tb_dialog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        tb_dialog.Size = new System.Drawing.Size(331, 403);
        tb_dialog.TabIndex = 0;
        // 
        // p_bottomBar
        // 
        p_bottomBar.BackColor = System.Drawing.SystemColors.AppWorkspace;
        p_bottomBar.Controls.Add(btn_send);
        p_bottomBar.Controls.Add(tb_message);
        p_bottomBar.Dock = System.Windows.Forms.DockStyle.Bottom;
        p_bottomBar.Location = new System.Drawing.Point(0, 403);
        p_bottomBar.Margin = new System.Windows.Forms.Padding(0);
        p_bottomBar.Name = "p_bottomBar";
        p_bottomBar.Size = new System.Drawing.Size(331, 77);
        p_bottomBar.TabIndex = 1;
        // 
        // btn_send
        // 
        btn_send.Anchor = System.Windows.Forms.AnchorStyles.None;
        btn_send.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        btn_send.Location = new System.Drawing.Point(265, 11);
        btn_send.Name = "btn_send";
        btn_send.Size = new System.Drawing.Size(52, 55);
        btn_send.TabIndex = 1;
        btn_send.Text = "send";
        btn_send.UseVisualStyleBackColor = true;
        btn_send.Click += btn_send_Click;
        // 
        // tb_message
        // 
        tb_message.Anchor = System.Windows.Forms.AnchorStyles.None;
        tb_message.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        tb_message.Location = new System.Drawing.Point(9, 10);
        tb_message.Multiline = true;
        tb_message.Name = "tb_message";
        tb_message.Size = new System.Drawing.Size(250, 57);
        tb_message.TabIndex = 0;
        tb_message.KeyDown += tb_message_KeyDown;
        // 
        // p_rightBar
        // 
        p_rightBar.BackColor = System.Drawing.SystemColors.ControlDarkDark;
        p_rightBar.Controls.Add(p_otherUsers);
        p_rightBar.Controls.Add(p_leaveButton);
        p_rightBar.Controls.Add(p_userInfo);
        p_rightBar.Dock = System.Windows.Forms.DockStyle.Right;
        p_rightBar.Location = new System.Drawing.Point(331, 0);
        p_rightBar.Margin = new System.Windows.Forms.Padding(0);
        p_rightBar.Name = "p_rightBar";
        p_rightBar.Size = new System.Drawing.Size(179, 480);
        p_rightBar.TabIndex = 0;
        // 
        // p_otherUsers
        // 
        p_otherUsers.BackColor = System.Drawing.Color.Gainsboro;
        p_otherUsers.Controls.Add(lb_users);
        p_otherUsers.Dock = System.Windows.Forms.DockStyle.Fill;
        p_otherUsers.Location = new System.Drawing.Point(0, 95);
        p_otherUsers.Margin = new System.Windows.Forms.Padding(0);
        p_otherUsers.Name = "p_otherUsers";
        p_otherUsers.Size = new System.Drawing.Size(179, 328);
        p_otherUsers.TabIndex = 2;
        // 
        // lb_users
        // 
        lb_users.Dock = System.Windows.Forms.DockStyle.Fill;
        lb_users.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        lb_users.Location = new System.Drawing.Point(0, 0);
        lb_users.Name = "lb_users";
        lb_users.Size = new System.Drawing.Size(179, 328);
        lb_users.TabIndex = 0;
        lb_users.Text = "Пользователи:";
        lb_users.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // p_leaveButton
        // 
        p_leaveButton.Controls.Add(btn_leave);
        p_leaveButton.Dock = System.Windows.Forms.DockStyle.Bottom;
        p_leaveButton.Location = new System.Drawing.Point(0, 423);
        p_leaveButton.Margin = new System.Windows.Forms.Padding(0);
        p_leaveButton.Name = "p_leaveButton";
        p_leaveButton.Size = new System.Drawing.Size(179, 57);
        p_leaveButton.TabIndex = 1;
        // 
        // btn_leave
        // 
        btn_leave.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        btn_leave.Location = new System.Drawing.Point(15, 11);
        btn_leave.Name = "btn_leave";
        btn_leave.Size = new System.Drawing.Size(145, 35);
        btn_leave.TabIndex = 0;
        btn_leave.Text = "leave";
        btn_leave.UseVisualStyleBackColor = true;
        btn_leave.Click += btn_leave_Click;
        // 
        // p_userInfo
        // 
        p_userInfo.BackColor = System.Drawing.Color.Silver;
        p_userInfo.Controls.Add(lb_info);
        p_userInfo.Dock = System.Windows.Forms.DockStyle.Top;
        p_userInfo.Location = new System.Drawing.Point(0, 0);
        p_userInfo.Margin = new System.Windows.Forms.Padding(0);
        p_userInfo.Name = "p_userInfo";
        p_userInfo.Size = new System.Drawing.Size(179, 95);
        p_userInfo.TabIndex = 0;
        // 
        // lb_info
        // 
        lb_info.Dock = System.Windows.Forms.DockStyle.Fill;
        lb_info.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        lb_info.Location = new System.Drawing.Point(0, 0);
        lb_info.Name = "lb_info";
        lb_info.Size = new System.Drawing.Size(179, 95);
        lb_info.TabIndex = 0;
        lb_info.Text = "Вы вошли как:";
        lb_info.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // ChatLayout
        // 
        BackColor = System.Drawing.SystemColors.ButtonHighlight;
        Controls.Add(p_chat);
        MinimumSize = new System.Drawing.Size(510, 480);
        Size = new System.Drawing.Size(510, 480);
        Load += ChatLayout_Load;
        p_chat.ResumeLayout(false);
        p_topBar.ResumeLayout(false);
        p_topBar.PerformLayout();
        p_bottomBar.ResumeLayout(false);
        p_bottomBar.PerformLayout();
        p_rightBar.ResumeLayout(false);
        p_otherUsers.ResumeLayout(false);
        p_leaveButton.ResumeLayout(false);
        p_userInfo.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Button btn_leave;

    private System.Windows.Forms.Button btn_send;

    private System.Windows.Forms.TextBox tb_message;

    private System.Windows.Forms.TextBox tb_dialog;

    private System.Windows.Forms.Label lb_users;

    private System.Windows.Forms.Label lb_info;

    private System.Windows.Forms.Panel p_otherUsers;

    private System.Windows.Forms.Panel p_leaveButton;

    private System.Windows.Forms.Panel p_topBar;

    private System.Windows.Forms.Panel p_userInfo;

    private System.Windows.Forms.Panel p_rightBar;

    private System.Windows.Forms.Panel p_chat;

    private System.Windows.Forms.Panel p_bottomBar;

    #endregion
}