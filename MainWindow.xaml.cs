/*
 * Project: GHOST MODE
 * Developer: Taekbly
 * License: Commercial use allowed, redistribution prohibited.
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace GhostMode
{
    public partial class MainWindow : Window
    {
        private const string RuleName = "GHOST_MODE_LOL";
        private bool isGhosting = false;
        private readonly DispatcherTimer statusTimer;

        private readonly Color ColorOnline = (Color)ColorConverter.ConvertFromString("#45A29E");
        private readonly Color ColorGhost = (Color)ColorConverter.ConvertFromString("#607D8B");

        public MainWindow()
        {
            InitializeComponent();

            CheckFirewallRule();
            UpdateUI();

            statusTimer = new DispatcherTimer();
            statusTimer.Interval = TimeSpan.FromSeconds(2);
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            CheckLeagueClient();
        }

        private void CheckFirewallRule()
        {
            string output = RunCmd($"netsh advfirewall firewall show rule name=\"{RuleName}\"");
            isGhosting = output.Contains(RuleName);
        }

        private void CheckLeagueClient()
        {
            bool isRunning = Process.GetProcessesByName("LeagueClient").Any();

            if (isRunning)
            {
                ClientStatusDot.Fill = new SolidColorBrush(Colors.LimeGreen);
                TxtClientStatus.Text = "League Client : Detected";
                TxtClientStatus.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                ClientStatusDot.Fill = new SolidColorBrush(Colors.Red);
                TxtClientStatus.Text = "League Client : Not Detected";
                TxtClientStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5C6C7"));
            }
        }

        private async void BtnGhostToggle_Click(object? sender, RoutedEventArgs e)
        {
            BtnGhostToggle.IsEnabled = false;

            await Task.Run(() =>
            {
                if (isGhosting)
                {
                    RunCmd($"netsh advfirewall firewall delete rule name=\"{RuleName}\"");
                    isGhosting = false;
                }
                else
                {
                    RunCmd($"netsh advfirewall firewall delete rule name=\"{RuleName}\"");
                    RunCmd($"netsh advfirewall firewall add rule name=\"{RuleName}\" dir=out protocol=TCP remoteport=5223 action=block");
                    isGhosting = true;
                }
            });

            UpdateUI();

            // 쿨타임: 시스템에 방화벽 규칙이 적용될 물리적 시간을 부여 (2.5초)
            await Task.Delay(2500);

            BtnGhostToggle.IsEnabled = true;
        }

        private void UpdateUI()
        {
            if (BtnGhostToggle.Template.FindName("ButtonBorder", BtnGhostToggle) is System.Windows.Controls.Border btnBorder)
            {
                if (isGhosting)
                {
                    TxtStatus.Text = "GHOSTING";
                    TxtStatus.Foreground = new SolidColorBrush(ColorGhost);
                    btnBorder.BorderBrush = new SolidColorBrush(ColorGhost);

                    if (btnBorder.Effect is DropShadowEffect glowEffect)
                    {
                        glowEffect.Color = ColorGhost;
                    }
                }
                else
                {
                    TxtStatus.Text = "ONLINE";
                    TxtStatus.Foreground = new SolidColorBrush(ColorOnline);
                    btnBorder.BorderBrush = new SolidColorBrush(ColorOnline);

                    if (btnBorder.Effect is DropShadowEffect glowEffect)
                    {
                        glowEffect.Color = ColorOnline;
                    }
                }
            }
        }

        private string RunCmd(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process? process = Process.Start(psi))
                {
                    if (process == null) return "Error";
                    using (System.IO.StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        process.WaitForExit();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}