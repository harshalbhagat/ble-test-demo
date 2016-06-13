namespace Ble.Test.Model
{
    class ConnectionStatus
    {


        //private async void BtleDevice2_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        //{
        //    var msg = "Connection Status Changed - " + btleDevice2.ConnectionStatus;
        //    Debug.WriteLine(msg);
        //    _rootPage.NotifyUser(msg, NotifyType.StatusMessage);

        //    // We are disconnected, intialize the variables
        //    if (btleDevice2.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
        //    {
        //        _isDisconnected = true;

        //        // Set the BandData IsConnected status
        //        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => bandData.IsConnected = false);

        //        // Reset the event handler for the ValueChanged
        //        _characteristicsStreamTx[0].ValueChanged -= CharacteristicsTx_ValueChanged;

        //        //
        //        // Initialize parameters for Encrypted communication
        //        //
        //        _isConnecting = true;
        //        _characteristicsTxDataIn.Clear();
        //        _neededBytes = 0;
        //        _rawPacket = null;

        //        _tmpBuffer = null;
        //        _reminder = null;

        //        _sequenceNumberIn = 0xFF;
        //        _sequenceNumberOut = 0x00;                              //

        //        _isBusyCharacteristicsTxValueChanged = 0;
        //        _multiSendBusy = 0;
        //        _simpleSendBusy = 0;
        //        _decodeResponseBusy = 0;
        //        _saveTransactionIdsBusy = 0;
        //        _saveEpochTimesBusy = 0;

        //        QueueTransactions.Clear();
        //        QueueResponsePackets.Clear();

        //        _pairingSequenceState = PairingMsgTypes.BandNotConnected;

        //        lockQueueTransactions = new object();                   //
        //        lockQueueResponsePackets = new object();                //

        //        Utilities.VibratePhone();
        //    }

        //    // We are connected and were disconnected before.... Start a connection sequence 
        //    if ((btleDevice2.ConnectionStatus == BluetoothConnectionStatus.Connected) && _isDisconnected)
        //    {
        //        _isDisconnected = false;

        //        // Initiate the re-connection
        //        await InitializeGattStreamServiceCharateristics(_deviceInformationDisplayConnect);
        //        await InitializeControlService(btleDevice2);              //
        //        //await InitializeInformationService( btleDevice2 );        //

        //        // Start a connection sequence 
        //        if (_isConnectionDone == false)
        //        {
        //            await StartConnectionSequence();                        // 1. - Works   

        //            _rootPage.NotifyUser("Starting Pairing Sequence", NotifyType.StatusMessage);
        //        }
        //        else
        //        {
        //            await Connecting_EstablishSecureChannel_5();            // 5. - Works
        //        }

        //        _isWaitingConnect = true;
        //    }
        //}
    }
}
