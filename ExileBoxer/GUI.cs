using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.NativeWrappers;
using Loki.Game.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExileBoxer
{
    public partial class GUI : Form
    {
        private static Color good = Color.LightGreen;
        private static Color bad = Color.OrangeRed;
        private static Color text = SystemColors.ControlText;
        private static Color neutral = SystemColors.Window;
        private static Color transparent = Color.Transparent;

        public GUI()
        {
            InitializeComponent();
        }

        public void OnInit()
        {
            //let this handle settings later pls!
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            numericUpDown1.Value = 50;
            numericUpDown2.Value = 63;
            numericUpDown3.Value = 35;
            tabControl1.SelectedIndex = 0;
        }

        public void OnPulse()
        {
            using (LokiPoe.AcquireFrame())
            {
                LokiPoe.ObjectManager.ClearCache();

                #region Info -> Leader
                if (LokiPoe.ObjectManager.Me.PartyStatus == PartyStatus.PartyMember)
                {
                    groupBox4.BackColor = neutral;

                    textBox1.Text = TheVariables.nameLeader;
                    textBox2.Text = TheVariables.distanceLeader.ToString();
                    textBox3.Text = TheVariables.areaNameLeader + " (" + TheVariables.areaIdLeader + ")";
                    textBox4.Text = TheVariables.posLeader.ToString();
                    textBox5.Text = TheVariables.posMe.ToString();

                    label17.Text = ">> " + TheVariables.townIdLeader;

                    #region WP controls
                    if (TheVariables.townIdLeader != TheVariables.townIdMe)
                        button5.Enabled = true;
                    else
                        button5.Enabled = false;
                    #endregion
                    #region TP controls
                    if (ExileBoxer.portalFromTownToArea != null || ExileBoxer.portalFromAreaToTown != null)
                        button3.Enabled = true;
                    else
                        button3.Enabled = false;

                    if (ExileBoxer.haveTPScrolls && !LokiPoe.ObjectManager.Me.IsInTown)
                        button4.Enabled = true;
                    else
                        button4.Enabled = false;

                    #endregion
                    #region Area Transition controls
                    foreach(AreaTransition a in TheVariables.availableAreaTransitions)
                    {
                        if (comboBox1.Items.Contains(a.Name))
                            return;

                        comboBox1.Items.Add(a.Name);
                    }
                    foreach(var a in comboBox1.Items)
                    {
                        bool xd = false;

                        foreach(AreaTransition b in TheVariables.availableAreaTransitions)
                        {
                            if (a == b.Name)
                                xd = true;
                        }

                        if (!xd)
                            comboBox1.Items.Remove(a);
                    }

                    if (comboBox1.Items.Count > 0)
                        button6.Enabled = true;

                    #endregion
                }
                else
                {
                    groupBox4.BackColor = transparent;

                    textBox1.Text = "--";
                    textBox2.Text = "--";
                    textBox3.Text = "--";
                    textBox4.Text = "--";
                    button3.Enabled = false;
                    button4.Enabled = false;
                    button5.Enabled = false;
                    button6.Enabled = false;
                }
                #endregion

                #region Accept/Leave/List
                if (LokiPoe.InstanceInfo.PendingPartyInvites.Count() < 1 && LokiPoe.InstanceInfo.PartyStatus == PartyStatus.None)
                {
                    button1.Enabled = false;
                    listBox1.Enabled = false;
                    button2.Enabled = false;
                    listBox1.Items.Clear();
                }
                if(LokiPoe.InstanceInfo.PartyStatus == PartyStatus.PartyMember || LokiPoe.InstanceInfo.PartyStatus == PartyStatus.PartyLeader)
                {
                    button1.Enabled = false;
                    listBox1.Enabled = false;
                    button2.Enabled = true;
                    listBox1.Items.Clear();
                }
                if(LokiPoe.InstanceInfo.PendingPartyInvites.Count() > 0 && LokiPoe.InstanceInfo.PartyStatus == PartyStatus.None)
                {
                    listBox1.Enabled = true;
                    button2.Enabled = false;
                }

                if (listBox1.Enabled)
                {
                    button1.Enabled = true;

                    foreach (PartyInvite pi in LokiPoe.InstanceInfo.PendingPartyInvites)
                    {
                        if (listBox1.Items.Contains(pi.CreatorAccountName))
                            return;

                        listBox1.Items.Add(pi.CreatorAccountName);
                    }
                }
                #endregion

                #region Info -> Variables -> Colors

                if (TheVariables.takePortalFromTownToArea != null)
                    label6.ForeColor = good;
                else
                    label6.ForeColor = bad;

                if (TheVariables.takePortalFromAreaToTown != null)
                    label5.ForeColor = good;
                else
                    label5.ForeColor = bad;

                if (TheVariables.targetTown.Length > 1)
                    label18.ForeColor = good;
                else
                    label18.ForeColor = bad;

                if (ExileBoxer.getWaypointOfCurrentArea() != null)
                    label19.ForeColor = good;
                else
                    label19.ForeColor = bad;

                if (TheVariables.moveToMiddleOfTown)
                    label20.ForeColor = good;
                else
                    label20.ForeColor = bad;

                if (TheVariables.portalFromTownToArea != null)
                    label9.ForeColor = good;
                else
                    label9.ForeColor = bad;

                if (TheVariables.portalFromAreaToTown != null)
                    label4.ForeColor = good;
                else
                    label4.ForeColor = bad;

                if (TheVariables.inTownMe)
                    label10.ForeColor = good;
                else
                    label10.ForeColor = bad;

                if (TheVariables.inTownLeader)
                    label11.ForeColor = good;
                else
                    label11.ForeColor = bad;

                if (TheVariables.takeAreaTransition.Length > 1)
                    label21.ForeColor = good;
                else
                    label21.ForeColor = bad;

                if (TheVariables.temp)
                    label22.ForeColor = good;
                else
                    label22.ForeColor = bad;

                #endregion

                #region Variables -> TheVariables
                TheVariables.numUpDown1 = (int)numericUpDown1.Value;
                TheVariables.numUpDown2 = (int)numericUpDown2.Value;
                TheVariables.numUpDown3 = (int)numericUpDown3.Value;
                TheVariables.checkBox1 = checkBox1.Checked;
                TheVariables.checkBox2 = checkBox2.Checked;
                #endregion
            }
        }

        #region ButtonX_Click()
        private void button1_Click(object sender, EventArgs e)
        {
            TheVariables.acceptPartyInviteFrom = listBox1.SelectedItem.ToString();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            TheVariables.leaveParty = true;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (TheVariables.inTownMe)
            {
                TheVariables.takePortalFromTownToArea = ExileBoxer.portalFromTownToArea;
            }
            
            if (!TheVariables.inTownMe)
            {
                TheVariables.takePortalFromAreaToTown = ExileBoxer.portalFromAreaToTown;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            TheVariables.makePortal = true;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            TheVariables.targetTown = TheVariables.townIdLeader;
        }
        #endregion

        private void button6_Click(object sender, EventArgs e)
        {
            using (LokiPoe.AcquireFrame())
            {
                TheVariables.takeAreaTransition = comboBox1.SelectedItem.ToString(); //LokiPoe.ObjectManager.AreaTransition(comboBox1.SelectedItem.ToString());
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (LokiPoe.AcquireFrame())
            {
                TheVariables.takeAreaTransition = string.Empty;
            }
        }

        //LokiPoe.InstanceInfo.PartyMembers.First(x => x.MemberStatus == PartyStatus.PartyLeader).PlayerEntry.

        
    }
}
