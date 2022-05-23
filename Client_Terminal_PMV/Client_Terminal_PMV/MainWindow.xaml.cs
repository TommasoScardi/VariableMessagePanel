using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client_Terminal_PMV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MAX_CHAR_TEXT = 75;

        IPAddress ServerIP, ClientIP;
        List<Models.ModelMessaggio> _messaggi;
        Models.ModelMessaggio msgSel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateGUI()
        {
            if (_messaggi == null)
                return;

            GUI_listaMessaggiSalvati.Items.Clear();
            foreach (Models.ModelMessaggio messaggio in _messaggi)
            {
                GUI_listaMessaggiSalvati.Items.Add(messaggio);
            }
        }

        private void GUI_btnConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ServerIP == null)
                {
                    IPAddress iPAddress = IPAddress.Parse(GUI_txtIpServer.Text);
                    if (Connection.Ping(iPAddress))
                    {
                        if (Connection.TestConnToServer(iPAddress))
                        {
                            ServerIP = iPAddress;
                            ClientIP = Connection.GetWorkingIpAddress(ServerIP);
                            GUI_btnConnectToServer.Content = "Disconnetti";

                            GUI_updateMsg_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show("Il Server Non Supporta il servizio PMV");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Server Non Trovato");
                    }
                }
                else
                {
                    ServerIP = null;
                    ClientIP = null;
                    GUI_btnConnectToServer.Content = "Connetti";

                    GUI_listaMessaggiSalvati.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GUI_updateMsg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _messaggi = null;
                _messaggi = Action.GetMessages(ServerIP);
                UpdateGUI();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Errore Ottenendo MSG");
            }
        }

        private void GUI_listaMessaggiSalvati_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GUI_editMessageContainer.Header = "Modifica Messaggio";
            GUI_editMsg_Add.Visibility = Visibility.Collapsed;
            GUI_editMsg_Edit.Visibility = Visibility.Visible;
            GUI_editMsg_Del.Visibility = Visibility.Visible;
            GUI_editMsg_Des.Visibility = Visibility.Visible;

            msgSel = GUI_listaMessaggiSalvati.SelectedItem as Models.ModelMessaggio;
            if (msgSel != null)
            {
                GUI_editMsg_text.Text = msgSel.Testo;
                GUI_editMsg_view.IsChecked = msgSel.Visualizza;
                GUI_editMsg_view.Click += GUI_editMsg_view_Click;
            }
            else
            {
                GUI_editMsg_Des_Click(sender, e);
            }
        }

        private void GUI_editMsg_Des_Click(object sender, RoutedEventArgs e)
        {
            GUI_editMessageContainer.Header = "Aggiungi Messaggio";
            GUI_editMsg_Add.Visibility = Visibility.Visible;
            GUI_editMsg_Edit.Visibility = Visibility.Collapsed;
            GUI_editMsg_Del.Visibility = Visibility.Collapsed;
            GUI_editMsg_Des.Visibility = Visibility.Collapsed;

            GUI_listaMessaggiSalvati.SelectedItem = null;
            GUI_listaMessaggiSalvati.SelectedIndex = -1;

            GUI_editMsg_text.Text = String.Empty;
            GUI_editMsg_view.IsChecked = false;
            GUI_editMsg_view.Click -= GUI_editMsg_view_Click;

            msgSel = null;
        }

        private void GUI_editMsg_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GUI_editMsg_text.Text.Length == 0)
                {
                    MessageBox.Show("Campi vuoti"); ;
                    return;
                }
                if (GUI_editMsg_text.Text.Length > MAX_CHAR_TEXT)
                {
                    MessageBox.Show("Il Campo testo supera i 75 caratteri"); ;
                    return;
                }

                _messaggi.Add(Action.AddMessage(ServerIP, new Models.ModelMessaggio()
                {
                    Testo = GUI_editMsg_text.Text,
                    Visualizza = (bool)GUI_editMsg_view.IsChecked
                }));
                UpdateGUI();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Errore Aggiungendo MSG");
            }
        }

        private void GUI_editMsg_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GUI_editMsg_text.Text.Length == 0)
                {
                    MessageBox.Show("Campi vuoti"); ;
                    return;
                }
                if (GUI_editMsg_text.Text.Length > MAX_CHAR_TEXT)
                {
                    MessageBox.Show("Il Campo testo supera i 75 caratteri"); ;
                    return;
                }

                msgSel.Testo = GUI_editMsg_text.Text;
                msgSel.Visualizza = (bool)GUI_editMsg_view.IsChecked;

                Action.EditMessage(ServerIP, msgSel);
                GUI_editMsg_Des_Click(sender, e);
                UpdateGUI();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Errore Modificando MSG");
            }
        }

        private void GUI_editMsg_view_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (msgSel == null)
                {
                    return;
                }

                msgSel.Visualizza = (bool)GUI_editMsg_view.IsChecked;

                Action.MakeMessageToView(ServerIP, msgSel);
                GUI_editMsg_Des_Click(sender, e);
                UpdateGUI();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Errore Modificando MSG");
            }
        }

        private void GUI_editMsg_Del_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Action.DeleteMessage(ServerIP, msgSel);
                _messaggi.Remove(msgSel);
                GUI_editMsg_Des_Click(sender, e);
                UpdateGUI();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Errore Eliminando MSG");
            }
        }
    }
}
