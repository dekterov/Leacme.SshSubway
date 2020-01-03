// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Leacme.Lib.SshSubway;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Leacme.App.SshSubway {

	public class AppUI {

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library lib = new Library();
		private SshClient client;
		private ShellStream ss;

		public AppUI() {

			rootPan.Spacing = 6;

			var blurb1 = App.TextBlock;
			blurb1.TextAlignment = TextAlignment.Center;
			blurb1.Text = "Connect to an SSH server to access its shell";

			var partcPanel = App.HorizontalStackPanel;
			partcPanel.HorizontalAlignment = HorizontalAlignment.Center;
			var hostBlurb = App.TextBlock;
			hostBlurb.Text = "Host:";
			var hostField = App.TextBox;
			hostField.Width = 130;

			var portBlurb = App.TextBlock;
			portBlurb.Text = "Port:";
			var portField = App.TextBox;
			portField.Width = 50;
			portField.Text = "22";

			var userBlurb = App.TextBlock;
			userBlurb.Text = "Username:";
			var userField = App.TextBox;

			var passBlurb = App.TextBlock;
			passBlurb.Text = "Password:";
			var passField = App.TextBox;
			passField.PasswordChar = '*';

			var connectBt = App.Button;
			connectBt.Content = "Connect";
			var disconnectBt = App.Button;
			disconnectBt.Content = "Disconnect";
			disconnectBt.IsEnabled = false;

			partcPanel.Children.AddRange(new List<IControl> { hostBlurb, hostField, portBlurb, portField, userBlurb, userField, passBlurb, passField, connectBt, disconnectBt });

			var logScrollable = App.ScrollableTextBlock;
			logScrollable.Background = Brushes.Transparent;
			logScrollable.Width = 900;
			logScrollable.Resources["ScrollBarThickness"] = 12;

			var logBox = App.TextBox;
			logBox.Width = logScrollable.Width - App.Margin.Top * 6;
			logBox.Height = 340;
			logBox.IsReadOnly = true;
			logBox.Foreground = Brushes.NavajoWhite;
			logBox.Background = Brushes.DarkBlue;
			logScrollable.Content = logBox;

			var commandPan = App.HorizontalFieldWithButton;
			commandPan.label.Text = "Input SSH command:";
			commandPan.field.Width = 600;
			commandPan.field.IsEnabled = false;
			commandPan.field.KeyUp += (z, zz) => { if (zz.Key.Equals(Key.Enter)) { commandPan.button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); } };
			commandPan.holder.HorizontalAlignment = HorizontalAlignment.Center;
			commandPan.button.Content = "Send";
			commandPan.button.IsEnabled = false;

			rootPan.Children.AddRange(new List<IControl> { blurb1, partcPanel, logScrollable, commandPan.holder });

			connectBt.Click += async (z, zz) => {
				try {
					if (string.IsNullOrWhiteSpace(hostField.Text)) {
						throw new ArgumentNullException();
					}
					((App)Application.Current).LoadingBar.IsIndeterminate = true;

					client = await lib.ConnectToSshServerWithPassword(hostField.Text, int.Parse(portField.Text), userField.Text, passField[TextBox.TextProperty].ToString());
					ss = client.CreateShellStream("", 1, 0, 0, 0, 1024 * 8);

					var dataObs = Observable.FromEventPattern<ShellDataEventArgs>(zzz => ss.DataReceived += zzz, zzz => ss.DataReceived -= zzz);
					logBox[!TextBlock.TextProperty] = dataObs.Select(zzz => logBox.Text + Encoding.Default.GetString(zzz.EventArgs.Data)).ToBinding();

					logBox.Text = "";

					connectBt.IsEnabled = false;
					disconnectBt.IsEnabled = true;
					commandPan.field.IsEnabled = true;
					commandPan.button.IsEnabled = true;
					hostField.IsEnabled = false;
					portField.IsEnabled = false;
					userField.IsEnabled = false;
					passField.IsEnabled = false;

					((App)Application.Current).LoadingBar.IsIndeterminate = false;

				} catch (Exception e) {
					((App)Application.Current).LoadingBar.IsIndeterminate = false;
					if (e is ArgumentNullException || e is ArgumentException || e is FormatException) {
						logBox.Text += "Empty or invalid connection parameter(s)." + "\n";
					} else {
						logBox.Text += e.Message + "\n";
					}
				}
			};

			disconnectBt.Click += (z, zz) => {
				ss.Dispose();
				client.Disconnect();
				client.Dispose();

				connectBt.IsEnabled = true;
				disconnectBt.IsEnabled = false;
				commandPan.field.IsEnabled = false;
				commandPan.button.IsEnabled = false;
				hostField.IsEnabled = true;
				portField.IsEnabled = true;
				userField.IsEnabled = true;
				passField.IsEnabled = true;
			};

			commandPan.button.Click += (z, zz) => {
				ss.WriteLine(commandPan.field.Text);
				commandPan.field.Text = "";
			};
		}
	}
}