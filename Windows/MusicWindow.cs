﻿using System.Diagnostics;
using ToNSaveManager.Extensions;
using ToNSaveManager.Localization;
using ToNSaveManager.Models;

namespace ToNSaveManager
{
    public partial class MusicWindow : Form
    {
        internal static MusicWindow? Instance;
        static List<Music> Musics = Music.ImportFromMemory();

        public MusicWindow()
        {
            InitializeComponent();
            listBox1.FixItemHeight();
        }

        public static void Open(Form parent)
        {
            if (Instance == null || Instance.IsDisposed) Instance = new MusicWindow();

            if (Instance.Visible)
            {
                Instance.BringToFront();
                return;
            }

            Instance.StartPosition = FormStartPosition.Manual;
            Instance.Location = new Point(
                parent.Location.X + (parent.Width - Instance.Width) / 2,
                Math.Max(parent.Location.Y + (parent.Height - Instance.Height) / 2, 0)
            );
            Instance.Show(); // Don't parent
        }

        internal static void RefreshLists()
        {
            Instance?.listBox1.Refresh();
        }

        internal void LocalizeContent() {
            LANG.C(this, "MUSIC.TITLE");

            foreach (Music obj in listBox1.Items) {
                (string? tx, string? tt) = LANG.T("MUSIC." + obj.Name.ToUpperInvariant().Replace(' ', '_'));
                if (!string.IsNullOrEmpty(tx)) obj.DisplayName = tx;
                if (!string.IsNullOrEmpty(tt)) obj.Tooltip = tt;
            }

            RightToLeft = LANG.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
        }

        private void MusicWindow_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            foreach (Music ob in Musics)
                listBox1.Items.Add(ob);

            LocalizeContent();
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            ListBox listBox = (ListBox)sender;
            Music music = (Music)listBox.Items[e.Index];

            string itemText = listBox.Items[e.Index].ToString() ?? string.Empty;
            Font itemFont = new Font(listBox.Font, music.IsCompleted ? FontStyle.Strikeout : FontStyle.Regular);

            using (var backColorBrush = new SolidBrush(music.IsSeparator ? this.BackColor : listBox.BackColor))
                e.Graphics.FillRectangle(backColorBrush, e.Bounds);

            Color itemColor;

            if (music.IsSeparator) itemColor = e.ForeColor;
            else if (e.Index == MouseDownIndex) itemColor = MouseRightClick ? Color.Cyan : Color.Red;
            else itemColor = music.IsCompleted ? Color.FromArgb(122, 122, 122) : (Settings.Get.ColorfulObjectives ? music.DisplayColor : e.ForeColor);

            int maxWidth = e.Bounds.Width;
            TextRenderer.DrawText(e.Graphics, MainWindow.GetTruncatedText(itemText, itemFont, maxWidth), itemFont, e.Bounds, itemColor,
                TextFormatFlags.VerticalCenter);
        }

        private int MouseDownIndex = -1;
        private bool MouseRightClick;
        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseRightClick = e.Button == MouseButtons.Right;
            if (MouseRightClick || e.Button == MouseButtons.Left)
            {
                MouseDownIndex = listBox1.IndexFromPoint(e.Location);
                if (MouseDownIndex > 0) InvalidateItem(MouseDownIndex);
                else MouseDownIndex = -1;
            }
        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.Location);
            if (index < 0 || index >= listBox1.Items.Count || index != MouseDownIndex)
            {
                index = MouseDownIndex;
                MouseDownIndex = -1;
                if (index > -1) InvalidateItem(index);
                return;
            }

            Music music = (Music)listBox1.Items[index];
            if (music.IsSeparator) return;

            switch (e.Button)
            {
                case MouseButtons.Right:
                    MainWindow.OpenExternalLink(music.Reference);
                    break;
                case MouseButtons.Left:
                    music.Toggle();
                    // MainWindow.SaveData.Export();
                    break;

                default: break;
            }

            MouseDownIndex = -1;
            InvalidateItem(index);
        }

        private void InvalidateItem(int index)
        {
            Rectangle itemRect = listBox1.GetItemRectangle(index);
            listBox1.Invalidate(itemRect);
        }

        // Tooltips
        int PreviousTooltipIndex = -1;
        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the index of the item under the mouse pointer
            int index = listBox1.IndexFromPoint(e.Location);

            if (PreviousTooltipIndex != index)
            {
                PreviousTooltipIndex = index;

                if (index < 0)
                {
                    toolTip.SetToolTip(listBox1, null);
                    return;
                }

                Music music = (Music)listBox1.Items[index];
                toolTip.SetToolTip(listBox1, music.Tooltip ?? music.Description);
            }
        }
    }
}
