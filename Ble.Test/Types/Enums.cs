﻿namespace Ble.Test.Types
{
    public class Enums
    {
        public enum WatcherEvents
        {
            Added = 0,                                                          // 0
            Updated = 1,                                                        // 1
            Removed = 2,                                                        // 2
            EnumerationCompleted = 3,                                           // 3
            Stopped = 4,                                                        // 4
        };

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        public enum HdrOffset
        {
            MessageType = 0,                                                    // 0
            Flags = 1,                                                          // 1
            TransactionID = 2,                                                  // 2
            PayloadSize = 3,                                                    // 3
        };

        public enum PairingMsgTypes
        {
            GetProtocolVersion = BtleLinkTypes.COMPONENTID_BTLESTREAM,          // 0
            SetConnectionSpeed = BtleLinkTypes.COMPONENTID_BTLESTREAM + 1,      // 1
            SpeedChangeComplete = BtleLinkTypes.COMPONENTID_BTLESTREAM + 2,     // 2
            KeyExchange = BtleLinkTypes.COMPONENTID_BTLESTREAM + 3,             // 3
            Authenticate = BtleLinkTypes.COMPONENTID_BTLESTREAM + 4,            // 4
            RespondToChallenge = BtleLinkTypes.COMPONENTID_BTLESTREAM + 5,      // 5
            EstablishSecureChannel = BtleLinkTypes.COMPONENTID_BTLESTREAM + 6,  // 6
            GetDeviceInfoVersion = BtleLinkTypes.COMPONENTID_BTLESTREAM + 7,    // 7

            BandNotConnected = 0xFE,                                            // 0xFE
            BandConnected = 0xFF                                                // 0xFF
        };

        public enum TransactionStatus
        {
            TransactionWait,                                                    // 0
            TransactionSent,                                                    // 1
            TransactionAck,                                                     // 2
        };

        public enum ResponseStatus
        {
            Success = 0,                                                        // 0x00 - 0
            Failed = -1,                                                        // 0xFF - 255
            Invalid = -2,                                                       // 0xFE - 254 
            ErrorInProgress = -3,                                               // 0xFD - 253
            Timeout = -4,                                                       // 0xFC - 252
            IncompleteResponse = -5,                                            // 0xFB - 251
            Disconnected = -6,                                                  // 0xFA - 250
            ProtocolMismatch = -7,                                              // 0xF9 - 249
            Canceled = -8,                                                      // 0xF8 - 248
            NotSupported = -9,                                                  // 0xF7 - 247
            FailedEncryption = -10,                                             // 0xF6 - 246
            ResponseNotAvailable = -11,                                         // 0xF5 - 245
            OtaOnlyConnection = -12,                                            // 0xF4 - 244
            IncorrectChecksum = -13                                             // 0xF4 - 243 
        };
    }
}
