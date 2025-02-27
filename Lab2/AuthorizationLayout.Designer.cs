using System.ComponentModel;

namespace Lab2;

partial class AuthorizationLayout
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
        panel_authorization = new System.Windows.Forms.Panel();
        btn_accept = new System.Windows.Forms.Button();
        lb_title = new System.Windows.Forms.Label();
        tb_username = new System.Windows.Forms.TextBox();
        lb_username = new System.Windows.Forms.Label();
        panel_authorization.SuspendLayout();
        SuspendLayout();
        // 
        // panel_authorization
        // 
        panel_authorization.BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
        panel_authorization.Controls.Add(btn_accept);
        panel_authorization.Controls.Add(lb_title);
        panel_authorization.Controls.Add(tb_username);
        panel_authorization.Controls.Add(lb_username);
        panel_authorization.Dock = System.Windows.Forms.DockStyle.Fill;
        panel_authorization.Location = new System.Drawing.Point(0, 0);
        panel_authorization.Margin = new System.Windows.Forms.Padding(0);
        panel_authorization.MinimumSize = new System.Drawing.Size(225, 254);
        panel_authorization.Name = "panel_authorization";
        panel_authorization.Size = new System.Drawing.Size(225, 254);
        panel_authorization.TabIndex = 0;
        // 
        // btn_accept
        // 
        btn_accept.Anchor = System.Windows.Forms.AnchorStyles.None;
        btn_accept.BackColor = System.Drawing.Color.DimGray;
        btn_accept.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        btn_accept.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
        btn_accept.FlatAppearance.BorderSize = 0;
        btn_accept.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        btn_accept.ForeColor = System.Drawing.Color.White;
        btn_accept.Location = new System.Drawing.Point(48, 161);
        btn_accept.Margin = new System.Windows.Forms.Padding(0);
        btn_accept.Name = "btn_accept";
        btn_accept.Size = new System.Drawing.Size(136, 26);
        btn_accept.TabIndex = 10;
        btn_accept.Text = "Подтвердить";
        btn_accept.UseVisualStyleBackColor = false;
        btn_accept.Click += btn_accept_Click;
        // 
        // lb_title
        // 
        lb_title.Anchor = System.Windows.Forms.AnchorStyles.None;
        lb_title.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        lb_title.ForeColor = System.Drawing.Color.White;
        lb_title.Location = new System.Drawing.Point(-11, -14);
        lb_title.Name = "lb_title";
        lb_title.Size = new System.Drawing.Size(247, 75);
        lb_title.TabIndex = 9;
        lb_title.Text = "Авторизация";
        lb_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // tb_username
        // 
        tb_username.Anchor = System.Windows.Forms.AnchorStyles.None;
        tb_username.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        tb_username.Location = new System.Drawing.Point(50, 112);
        tb_username.Name = "tb_username";
        tb_username.PlaceholderText = "пользователь";
        tb_username.Size = new System.Drawing.Size(129, 25);
        tb_username.TabIndex = 5;
        // 
        // lb_username
        // 
        lb_username.Anchor = System.Windows.Forms.AnchorStyles.None;
        lb_username.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        lb_username.ForeColor = System.Drawing.Color.White;
        lb_username.Location = new System.Drawing.Point(-11, 80);
        lb_username.Margin = new System.Windows.Forms.Padding(0);
        lb_username.Name = "lb_username";
        lb_username.Size = new System.Drawing.Size(247, 29);
        lb_username.TabIndex = 6;
        lb_username.Text = "Имя пользователя";
        lb_username.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // AuthorizationLayout
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.White;
        Controls.Add(panel_authorization);
        Margin = new System.Windows.Forms.Padding(0);
        MinimumSize = new System.Drawing.Size(225, 254);
        Size = new System.Drawing.Size(225, 254);
        panel_authorization.ResumeLayout(false);
        panel_authorization.PerformLayout();
        ResumeLayout(false);
    }

    private System.Windows.Forms.Button btn_accept;

    private System.Windows.Forms.Label lb_title;
    private System.Windows.Forms.Label lb_username;
    private System.Windows.Forms.TextBox tb_username;

    private System.Windows.Forms.Panel panel_authorization;

    #endregion
}