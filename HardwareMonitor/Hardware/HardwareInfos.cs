using Hardware.Info;
using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using OpenHardwareMonitor.Hardware;

namespace ConsoleService.Hardware
{
    internal class HardwareInfos
    {
        private IHardwareInfo _hardwareInfo = new HardwareInfo();
        public int Disk { get; set; }
        public int RAM { get; set; }
        public int CPU { get; set; }
        public int TMP { get; set; }

        private Dictionary<string, int> _disks;
        
        public Dictionary<string, int> GetHardDisksValues()
        {
            Dictionary<string, int> disks = new Dictionary<string, int>();

            _hardwareInfo.RefreshDriveList();

            foreach (var drive in _hardwareInfo.DriveList)
            {
                foreach (var partition in drive.PartitionList)
                {
                    foreach (var volume in partition.VolumeList)
                    {
                        disks.Add(volume.Name, byteToGB(volume.Size - volume.FreeSpace));
                    }
                }
            }

            _disks = disks;

            return _disks;
        }

        public int GetTotalHardDisksValue()
        {
            ulong _diskSize = new ulong();
            ulong _diskFreeSize = new ulong();
            _hardwareInfo.RefreshDriveList();
            

            foreach (var drive in _hardwareInfo.DriveList)
            {
                foreach (var partition in drive.PartitionList)
                {
                    foreach (var volume in partition.VolumeList)
                    {
                        _diskSize += volume.Size;
                        _diskFreeSize += volume.FreeSpace;
                    }
                }
            }

            Disk = byteToGB(_diskSize - _diskFreeSize);

            return Disk;
        }

        public int GetMemoryValue()
        {
            _hardwareInfo.RefreshMemoryStatus();

            RAM = byteToGB(_hardwareInfo.MemoryStatus.TotalPhysical - _hardwareInfo.MemoryStatus.AvailablePhysical);

            return RAM;
        }

        public int GetCpuPercent()
        {
            _hardwareInfo.RefreshCPUList();

            CPU = _hardwareInfo.CpuList.Sum(x=>(int)x.PercentProcessorTime);
            
            return CPU;
        }    

        public  int GetCpuTemperature()
        {

            Computer computer = new Computer();

            computer.CPUEnabled = true;

            computer.Open();

            foreach (var item in computer.Hardware)
            {
                foreach (var sensor in item.Sensors)
                {
                    if (sensor.Identifier.ToString().Contains("temperature") && sensor.Name.Equals("CPU Package"))
                    {
                        TMP = (int)sensor.Value;
                    }
                }
                item.Update();
            }

            computer.Close();

            return TMP;
        }

        public int byteToGB(ulong value)
        {
            var v = value / 1024 / 1024 / 1024;

            return (int)v;
        }

        public void RefreshAll()
        {
            GetMemoryValue();
            GetCpuPercent();
            GetTotalHardDisksValue();
            GetCpuTemperature();
        }
    }
}
