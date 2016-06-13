namespace Ble.Test.Band
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Text;

    // ReSharper disable ValueParameterNotUsed
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global

    namespace BTLE.Misc
    {
        [DataContract]
        public class BandData : INotifyPropertyChanged
        {
            #region Definitions for the UI display
            private int _steps;
            [DataMember]
            public int Steps
            {
                get
                {
                    return _steps;
                }

                set
                {
                    _steps = value;
                    OnPropertyChanged("Steps");
                }
            }

            private bool _isPossibleBandRemoved;
            [DataMember]
            public bool IsPossibleBandRemoved
            {
                get
                {
                    return _isPossibleBandRemoved;
                }

                set
                {
                    _isPossibleBandRemoved = value;
                    OnPropertyChanged("IsPossibleBandRemoved");
                }
            }

            private bool _isBatteryCharging;
            [DataMember]
            public bool IsBatteryCharging
            {
                get
                {
                    return _isBatteryCharging;
                }

                set
                {
                    _isBatteryCharging = value;
                    OnPropertyChanged("IsBatteryCharging");
                }
            }

            private double _distanceInMeters;
            [DataMember]
            public double DistanceInMeters
            {
                get
                {
                    return _distanceInMeters;
                }
                set
                {
                    _distanceInMeters = value;
                    OnPropertyChanged("DistanceInMeters");
                }
            }

            private byte _medianHeartRate;
            [DataMember]
            public byte MedianHeartRate
            {
                get
                {
                    return _medianHeartRate;
                }
                set
                {
                    _medianHeartRate = value;
                    OnPropertyChanged("MedianHeartRate");
                }
            }

            private int _count;
            [DataMember]
            public int Count
            {
                get
                {
                    return _count;
                }
                set
                {
                    _count = value;
                    OnPropertyChanged("Count");
                }
            }

            private bool _isWalking;
            [DataMember]
            public bool IsWalking
            {
                get
                {
                    return _isWalking;
                }

                set
                {
                    _isWalking = value;
                    OnPropertyChanged("IsWalking");
                }
            }

            private bool _isConnected;
            [DataMember]
            public bool IsConnected
            {
                get
                {
                    return _isConnected;
                }

                set
                {
                    _isConnected = value;
                    OnPropertyChanged("IsConnected");
                }
            }

            private byte _battery;
            [DataMember]
            public byte Battery
            {
                get
                {
                    return _battery;
                }

                set
                {
                    _battery = value;
                    OnPropertyChanged("Battery");
                }
            }

            private DateTime _lastTickUpdateLocal;
            [IgnoreDataMember]
            public DateTime LastTickUpdateLocal
            {
                get
                {
                    return _lastTickUpdateLocal;
                }

                set
                {
                    _lastTickUpdateLocal = value;
                    OnPropertyChanged("LastTickUpdateLocal");
                }
            }

            private DateTime _lastTickUpdate;
            [DataMember]
            public DateTime LastTickUpdate
            {
                get
                {
                    return _lastTickUpdate;
                }

                set
                {
                    _lastTickUpdate = value;
                    LastTickUpdateLocal = _lastTickUpdate.ToLocalTime();
                    OnPropertyChanged("LastTickUpdate");
                }
            }

            public override string ToString()
            {
                var msg = new StringBuilder();

                msg.AppendFormat("\n");
                msg.AppendFormat("    Band Data\n");

                msg.AppendFormat("        Steps:                 " + Steps + "\n");
                msg.AppendFormat("        Distance:              " + DistanceInMeters + "\n");

                msg.AppendFormat("        Median HR:             " + MedianHeartRate + "\n");

                msg.AppendFormat("        Is Removed:            " + IsPossibleBandRemoved + "\n");
                msg.AppendFormat("        Is Charging:           " + IsBatteryCharging + "\n");

                msg.AppendFormat("        Last Tick:             " + LastTickUpdate.ToLocalTime() + "\n");

                msg.AppendFormat("        Count:                 " + Count + "\n");

                msg.AppendFormat("        Is Walking:            " + IsWalking + "\n");
                msg.AppendFormat("        Is Connected:          " + IsConnected + "\n");

                return msg.ToString();
            }

            public BandData()
            {
                Steps = 0;
                DistanceInMeters = 0.0;

                MedianHeartRate = 0;

                IsPossibleBandRemoved = false;
                IsBatteryCharging = false;

                Count = 0;

                IsWalking = false;
                IsConnected = false;

                LastTickUpdate = DateTime.Now.AddDays(-1);
            }
            #endregion


            #region INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}
