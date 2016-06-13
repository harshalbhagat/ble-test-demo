using Ble.Test.Band;
using Ble.Test.Band.BTLE.Misc;
using Ble.Test.Model;
using Ble.Test.Types;
using Ble.Test.Utility;
using Ble.Test.Watcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static Ble.Test.Model.ResponseHeader;
using static Ble.Test.Types.Enums;
using Buffer = System.Buffer;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ble.Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private static readonly ObservableCollection<RawPacket> QueueResponsePackets = new ObservableCollection<RawPacket>();
        private static readonly ObservableCollection<Transaction> QueueTransactions = new ObservableCollection<Transaction>();
        private static IReadOnlyList<GattCharacteristic> _characteristicsControlConnectMode;
        // JAWBONE_CONTROL_SERVICE_UUID
        private static IReadOnlyList<GattCharacteristic> _characteristicsControlDatetime;

        private static IReadOnlyList<GattCharacteristic> _characteristicsStreamRx;
        // GATT_STREAM_SERVICE_UUID
        private static IReadOnlyList<GattCharacteristic> _characteristicsStreamTx;

        private static bool _isConnecting;
        private static bool _isConnectionDone;
        private static int _connect_5_Retry;
        // ** Needed for Re-Connect ** //
        private static PairingMsgTypes _pairingSequenceState = PairingMsgTypes.BandNotConnected;

        private static MainPage _rootPage;

        private static byte _sequenceNumberIn = 0xFF;
        // ** Needed for Re-Connect ** //
        private static byte _sequenceNumberOut;

        private static StreamEncryptor _streamEncryptor;
        private static ObservableCollection<TransactionIdItem> TransactionIds = new ObservableCollection<TransactionIdItem>();
        readonly ArrayList _characteristicsTxDataIn = new ArrayList();
        readonly string deviceId = @"\\?\BTHLEDevice#{151c1000-4580-4111-9ca1-5056f3454fbc}_ca32ba952d17#8&38c1c50d&9&0011#{6e3bb679-4372-40c8-9eaa-4509df260cd8}";
        private byte[] _bandChallenge = new byte[0];
        private byte[] _bandResponse = new byte[0];
        private int _decodeResponseBusy;
        private byte[] _deviceSeed = new byte[0];
        // Dictionary definitions
        //private Dictionary<uint, DateTime> _dictionaryEpochTimes = new Dictionary<uint, DateTime>();
        private Dictionary<byte, byte> _dictionaryTransactionIds = new Dictionary<byte, byte>();

        private byte _flags;

        // GATT Device Services
        private GattDeviceService _gattStreamService;
        private GattDeviceService _gattControlService;
        private GattDeviceService _gattInformationService;

        private int _isBusyCharacteristicsTxValueChanged;
        private bool _isDisconnected;
        private bool _isWaitingConnect = true;
        private byte[] _key = new byte[0];
        private byte[] _maskedPhoneChallengeZ = new byte[0];
        private byte _messageType;
        private int _multiSendBusy;
        private int _neededBytes;
        private byte _payloadSize;
        private byte[] _phoneChallenge = new byte[0];
        private byte[] _phoneResponse = new byte[0];
        private byte[] _phoneSeed = new byte[0];
        private RawPacket _rawPacket;
        private byte[] _reminder;
        private int _saveEpochTimesBusy;
        // ** Needed for Re-Connect ** //
        // ** Needed for Re-Connect ** //
        private int _saveTransactionIdsBusy;

        private int _simpleSendBusy;
        private byte[] _tmpBuffer;
        private byte _transactionId = 0xF1;
        private BtleWatcher _watcher2;
        private BtleWatcher _watcher3;
        private BtleWatcher _watcher4;


        private BandData bandData = new BandData();
        private BluetoothLEDevice btleDevice2;
        private BluetoothLEDevice btleDevice3;
        private BluetoothLEDevice btleDevice4;
        private object lockQueueResponsePackets = new object();

        // ** Needed for Re-Connect ** //
        // ** Needed for Re-Connect ** //
        // ** Needed for Re-Connect ** //
        private object lockQueueTransactions = new object();


        DeviceInformationDisplay _deviceInformationDisplayConnect;

        // GATT Stream Service - Stream Data from the band          
        // GATT Stream Service - Stream Data to the band
        public MainPage()
        {
            this.InitializeComponent();
            _rootPage = this;

            var aqsFilter = GattDeviceService.GetDeviceSelectorFromUuid(new Guid(UuidDefs.GATT_STREAM_SERVICE_UUID));
            _watcher2 = new BtleWatcher(this, "W2 - GATT_STREAM_SERVICE_UUID ");
            _watcher2.InitializeBtleWatcher(aqsFilter);


            // Hook a function to the watcher events
            _watcher2.WacherEvent += Wacher2EventFired;

            // Start the third watcher for the UP Band Control Service: JAWBONE_CONTROL_SERVICE_UUID
            aqsFilter = GattDeviceService.GetDeviceSelectorFromUuid(new Guid(UuidDefs.JAWBONE_CONTROL_SERVICE_UUID));
            _watcher3 = new BtleWatcher(this, "W3 - JAWBONE_CONTROL_SERVICE_UUID ");
            _watcher3.InitializeBtleWatcher(aqsFilter);

            // Start the third watcher for the UP Band Control Service: DEVICE_INFORMATION_SERVICE_UUID
            aqsFilter = GattDeviceService.GetDeviceSelectorFromUuid(new Guid(UuidDefs.DEVICE_INFORMATION_SERVICE_UUID));
            _watcher4 = new BtleWatcher(this, "W4 - DEVICE_INFORMATION_SERVICE_UUID ");
            _watcher4.InitializeBtleWatcher(aqsFilter);

            _rootPage.NotifyUser("W2/W3/W4.....Watcher Initilized", NotifyType.StatusMessage);

            // _watcher2
            _watcher2.Watcher.Start();

            // _watcher3
            _watcher3.Watcher.Start();

            // _watcher4
            _watcher4.Watcher.Start();

        }

        public async void NotifyUser(string strMessage, NotifyType type)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (type)
                {
                    case NotifyType.StatusMessage:
                        StatusBorder.Background = new SolidColorBrush(Colors.Green);
                        break;

                    case NotifyType.ErrorMessage:
                        StatusBorder.Background = new SolidColorBrush(Colors.Red);
                        break;
                }

                StatusBlock.Text = strMessage;

                // Collapse the StatusBlock if it has no text to conserve real estate.
                StatusBorder.Visibility = StatusBlock.Text != string.Empty ? Visibility.Visible : Visibility.Collapsed;
                if (StatusBlock.Text != string.Empty)
                {
                    StatusBorder.Visibility = Visibility.Visible;
                    StatusPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    StatusBorder.Visibility = Visibility.Collapsed;
                    StatusPanel.Visibility = Visibility.Collapsed;
                }
            });
        }

        private static void Decrypt(byte[] data, int index, int len)
        {
            if ((_streamEncryptor != null) && (_streamEncryptor.IsEnabled) && (len > 0))
            {
                _streamEncryptor.DecryptAsync(data, index, len);
            }
        }

        private async void BtleDevice2_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            var msg = "Connection Status Changed - " + btleDevice2.ConnectionStatus;
            Debug.WriteLine(msg);
            _rootPage.NotifyUser(msg, NotifyType.StatusMessage);

            // We are disconnected, intialize the variables
            if (btleDevice2.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                _isDisconnected = true;

                // Set the BandData IsConnected status
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => bandData.IsConnected = false);

                // Reset the event handler for the ValueChanged
                _characteristicsStreamTx[0].ValueChanged -= CharacteristicsTx_ValueChanged;

                //
                // Initialize parameters for Encrypted communication
                //
                _isConnecting = true;
                _characteristicsTxDataIn.Clear();
                _neededBytes = 0;
                _rawPacket = null;

                _tmpBuffer = null;
                _reminder = null;

                _sequenceNumberIn = 0xFF;
                _sequenceNumberOut = 0x00;                              //

                _isBusyCharacteristicsTxValueChanged = 0;
                _multiSendBusy = 0;
                _simpleSendBusy = 0;
                _decodeResponseBusy = 0;
                _saveTransactionIdsBusy = 0;
                _saveEpochTimesBusy = 0;

                QueueTransactions.Clear();
                QueueResponsePackets.Clear();

                _pairingSequenceState = PairingMsgTypes.BandNotConnected;

                lockQueueTransactions = new object();                   //
                lockQueueResponsePackets = new object();                //

                //Utilities.VibratePhone(500);
            }

            // We are connected and were disconnected before.... Start a connection sequence 
            if ((btleDevice2.ConnectionStatus == BluetoothConnectionStatus.Connected) && _isDisconnected)
            {
                // await SpeakText("Band is reconnected");

                _isDisconnected = false;

                // Initiate the re-connection
                await InitializeGattStreamServiceCharateristics(_deviceInformationDisplayConnect);
                await InitializeControlService(btleDevice2);              //
                                                                          //await InitializeInformationService( btleDevice2 );        //

                if (_connect_5_Retry == 3)
                {
                    _isConnectionDone = false;
                }


                // Start a connection sequence 
                if (_isConnectionDone == false)
                {
                    await StartConnectionSequence();                        // 1. - Works   

                    _rootPage.NotifyUser("Starting Connection Sequence", NotifyType.StatusMessage);
                }
                else
                {
                    await Connecting_EstablishSecureChannel_5();            // 5. - Works
                    _connect_5_Retry++;
                }

                _isWaitingConnect = true;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            StopDeviceWatchers();

            btleDevice2 = await BluetoothLEDevice.FromIdAsync(deviceId);

            Debug.WriteLine("\n" + btleDevice2.BluetoothAddress + "\n");
#if DEBUG
            if (btleDevice2 != null)
            {
                // BT_Alert: GattServices returns a list of all the supported services of the device. If the services supported by the device are expected to change
                // during BT usage, make sure to implement the GattServicesChanged event
                Debug.WriteLine("Services: ");
                Debug.WriteLine("========= ");

                foreach (var service in btleDevice2.GattServices)
                {
                    Debug.WriteLine("    Custom Service: " + service.Uuid);
                }
            }
#endif
            _deviceInformationDisplayConnect = _watcher2.ResultCollection.ElementAt(0);

            _deviceInformationDisplayConnect.BtleAddress = btleDevice2.BluetoothAddress;
            // Occurs when the connection status for the device has changed.
            btleDevice2.ConnectionStatusChanged += BtleDevice2_ConnectionStatusChanged;


            //
            // Get the GATT_STREAM_SERVICE and Characteristics
            //
            await InitializeGattStreamServiceCharateristics(_deviceInformationDisplayConnect);

            // _watcher3
            //
            // Get the JAWBONE_CONTROL_SERVICE and Characteristics
            //
            await InitializeControlService(btleDevice2);

            // _watcher4
            //
            // Get the DEVICE_INFORMATION_SERVICE_UUID and Characteristics
            //
            await InitializeInformationService(btleDevice2);

            await StartConnectionSequence();

            _rootPage.NotifyUser("Starting Pairing Sequence", NotifyType.StatusMessage);
        }

        private BandInfo bandInfo = new BandInfo();
        // DEVICE_INFORMATION_SERVICE_UUID
        private static IReadOnlyList<GattCharacteristic> _characteristicsInformationModelNo;            // Device Information Service - Read from the Band - Model Number
        private static IReadOnlyList<GattCharacteristic> _characteristicsInformationSerialNo;           // Device Information Service - Read from the Band - Serial Number
        private static IReadOnlyList<GattCharacteristic> _characteristicsInformationFirmwareVer;        // Device Information Service - Read from the Band - Firmware Version
        private static IReadOnlyList<GattCharacteristic> _characteristicsInformationSoftwareVer;        // Device Information Service - Read from the Band - Software Version
        private static IReadOnlyList<GattCharacteristic> _characteristicsInformationlManufacturer;      // Device Information Service - Read from the Band - Manufacturer Name

        #region Information Service - DEVICE_INFORMATION_SERVICE_UUID
        private async Task InitializeInformationService(BluetoothLEDevice btleDevice)
        {
            if (_watcher4.ResultCollection.Any() && _watcher4.EnumCompleted)
            {
                //foreach ( DeviceInformationDisplay entry in ResultCollection4 )
                foreach (DeviceInformationDisplay entry in _watcher4.ResultCollection)
                {
                    btleDevice4 = await BluetoothLEDevice.FromIdAsync(entry.Id);

                    // Here we check if we are using the same BTLE address as btleDevice2: btleDevice2.BluetoothAddress == btleDevice3.BluetoothAddress
                    if (btleDevice4.BluetoothAddress == btleDevice.BluetoothAddress)
                    {
                        Guid guid = new Guid(UuidDefs.DEVICE_INFORMATION_SERVICE_UUID);
                        _gattInformationService = btleDevice4.GetGattService(guid);

                        // Get the Model Number characteristics
                        _characteristicsInformationModelNo = GetCharacteristics(_gattInformationService, UuidDefs.DEVICE_INFORMATION_MODEL_NUMBER_CHARACTERISTIC_UUID);
                        Debug.Assert(_characteristicsInformationModelNo != null);
                        GattReadResult modelNo = await _characteristicsInformationModelNo[0].ReadValueAsync();
                        Debug.WriteLine("   Characteristics:                             " + "_characteristicsInformationModelNo");
                        Debug.WriteLine("       Status:                                  " + modelNo.Status);
                        Debug.WriteLine("       Value:                                   " + Utilities.ReadIBuffer2Str(modelNo.Value, true));
                        using (DataReader dataReader = DataReader.FromBuffer(modelNo.Value))
                        {
                            bandInfo.ModelNo = dataReader.ReadString(modelNo.Value.Length);
                            Debug.WriteLine("       Model Number:                            " + bandInfo.ModelNo);
                        }
                        Debug.WriteLine("");

                        // Get the Serial Number characteristics
                        _characteristicsInformationSerialNo = GetCharacteristics(_gattInformationService, UuidDefs.DEVICE_INFORMATION_SERIAL_NUMBER_CHARACTERISTIC_UUID);
                        Debug.Assert(_characteristicsInformationSerialNo != null);
                        GattReadResult serialNo = await _characteristicsInformationSerialNo[0].ReadValueAsync();
                        Debug.WriteLine("   Characteristics:                             " + "_characteristicsInformationSerialNo");
                        Debug.WriteLine("       Status:                                  " + serialNo.Status);
                        Debug.WriteLine("       Value:                                   " + Utilities.ReadIBuffer2Str(serialNo.Value, true));
                        using (DataReader dataReader = DataReader.FromBuffer(serialNo.Value))
                        {
                            byte[] serialNumBytes = new byte[serialNo.Value.Length];
                            dataReader.ReadBytes(serialNumBytes);
                            bandInfo.SerialNo = serialNumBytes.BytesToString(serialNumBytes.Length);
                            Debug.WriteLine("       Serial Number:                           " + bandInfo.SerialNo);

                            var buffer = new byte[2];
                            Buffer.BlockCopy(serialNumBytes, 0, buffer, 0, 2);
                            bandInfo.PatternConfig = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          Pattern Config:                       " + bandInfo.PatternConfig);

                            buffer = new byte[1];
                            Buffer.BlockCopy(serialNumBytes, 2, buffer, 0, 1);
                            bandInfo.PcbVersion = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          PCB Version:                          " + bandInfo.PcbVersion);

                            buffer = new byte[1];
                            Buffer.BlockCopy(serialNumBytes, 3, buffer, 0, 1);
                            bandInfo.YearCode = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          Year Code:                            " + bandInfo.YearCode);

                            buffer = new byte[2];
                            Buffer.BlockCopy(serialNumBytes, 4, buffer, 0, 2);
                            bandInfo.WeekCode = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          Week Code:                            " + bandInfo.WeekCode);

                            buffer = new byte[5];
                            Buffer.BlockCopy(serialNumBytes, 6, buffer, 0, 5);
                            bandInfo.Serial = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          Serial:                               " + bandInfo.Serial);

                            buffer = new byte[1];
                            Buffer.BlockCopy(serialNumBytes, 11, buffer, 0, 1);
                            bandInfo.FwCode = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          FW Code:                              " + bandInfo.FwCode);

                            buffer = new byte[1];
                            Buffer.BlockCopy(serialNumBytes, 12, buffer, 0, 1);
                            bandInfo.Size = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          Size:                                 " + bandInfo.Size);

                            buffer = new byte[2];
                            Buffer.BlockCopy(serialNumBytes, 13, buffer, 0, 2);
                            bandInfo.BandColor = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          Band Color:                           " + bandInfo.BandColor);

                            buffer = new byte[1];
                            Buffer.BlockCopy(serialNumBytes, 15, buffer, 0, 1);
                            bandInfo.ProductCode = buffer.BytesToString(buffer.Length);
                            Debug.WriteLine("          Product Code:                         " + bandInfo.ProductCode);
                        }
                        Debug.WriteLine("");

                        // Get the Firmware Version characteristics
                        _characteristicsInformationFirmwareVer = GetCharacteristics(_gattInformationService, UuidDefs.DEVICE_INFORMATION_FIRMWARE_REVISION_CHARACTERISTIC_UUID);
                        Debug.Assert(_characteristicsInformationFirmwareVer != null);
                        GattReadResult firmwareVer = await _characteristicsInformationFirmwareVer[0].ReadValueAsync();
                        Debug.WriteLine("   Characteristics:                             " + "_characteristicsInformationFirmwareVer");
                        Debug.WriteLine("       Status:                                  " + firmwareVer.Status);
                        Debug.WriteLine("       Value:                                   " + Utilities.ReadIBuffer2Str(firmwareVer.Value, true));
                        using (DataReader dataReader = DataReader.FromBuffer(firmwareVer.Value))
                        {
                            bandInfo.FirmwareVer = dataReader.ReadString(firmwareVer.Value.Length);
                            Debug.WriteLine("       Firmware Version:                        " + bandInfo.FirmwareVer);
                        }
                        Debug.WriteLine("");

                        // Get the Software Version characteristics
                        _characteristicsInformationSoftwareVer = GetCharacteristics(_gattInformationService, UuidDefs.DEVICE_INFORMATION_SOFTWARE_REVISION_CHARACTERISTIC_UUID);
                        Debug.Assert(_characteristicsInformationSoftwareVer != null);
                        GattReadResult softwareVer = await _characteristicsInformationSoftwareVer[0].ReadValueAsync();
                        Debug.WriteLine("   Characteristics:                             " + "_characteristicsInformationSoftwareVer");
                        Debug.WriteLine("       Status:                                  " + softwareVer.Status);
                        Debug.WriteLine("       Value:                                   " + Utilities.ReadIBuffer2Str(softwareVer.Value, true));
                        using (DataReader dataReader = DataReader.FromBuffer(softwareVer.Value))
                        {
                            bandInfo.SoftwareVer = dataReader.ReadString(softwareVer.Value.Length);
                            Debug.WriteLine("       Software Version:                        " + bandInfo.SoftwareVer);
                        }
                        Debug.WriteLine("");

                        // Get the Connect Mode characteristics
                        _characteristicsInformationlManufacturer = GetCharacteristics(_gattInformationService, UuidDefs.DEVICE_INFORMATION_MANUFACTURER_NAME_CHARACTERISTIC_UUID);
                        Debug.Assert(_characteristicsInformationlManufacturer != null);
                        GattReadResult manufacturer = await _characteristicsInformationlManufacturer[0].ReadValueAsync();
                        Debug.WriteLine("   Characteristics:                             " + "_characteristicsInformationlManufacturer");
                        Debug.WriteLine("       Status:                                  " + manufacturer.Status);
                        Debug.WriteLine("       Value:                                   " + Utilities.ReadIBuffer2Str(manufacturer.Value, true));
                        using (DataReader dataReader = DataReader.FromBuffer(manufacturer.Value))
                        {
                            bandInfo.Manufacturer = dataReader.ReadString(manufacturer.Value.Length - 1);
                            Debug.WriteLine("       Manufacturer:                            " + bandInfo.Manufacturer);
                        }
                        Debug.WriteLine("");
                        break;
                    }

                    bandInfo.Id = entry.Id;
                    bandInfo.Name = entry.Name;
                    bandInfo.BtleAddress = entry.BtleAddress;
                }

                //BorderBandInfo.Visibility = Visibility.Visible;
                //BorderBandInfo.DataContext = bandInfo;

                bool status = await StorageHelper.WriteDataFileAsync("BandInfo", bandInfo);
            }
        }
        #endregion


        private void StopDeviceWatchers()
        {
            _watcher2.StopDeviceWatcher();
            _watcher3.StopDeviceWatcher();
            _watcher4.StopDeviceWatcher();
        }

        private void CharacteristicsTx_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (_isBusyCharacteristicsTxValueChanged > 0)
            {
                return;
            }
            _isBusyCharacteristicsTxValueChanged = Interlocked.Increment(ref _isBusyCharacteristicsTxValueChanged);

            byte[] buffer = Utilities.ReadBuffer(args.CharacteristicValue);

            // Check out of sequence
            if ((byte)(_sequenceNumberIn + 1) != buffer[0])
            {
                Debug.WriteLine("!!! Error, out of sequence packet !!!");
                Debug.WriteLine("        Expected: " + string.Format("0x" + (_sequenceNumberIn + 1).ToString("X2")));
                Debug.WriteLine("        Recieved: " + string.Format("0x" + buffer[0].ToString("X2")));
            }
            _sequenceNumberIn = buffer[0];

            //
            // Still pairing - Data not Encrypted
            //
            if (_isConnecting)
            {
                _characteristicsTxDataIn?.AddRange(buffer);

                // Here we need to decode the data 
                DecodePairingResponse(buffer);

                _isBusyCharacteristicsTxValueChanged = Interlocked.Decrement(ref _isBusyCharacteristicsTxValueChanged);
                return;
            }

            //
            // Done pairing, store packets into _queuePackets queue - Data Encryped
            //
            if (_isConnecting == false)
            {
                // Here we need first to glue packets together so we have one full packet
                //Debug.WriteLine( "CharacteristicsTx_ValueChanged\n" );
                ReadPacketIn(buffer);

                _isBusyCharacteristicsTxValueChanged = Interlocked.Decrement(ref _isBusyCharacteristicsTxValueChanged);
            }
        }

        private async Task Connecting_Authenticate_3()
        {
            Debug.WriteLine("\n\n== 3. Authentication");
            _phoneChallenge = CryptographicBuffer.GenerateRandom(BtleLinkTypes.ENCR_CHALLENGE_SIZE).ToArray();

            byte[] phoneChallengeZ = new byte[_phoneChallenge.Length + BtleLinkTypes.ENCR_CHALLENGE_SIZE];
            Buffer.BlockCopy(_phoneChallenge, 0, phoneChallengeZ, 0, _phoneChallenge.Length);

            IBuffer phoneChallengeZBuffer = CryptographicBuffer.CreateFromByteArray(phoneChallengeZ);
            SymmetricKeyAlgorithmProvider aesProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcb);

            // Creates a symmetric key

            CryptographicKey cryptographicKey = aesProvider.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(_key));

            // Encrypt the data
            IBuffer phoneChallengeZEncrypted = CryptographicEngine.Encrypt(cryptographicKey, phoneChallengeZBuffer, null);

            byte[] phoneChallengeZBytes;
            CryptographicBuffer.CopyToByteArray(phoneChallengeZEncrypted, out phoneChallengeZBytes);

            _maskedPhoneChallengeZ = new byte[BtleLinkTypes.ENCRYPTED_BLOCK_SIZE];
            Buffer.BlockCopy(phoneChallengeZBytes, 0, _maskedPhoneChallengeZ, 0, BtleLinkTypes.ENCRYPTED_BLOCK_SIZE);

            await SendAuthenticateCmd();
        }

        // ** Needed for Re-Connect ** //
        //-----------------------------
        private async Task Connecting_EstablishSecureChannel_5()
        {
            Debug.WriteLine("\n\n== 5. Establish Secure Channel");
            Debug.WriteLine("\nEstablishing secure channel... Key = " + Utilities.ByteArray2String(_key, true));

            _phoneSeed = CryptographicBuffer.GenerateRandom(BtleLinkTypes.ENCRYPTED_BLOCK_SIZE).ToArray();

            _messageType = (byte)PairingMsgTypes.EstablishSecureChannel;    // 0x06
            _flags = 0x00;
            byte[] protocolEstablishSecureChannel = new byte[BtleLinkTypes.ENCRYPTED_BLOCK_SIZE];
            Buffer.BlockCopy(_phoneSeed, 0, protocolEstablishSecureChannel, 0, BtleLinkTypes.ENCRYPTED_BLOCK_SIZE);
            _payloadSize = (byte)protocolEstablishSecureChannel.Length;
            var msgHeader = new TransactionHeader((PairingMsgTypes)_messageType, _flags, _transactionId, _payloadSize);
            var transaction = new Transaction(_characteristicsStreamRx[0], msgHeader, protocolEstablishSecureChannel, false);
            await SaveTransaction(transaction);

            _rootPage.NotifyUser("5. Establish Secure Channel", NotifyType.StatusMessage);
        }

        private async Task Connecting_GetProtocolVersion_1()
        {
            Debug.WriteLine("\n\n== 1. Get Protocol Version");
            _messageType = (byte)PairingMsgTypes.GetProtocolVersion;        // 0x00
            _flags = 0x00;
            _payloadSize = 0x00;
            var msgHeader = new TransactionHeader((PairingMsgTypes)_messageType, _flags, _transactionId, _payloadSize);
            var transaction = new Transaction(_characteristicsStreamRx[0], msgHeader, new byte[0], false);
            await SaveTransaction(transaction);

            _rootPage.NotifyUser("1. Protocol Version", NotifyType.StatusMessage);
        }

        private async Task Connecting_KeyExchange_2()
        {
            Debug.WriteLine("\n\n== 2. Key Exchange");
            _messageType = (byte)PairingMsgTypes.KeyExchange;               // 0x03
            _flags = 0x00;
            _key = CryptographicBuffer.GenerateRandom(BtleLinkTypes.ENCRYPTED_BLOCK_SIZE).ToArray();

            Debug.Assert(_key != null);

            byte[] protocolKeyExchangePayload = new byte[_key.Length];
            Buffer.BlockCopy(_key, 0, protocolKeyExchangePayload, 0, _key.Length);
            _payloadSize = (byte)protocolKeyExchangePayload.Length;
            var msgHeader = new TransactionHeader((PairingMsgTypes)_messageType, _flags, _transactionId, _payloadSize);
            var transaction = new Transaction(_characteristicsStreamRx[0], msgHeader, protocolKeyExchangePayload, false);
            await SaveTransaction(transaction);

            _rootPage.NotifyUser("2. Key Exchange - Tap Band", NotifyType.StatusMessage);
        }

        private async void DecodePairingResponse(byte[] buffer)
        {
            if (_characteristicsTxDataIn.Count < 1 + BtleLinkTypes.RESPONSE_HEADER_SIZE)
            {
                _characteristicsTxDataIn.Clear();
                return;
            }

            Debug.WriteLine("\nPairing - DecodePairingResponse: " + Utilities.ByteArray2String(buffer, true));


            ///////////////////////
            // Decode the Header //
            /////////////////////// 
            byte messageType = (byte)_characteristicsTxDataIn[1 + (int)HdrOffset.MessageType];
            byte requestStatus = (byte)_characteristicsTxDataIn[1 + (int)HdrOffset.Flags];
            byte trasactionId = (byte)_characteristicsTxDataIn[1 + (int)HdrOffset.TransactionID];
            byte payloadSize = (byte)_characteristicsTxDataIn[1 + (int)HdrOffset.PayloadSize];

            var response = new Response
            {
                Payload = new byte[payloadSize],
                Header =
                    {
                    ResponseType = messageType,
                    Flags = (ResponseStatus)requestStatus,
                    TransactionId = trasactionId,
                    PayloadSize = payloadSize
                    }
            };


            /////////////////////
            // Simple Response //
            /////////////////////
            if (payloadSize <= BtleLinkTypes.SIMPLE_TRANSACTION_MAX_PAYLOAD_SIZE)
            {
                Buffer.BlockCopy(_characteristicsTxDataIn.ToArray(typeof(byte)), 1 + BtleLinkTypes.RESPONSE_HEADER_SIZE, response.Payload, 0, payloadSize);

                // dump the data
#if DEBUG
                DumpResponse(response);
#endif

                //dataIn.Clear();
                _characteristicsTxDataIn.RemoveRange(0, 1 + BtleLinkTypes.RESPONSE_HEADER_SIZE + payloadSize);

                lock (lockQueueTransactions)
                {
                    if (QueueTransactions.Any())
                    {
                        QueueTransactions.RemoveAt(0);
                    }
                }

                if (_pairingSequenceState == PairingMsgTypes.GetProtocolVersion)
                {
                    // 2. Key Exchange
                    await Connecting_KeyExchange_2();
                }

                if (_pairingSequenceState == PairingMsgTypes.KeyExchange)
                {
                    // 3. Authenticate
                    await Connecting_Authenticate_3();
                }

                if (_pairingSequenceState == PairingMsgTypes.RespondToChallenge)
                {
                    // 5. EstablishSecureChannel
                    await Connecting_EstablishSecureChannel_5();
                }

                _neededBytes = 0;
                return;
            }


            ////////////////////
            // Multi Response //
            ////////////////////
            if (_characteristicsTxDataIn.Count <= BtleLinkTypes.BTLE_PACKET_SIZE)
            {
                return;
            }

            _neededBytes = payloadSize - BtleLinkTypes.SIMPLE_TRANSACTION_MAX_PAYLOAD_SIZE;
            if (_neededBytes != buffer.Length - 1)
            {
                Debug.WriteLine("!!!!!  Error in the second packet  !!!!!    Expected: " + _neededBytes + "  -- Actual: " + (buffer.Length - 1));

                Buffer.BlockCopy(_characteristicsTxDataIn.ToArray(typeof(byte)), 1 + BtleLinkTypes.RESPONSE_HEADER_SIZE, response.Payload, 0, BtleLinkTypes.SIMPLE_TRANSACTION_MAX_PAYLOAD_SIZE);
                Buffer.BlockCopy(buffer, 1, response.Payload, BtleLinkTypes.SIMPLE_TRANSACTION_MAX_PAYLOAD_SIZE, Math.Min(_neededBytes, buffer.Length - 1));

                _tmpBuffer = new byte[buffer.Length - _neededBytes];
                _tmpBuffer[0] = buffer[0];
                Buffer.BlockCopy(buffer, 1 + _neededBytes, _tmpBuffer, 1, buffer.Length - 2);

                _characteristicsTxDataIn.Clear();
            }
            else
            {
                Buffer.BlockCopy(_characteristicsTxDataIn.ToArray(typeof(byte)), 1 + BtleLinkTypes.RESPONSE_HEADER_SIZE, response.Payload, 0, BtleLinkTypes.SIMPLE_TRANSACTION_MAX_PAYLOAD_SIZE);
                Buffer.BlockCopy(_characteristicsTxDataIn.ToArray(typeof(byte)), BtleLinkTypes.BTLE_PACKET_SIZE + 1, response.Payload, BtleLinkTypes.SIMPLE_TRANSACTION_MAX_PAYLOAD_SIZE, _neededBytes);
                _characteristicsTxDataIn.RemoveRange(0, BtleLinkTypes.BTLE_PACKET_SIZE + 1 + _neededBytes);
            }

#if DEBUG
            DumpResponse(response);
#endif

            // Response for: Authenticate
            if (_pairingSequenceState == PairingMsgTypes.Authenticate)
            {
                Debug.Assert(_key != null);

                IBuffer keyBuffer = CryptographicBuffer.CreateFromByteArray(_key);
                _bandResponse = response.Payload;
                IBuffer BR_Buffer = CryptographicBuffer.CreateFromByteArray(_bandResponse);
                SymmetricKeyAlgorithmProvider aesProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcb);

                // Creates a symmetric key
                // https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k(Windows.Security.Cryptography.Core.SymmetricKeyAlgorithmProvider.CreateSymmetricKey);k(TargetFrameworkMoniker-.NETCore,Version%3Dv5.0);k(DevLang-csharp)&rd=true 
                // https://msdn.microsoft.com/en-us/library/windows/apps/xaml/br241541(v=win.10).aspx?appid=dev14idef1&l=en-us&k=k(windows.security.cryptography.core.symmetrickeyalgorithmprovider.createsymmetrickey)%3bk(targetframeworkmoniker-.netcore,version%3dv5.0)%3bk(devlang-csharp)&rd=true
                CryptographicKey cryptographicKey = aesProvider.CreateSymmetricKey(keyBuffer);

                // Decrypt the data
                IBuffer phoneChallengeBandChallengeBuffer = CryptographicEngine.Decrypt(cryptographicKey, BR_Buffer, null);

                byte[] phoneChallengeBandChallenge;
                CryptographicBuffer.CopyToByteArray(phoneChallengeBandChallengeBuffer, out phoneChallengeBandChallenge);
                Debug.WriteLine("\nBand response to PhoneChallenge is: ");
                Debug.WriteLine("\n    PhoneChallenge_BandChallenge = " + Utilities.ByteArray2String(phoneChallengeBandChallenge, true));

                byte[] phoneChallengeReceived = new byte[BtleLinkTypes.ENCR_CHALLENGE_SIZE];
                Buffer.BlockCopy(phoneChallengeBandChallenge, 0, phoneChallengeReceived, 0, BtleLinkTypes.ENCR_CHALLENGE_SIZE);
                if (!_phoneChallenge.SequenceEqual(phoneChallengeReceived))
                {
                    Debug.WriteLine("PhoneChallenges not matched. PhoneChallenge = {0}, PhoneChallenge_Received = {1}\n",
                                            Utilities.ByteArray2String(_phoneChallenge),
                                            Utilities.ByteArray2String(phoneChallengeReceived));
                    throw new Exception("JawboneErrorCodes.DECRYPTION_FAILED");
                }
                Debug.WriteLine("  +++ PhoneChallenges matched! +++\n");

                // BandChallenge: Band challenge, random non-zero 8 bytes generated by the band
                _bandChallenge = new byte[BtleLinkTypes.ENCR_CHALLENGE_SIZE];
                Buffer.BlockCopy(phoneChallengeBandChallenge, BtleLinkTypes.ENCR_CHALLENGE_SIZE, _bandChallenge, 0, BtleLinkTypes.ENCR_CHALLENGE_SIZE);

                lock (lockQueueTransactions)
                {
                    if (QueueTransactions.Any())
                    {
                        QueueTransactions.RemoveAt(0);
                    }
                }

                // 4. Prepare the next command: RespondToChallenge
                await Connecting_RespondToChallenge_4(keyBuffer, phoneChallengeBandChallengeBuffer);

                return;
            }

            if (_pairingSequenceState == PairingMsgTypes.EstablishSecureChannel)
            {
                _deviceSeed = new byte[BtleLinkTypes.ENCRYPTED_BLOCK_SIZE];
                Buffer.BlockCopy(response.Payload, 0, _deviceSeed, 0, BtleLinkTypes.ENCRYPTED_BLOCK_SIZE);

                // 6. Initialize Stream Encryptor
                Connecting_InitStreamEncryptor_6();

                // We don't need the dataIn anymore
                _characteristicsTxDataIn.Clear();

                _pairingSequenceState = PairingMsgTypes.BandConnected;
                _isConnecting = false;
                _isConnectionDone = true;
                _connect_5_Retry = 0;

                if (_tmpBuffer != null)
                {
                    //Debug.WriteLine( "  +++ PairingMsgTypes.EstablishSecureChannel +++\n" );
                    ReadPacketIn(_tmpBuffer);
                    _tmpBuffer = null;
                }

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //AppBarButtonAlert.IsEnabled = true;
                    //AppBarButtonDateTime.IsEnabled = true;
                    //AppBarButtonSync.IsEnabled = true;
                    //AppBarButtonSetting.IsEnabled = true;
                });

                _rootPage.NotifyUser("Connection Is Done...", NotifyType.StatusMessage);
            }
        }

        private void Connecting_InitStreamEncryptor_6()
        {
#if DEBUG
            var msg = new StringBuilder();

            msg.AppendFormat("\n\n== 6. Init Stream Encryptor\n");

            Debug.Assert(_key != null && _key.Length > 0);
            Debug.Assert(_deviceSeed != null && _deviceSeed.Length > 0);
            Debug.Assert(_phoneSeed != null && _phoneSeed.Length > 0);
#endif

            _streamEncryptor = new StreamEncryptor(_key, _deviceSeed, _phoneSeed);

#if DEBUG
            msg.AppendFormat("    END - Connecting_InitStreamEncryptor_6:\n");
            msg.AppendFormat("        key:        " + Utilities.ByteArray2String(_key) + "\n");
            msg.AppendFormat("        DeviceSeed: " + Utilities.ByteArray2String(_deviceSeed) + "\n");
            msg.AppendFormat("        PhoneSeed:  " + Utilities.ByteArray2String(_phoneSeed) + "\n");
            msg.AppendFormat("\n\n");

            Debug.WriteLine(msg);
#endif

            _rootPage.NotifyUser("6. Initialize Stream Encryptor", NotifyType.StatusMessage);
        }

        private async Task Connecting_RespondToChallenge_4(IBuffer keyBuffer, IBuffer phoneChallengeBandChallengeBuffer)
        {
            Debug.WriteLine("\n\n== 4. Respond To Challenge");

            Debug.WriteLine("Sending response to BandResponse");
            Debug.WriteLine("    BandChallenge = " + Utilities.ByteArray2String(_bandChallenge, true));

            Debug.Assert(keyBuffer != null && keyBuffer.Length > 0);
            Debug.Assert(phoneChallengeBandChallengeBuffer != null && phoneChallengeBandChallengeBuffer.Length > 0);
            Debug.Assert(_bandChallenge != null && _bandChallenge.Length > 0);

            byte[] zBandChallenge = new byte[BtleLinkTypes.ENCR_CHALLENGE_SIZE + _bandChallenge.Length];
            Buffer.BlockCopy(_bandChallenge, 0, zBandChallenge, BtleLinkTypes.ENCR_CHALLENGE_SIZE, _bandChallenge.Length);
            Debug.WriteLine("    Z_BandChallenge = " + Utilities.ByteArray2String(zBandChallenge, true));

            SymmetricKeyAlgorithmProvider aesProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcb);

            // Creates a symmetric key
            CryptographicKey cryptographicKey = aesProvider.CreateSymmetricKey(keyBuffer);

            // Encrypt the data
            IBuffer PR_Buffer = CryptographicEngine.Encrypt(cryptographicKey, phoneChallengeBandChallengeBuffer, null);
            _phoneResponse = Utilities.Xor(PR_Buffer.ToArray(), zBandChallenge, zBandChallenge.Length);
            Debug.WriteLine("    PhoneResponse = " + Utilities.ByteArray2String(_phoneResponse, true));

            _messageType = (byte)PairingMsgTypes.RespondToChallenge;        // 0x05
            _flags = 0x00;
            byte[] challengePayload = _phoneResponse;
            _payloadSize = (byte)challengePayload.Length;
            var msgHeader = new TransactionHeader((PairingMsgTypes)_messageType, _flags, _transactionId, _payloadSize);
            var transaction = new Transaction(_characteristicsStreamRx[0], msgHeader, challengePayload, false);
            await SaveTransaction(transaction);

            _rootPage.NotifyUser("4. Respond To Challenge", NotifyType.StatusMessage);
        }

        private void DumpResponse(Response response)
        {
#if DEBUG
            var msg = new StringBuilder();

            msg.AppendFormat("    Response Data: " + Utilities.ByteArray2String((byte[])_characteristicsTxDataIn.ToArray(typeof(byte)), true) + "\n");
            msg.AppendFormat("        Header:\n");
            msg.AppendFormat("            ResponseType:     " + Enum.GetName(typeof(MessageReponseTypes), response.Header.ResponseType) + "\n");
            msg.AppendFormat("            RequestStatus:    " + Enum.GetName(typeof(ResponseStatus), response.Header.Flags) + "\n");
            msg.AppendFormat("            TrasactionId:     " + "0x" + response.Header.TransactionId.ToString("X2") + "\n");
            msg.AppendFormat("            PayloadSize:      " + response.Header.PayloadSize + "\n");
            msg.AppendFormat("        Payload:\n");
            msg.AppendFormat("            Data:             " + Utilities.ByteArray2String(response.Payload, true) + "\n");

            Debug.WriteLine(msg);
#endif
        }

        private IReadOnlyList<GattCharacteristic> GetCharacteristics(GattDeviceService gattService, string characteristicUuid)
        {
            var characteristics = gattService.GetCharacteristics(new Guid(characteristicUuid));
            return characteristics;
        }

        private async Task InitializeControlService(BluetoothLEDevice btleDevice)
        {
            if (_watcher3.ResultCollection.Any() && _watcher3.EnumCompleted)

            {
                foreach (DeviceInformationDisplay entry in _watcher3.ResultCollection)
                {
                    btleDevice3 = await BluetoothLEDevice.FromIdAsync(entry.Id);

                    // Here we check if we are using the same BTLE address as btleDevice: btleDevice2.BluetoothAddress == btleDevice3.BluetoothAddress
                    if (btleDevice3.BluetoothAddress != btleDevice.BluetoothAddress)
                    {
                        continue;
                    }

                    var guid = new Guid(UuidDefs.JAWBONE_CONTROL_SERVICE_UUID);
                    _gattControlService = btleDevice3.GetGattService(guid);

                    // Get the DateTime characteristics
                    _characteristicsControlDatetime = GetCharacteristics(_gattControlService, UuidDefs.JAWBONE_CONTROL_DATETIME_CHARACTERISTIC_UUID);
                    Debug.Assert(_characteristicsControlDatetime != null);

                    // Get the Connect Mode characteristics
                    _characteristicsControlConnectMode = GetCharacteristics(_gattControlService, UuidDefs.JAWBONE_CONTROL_CONNECT_MODE_CHARACTERISTIC_UUID);
                    Debug.Assert(_characteristicsControlConnectMode != null);

                    break;
                }
            }
        }

        // JAWBONE Control Service Data - DateTime to/from the band
        // JAWBONE Control Service Data - ConnectMode to/from the band
        private async Task InitializeGattStreamServiceCharateristics(DeviceInformationDisplay deviceInfoDisp)
        {
            _gattStreamService = await GattDeviceService.FromIdAsync(deviceInfoDisp.Id);
            Debug.WriteLine("\n" + _gattStreamService + ":   " + _gattStreamService.Device.Name + "\n");
            _rootPage.NotifyUser("Getting GATT Services", NotifyType.StatusMessage);

            // Get the Tx characteristic - We will get data from this Characteristics
            _characteristicsStreamTx = GetCharacteristics(_gattStreamService, UuidDefs.GATT_STREAM_TX_CHARACTERISTIC_UUID);
            Debug.Assert(_characteristicsStreamTx != null);


            // Set the Client Characteristic Configuration Descriptor to "Indicate"
            GattCommunicationStatus status = await _characteristicsStreamTx[0].WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);

            // Get the Rx characteristic - We will send data to this Characteristics
            _characteristicsStreamRx = GetCharacteristics(_gattStreamService, UuidDefs.GATT_STREAM_RX_CHARACTERISTIC_UUID);
            Debug.Assert(_characteristicsStreamRx != null);

            //Debug_ListCharacteristics();

            // This is the ValueChanged handler: Set an handler to the characteristicsTx
            _characteristicsStreamTx[0].ValueChanged += CharacteristicsTx_ValueChanged;
        }

        private void ReadPacketIn(byte[] buffer)
        {
#if DEBUG
            var msg = new StringBuilder();

            msg.AppendFormat("\n");
            msg.AppendFormat("Read Packet In:\n");
            msg.AppendFormat("===============\n");
            msg.AppendFormat("  TimeStamp: " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            msg.AppendFormat("    Packet Data:     " + Utilities.ByteArray2String(buffer, true) + "\n");
            msg.AppendFormat("    Sequence Number: " + buffer[0].ToString("00") + "\n");
#endif

            var packet = new byte[buffer.Length - 1];
            Buffer.BlockCopy(buffer, 1, packet, 0, buffer.Length - 1);

            // Here we do the Decryption of the data
#if DEBUG
            msg.AppendFormat("    Before Decrypt:  " + Utilities.ByteArray2String(packet, true) + "\n");
#endif
            Decrypt(packet, 0, packet.Length);

#if DEBUG
            msg.AppendFormat("    After Decrypt:   " + Utilities.ByteArray2String(packet, true) + "\n");
            Debug.WriteLine(msg);
#endif

            Buffer.BlockCopy(packet, 0, buffer, 1, buffer.Length - 1);

            if (_rawPacket == null)
            {
                _rawPacket = new RawPacket(packet, _reminder);
            }
            else
            {
                _rawPacket.Add(buffer);
            }

            if (_rawPacket.Reminder != null)
            {
                _reminder = new byte[_rawPacket.Reminder.Length];
                Buffer.BlockCopy(_rawPacket.Reminder, 0, _reminder, 0, _rawPacket.Reminder.Length);
            }
            else
            {
                _reminder = null;
            }

            if (_rawPacket.IsComplete)
            {
                lock (lockQueueResponsePackets)
                {
                    QueueResponsePackets.Add(_rawPacket);
                }
                _rawPacket = null;
            }
        }

        private async Task SaveTransactionIdsDictionary()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                if (_dictionaryTransactionIds != null)
                {
                    TransactionIds.Clear();
                    foreach (var item in _dictionaryTransactionIds)
                    {
                        var transactionIdItem = new TransactionIdItem
                        {
                            TransactionId = item.Key,
                            MessageType = item.Value
                        };

                        TransactionIds.Add(transactionIdItem);
                    }
                }

                if (_saveTransactionIdsBusy == 0)
                {
                    _saveTransactionIdsBusy = Interlocked.Increment(ref _saveTransactionIdsBusy);
                    await StorageHelper.WriteDataFileAsync("TransactionIds", TransactionIds);
                    _saveTransactionIdsBusy = Interlocked.Decrement(ref _saveTransactionIdsBusy);
                }
            });
        }

        private async Task SendAuthenticateCmd()
        {
            _messageType = (byte)PairingMsgTypes.Authenticate;              // 0x04
            _flags = 0x00;
            byte[] authenticationRequestPayload = _maskedPhoneChallengeZ;
            _payloadSize = (byte)authenticationRequestPayload.Length;
            var msgHeader = new TransactionHeader((PairingMsgTypes)_messageType, _flags, _transactionId, _payloadSize);
            var transaction = new Transaction(_characteristicsStreamRx[0], msgHeader, authenticationRequestPayload, false);
            await SaveTransaction(transaction);

            _rootPage.NotifyUser("3. Authenticate", NotifyType.StatusMessage);
        }

        private async Task SaveTransaction(Transaction transaction)
        {
            transaction.NoPackets = transaction.Payload.Length <= BtleLinkTypes.SIMPLE_TRANSACTION_MAX_PAYLOAD_SIZE ? 1 : transaction.CalcNoPackets();
            transaction.Status = TransactionStatus.TransactionWait;

            if (_dictionaryTransactionIds.ContainsKey(transaction.Header.TransactionId))
            {
                _dictionaryTransactionIds[transaction.Header.TransactionId] = (byte)transaction.Header.MessageType;
            }
            else
            {
                _dictionaryTransactionIds.Add(transaction.Header.TransactionId, (byte)transaction.Header.MessageType);
            }
            await SaveTransactionIdsDictionary();

#if DEBUG
            // For debug
            DumpTransaction(transaction);
#endif

            lock (lockQueueTransactions)
            {
                _transactionId++;
                QueueTransactions.Add(transaction);
            }
        }

        private async Task StartConnectionSequence()
        {
            _isConnecting = true;

            // 1. Get Protocol Version
            await Connecting_GetProtocolVersion_1();
        }

        private void Wacher2EventFired(object sender, BtleWatcherEventsArgs e)
        {
            Debug.WriteLine("Wacher2EventFired: " + Enum.GetName(typeof(WatcherEvents), e.Event));
        }
        #region Debug Utilities
        private static void DumpTransaction(Transaction transaction)
        {
#if DEBUG
            var msg = new StringBuilder();

            msg.AppendFormat("    Transaction Command: \n");
            msg.AppendFormat("        Header:\n");
            msg.AppendFormat("            NoPackets:        " + transaction.NoPackets + "\n");
            msg.AppendFormat("            MessageType:      " + "0x" + ((byte)transaction.Header.MessageType).ToString("X2") + "\n");
            msg.AppendFormat("            Flags:            " + "0x" + transaction.Header.Flags.ToString("X2") + "\n");
            msg.AppendFormat("            TrasactionId:     " + "0x" + transaction.Header.TransactionId.ToString("X2") + "\n");
            msg.AppendFormat("            PayloadSize:      " + transaction.Header.PayloadSize + "\n");
            msg.AppendFormat("        Payload:\n");
            msg.AppendFormat("            Data:             " + Utilities.ByteArray2String(transaction.Payload, true) + "\n");

            Debug.WriteLine(msg);
#endif
        }
        #endregion Debug Utilities
        [DataContract]
        public class EpochTimeItem
        {
            [DataMember]
            public uint EpochId
            {
                get; set;
            }


            [DataMember]
            public DateTime TimeStamp
            {
                get; set;
            }
        }


        [DataContract]
        public class TransactionIdItem
        {
            [DataMember]
            public byte MessageType
            {
                get; set;
            }

            [DataMember]
            public byte TransactionId
            {
                get; set;
            }
        }
    }
}
