using Ble.Test.Utility;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace Ble.Test.Band
{
    [DataContract]
    public class BandInfo : INotifyPropertyChanged
    {
        #region Definitions for the UI display
        private string _modelNo;
        [DataMember]
        public string ModelNo
        {
            get
            {
                return _modelNo;
            }
            set
            {
                _modelNo = value;
                OnPropertyChanged("ModelNo");
            }
        }

        private string _serialNo;
        [DataMember]
        public string SerialNo
        {
            get
            {
                return _serialNo;
            }
            set
            {
                _serialNo = value;
                OnPropertyChanged("SerialNo");
            }
        }

        private string _patternConfig;
        [DataMember]
        public string PatternConfig
        {
            get
            {
                return _patternConfig;
            }
            set
            {
                _patternConfig = value;
                OnPropertyChanged("PatternConfig");
            }
        }

        private string _pcbVersion;
        [DataMember]
        public string PcbVersion
        {
            get
            {
                return _pcbVersion;
            }
            set
            {
                _pcbVersion = value;
                OnPropertyChanged("PcbVersion");
            }
        }

        private string _yearCode;
        [DataMember]
        public string YearCode
        {
            get
            {
                return _yearCode;
            }
            set
            {
                _yearCode = value;
                OnPropertyChanged("YearCode");
            }
        }

        private string _weekCode;
        [DataMember]
        public string WeekCode
        {
            get
            {
                return _weekCode;
            }
            set
            {
                _weekCode = value;
                OnPropertyChanged("WeekCode");
            }
        }

        private string _serial;
        [DataMember]
        public string Serial
        {
            get
            {
                return _serial;
            }
            set
            {
                _serial = value;
                OnPropertyChanged("Serial");
            }
        }

        private string _fwCode;
        [DataMember]
        public string FwCode
        {
            get
            {
                return _fwCode;
            }
            set
            {
                _fwCode = value;
                OnPropertyChanged("FwCode");
            }
        }

        private string _size;
        [DataMember]
        public string Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                OnPropertyChanged("Size");
            }
        }

        private string _bandColor;
        [DataMember]
        public string BandColor
        {
            get
            {
                return _bandColor;
            }
            set
            {
                _bandColor = value;
                OnPropertyChanged("BandColor");
            }
        }

        private string _productCode;
        [DataMember]
        public string ProductCode
        {
            get
            {
                return _productCode;
            }
            set
            {
                _productCode = value;
                OnPropertyChanged("ProductCode");
            }
        }

        private string _firmwareVer;
        [DataMember]
        public string FirmwareVer
        {
            get
            {
                return _firmwareVer;
            }
            set
            {
                _firmwareVer = value;
                OnPropertyChanged("FirmwareVer");
            }
        }

        private string _softwareVer;
        [DataMember]
        public string SoftwareVer
        {
            get
            {
                return _softwareVer;
            }
            set
            {
                _softwareVer = value;
                OnPropertyChanged("SoftwareVer");
            }
        }

        private string _manufacturer;
        [DataMember]
        public string Manufacturer
        {
            get
            {
                return _manufacturer;
            }
            set
            {
                _manufacturer = value;
                OnPropertyChanged("Manufacturer");
            }
        }

        private string _id;
        [DataMember]
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        private string _name;
        [DataMember]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private ulong _btleAddress;
        [DataMember]
        public ulong BtleAddress
        {
            get
            {
                return _btleAddress;
            }
            set
            {
                _btleAddress = value;
                OnPropertyChanged("BtleAddress");
            }
        }


        public string Address => BtleAddress != 0x00 ? Utilities.BtleAddress2String(BtleAddress) : "xx-xx-xx-xx-xx-xx";

        public override string ToString()
        {
            var msg = new StringBuilder();

            msg.AppendFormat("\n");
            msg.AppendFormat("    Band Info\n");
            msg.AppendFormat("        Model No:              " + ModelNo + "\n");
            msg.AppendFormat("        Pattern Config:        " + PatternConfig + "\n");
            msg.AppendFormat("        Serial No:             " + SerialNo + "\n");
            msg.AppendFormat("        PCB Version:           " + PcbVersion + "\n");
            msg.AppendFormat("        Year Code:             " + YearCode + "\n");
            msg.AppendFormat("        WeekCode:              " + WeekCode + "\n");
            msg.AppendFormat("        Serial:                " + Serial + "\n");
            msg.AppendFormat("        FW Code:               " + FwCode + "\n");
            msg.AppendFormat("        Size:                  " + Size + "\n");
            msg.AppendFormat("        Band Color:            " + BandColor + "\n");
            msg.AppendFormat("        ProductCode:           " + ProductCode + "\n");
            msg.AppendFormat("        Firmaware Version:     " + FirmwareVer + "\n");
            msg.AppendFormat("        Software Version:      " + SoftwareVer + "\n");
            msg.AppendFormat("        Manufacturer:          " + Manufacturer + "\n");

            msg.AppendFormat("        Name:                  " + Name + "\n");
            msg.AppendFormat("        Id:                    " + Id + "\n");
            msg.AppendFormat("        BTLE Address:          " + Address + "\n");

            return msg.ToString();
        }

        public BandInfo()
        {
            ModelNo = string.Empty;
            PatternConfig = string.Empty;
            SerialNo = string.Empty;
            PcbVersion = string.Empty;
            YearCode = string.Empty;
            WeekCode = string.Empty;
            Serial = string.Empty;
            FwCode = string.Empty;
            Size = string.Empty;
            BandColor = string.Empty;
            ProductCode = string.Empty;
            FirmwareVer = string.Empty;
            SoftwareVer = string.Empty;
            Manufacturer = string.Empty;

            Name = string.Empty;
            Id = string.Empty;
            BtleAddress = 0x00;
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
