using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cluster.Famicom
{
    public class UnifFile
    {
        public Dictionary<string, byte[]> Fields = new Dictionary<string, byte[]>();
        public int Version;

        public UnifFile(string fileName)
        {
            var file = File.ReadAllBytes(fileName);
            var header = new byte[32];
            Array.Copy(file, header, 32);
            if (header[0] != 'U' || header[1] != 'N' || header[2] != 'I' || header[3] != 'F')
                throw new Exception("Invalid UNIF file");
            Version = header[4] | (header[5] << 8) | (header[6] << 16) | (header[7] << 24);
            int pos = 32;
            while (pos < file.Length)
            {
                var type = Encoding.UTF8.GetString(file, pos, 4);
                pos += 4;
                int length = file[pos] | (file[pos + 1] << 8) | (file[pos + 2] << 16) | (file[pos + 3] << 24);
                pos += 4;
                var data = new byte[length];
                Array.Copy(file, pos, data, 0, length);
                Fields[type] = data;
                pos += length;
            }
        }

        public UnifFile()
        {
        }

        public void Save(string fileName)
        {
            var data = new List<byte>();
            var header = new byte[32];
            Array.Copy(Encoding.UTF8.GetBytes("UNIF"), header, 4);
            header[4] = (byte)(Version & 0xFF);
            header[5] = (byte)((Version >> 8) & 0xFF);
            header[6] = (byte)((Version >> 16) & 0xFF);
            header[7] = (byte)((Version >> 24) & 0xFF);
            data.AddRange(header);

            var fields = new Dictionary<string, byte[]>(Fields);

            if (!fields.ContainsKey("DINF") && Version >= 2)
            {
                var dinf = new byte[204];
                var name = UnifFile.StringToUTF8N("Cluster / clusterrr@clusterrr.com / http://clusterrr.com");
                Array.Copy(name, dinf, name.Length);
                var dt = DateTime.Now;
                dinf[100] = (byte)dt.Month;
                dinf[101] = (byte)dt.Day;
                dinf[102] = (byte)(dt.Year & 0xFF);
                dinf[103] = (byte)(dt.Year >> 8);
                var software = UnifFile.StringToUTF8N("My own software and hardware");
                Array.Copy(software, 0, dinf, 104, software.Length);
                fields["DINF"] = dinf;
            }

            /*
            if (Version >= 5)
            {
                for (int p = 0; p < 16; p++)
                {
                    if (fields.ContainsKey(string.Format("PRG{0:X1}", p)) && !fields.ContainsKey(string.Format("PCK{0:X1}", p)))
                        fields[string.Format("PCK{0:X1}", p)] = CRC32(fields[string.Format("PRG{0:X1}", p)]);
                    if (fields.ContainsKey(string.Format("CHR{0:X1}", p)) && !fields.ContainsKey(string.Format("CCK{0:X1}", p)))
                        fields[string.Format("CCK{0:X1}", p)] = CRC32(fields[string.Format("CHR{0:X1}", p)]);
                }
            }
            */

            foreach (var name in fields.Keys)
            {
                data.AddRange(Encoding.UTF8.GetBytes(name));
                int len = fields[name].Length;
                data.Add((byte)(len & 0xFF));
                data.Add((byte)((len >> 8) & 0xFF));
                data.Add((byte)((len >> 16) & 0xFF));
                data.Add((byte)((len >> 24) & 0xFF));
                data.AddRange(fields[name]);
            }

            File.WriteAllBytes(fileName, data.ToArray());
        }

        public static byte[] StringToUTF8N(string value)
        {
            var str = Encoding.UTF8.GetBytes(value);
            var result = new byte[str.Length + 1];
            Array.Copy(str, result, str.Length);
            return result;
        }

        public string Mapper
        {
            get
            {
                return Encoding.UTF8.GetString(Fields["MAPR"], 0, Fields["MAPR"].Length - 1);
            }
            set
            {
                Fields["MAPR"] = StringToUTF8N(value);
            }
        }
        static byte[] CRC32(byte[] data)
        {
            uint poly = 0xedb88320;
            uint[] table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
            uint crc = 0xffffffff;
            for (int i = 0; i < data.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ data[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            crc = ~crc;
            // Which one is correct? 
            return new byte[4] { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF), (byte)((crc >> 16) & 0xFF), (byte)((crc >> 24) & 0xFF) };
            //return new byte[4] { (byte)((crc >> 24) & 0xFF), (byte)((crc >> 16) & 0xFF), (byte)((crc >> 8) & 0xFF), (byte)(crc & 0xFF) };
        }
    }
}
