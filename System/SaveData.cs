using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLaMulana.System
{
    // Special thanks to worsety for info on this class!
    class SaveData
    {
        struct EncryptionBlock
        {
            public byte Key;
            public byte[] Data;
            public byte Checksum;
            public EncryptionBlock(byte[] inData, int offset, int len)
            {
                Key = inData[offset];
                Data = new byte[len];
                for (int i = 1; i < len; i++)
                {
                    Data[i - 1] = inData[i];
                }
                Checksum = inData[offset + len + 1];
            }
        }

        enum SAVE_REGIONS {
            FLAGS,
            TREASURES,
            TREASURESMENU,
            BLOCK_D,
            BLOCK_E,
            BLOCK_F,
            SUBWEAPONS,
            AMMO,
            MAXHP_32,
            COINS,
            WEIGHTS,
            GAMETIME,
            BLOCK_M,
            ROMS,
            BLOCK_O,
            BLOCK_P,
            MAX
        };

        private const int SAVE_SIZE = 1459; // 0x5B3 bytes of data
        byte globalKey;

        EncryptionBlock[] saveFile;

        public void ReadEncryptedSave(string fileName)
        {
            saveFile = new EncryptionBlock[(int)SAVE_REGIONS.MAX];

            int[] regionSizes = { 870, 60, 40, 24, 5, 24, 10, 20, 1, 2, 2, 4, 4, 336, 4, 20};

            byte[] data = File.ReadAllBytes(fileName);
            
            int i = 0;
            
            for (int offset = 0; offset < data.Length; offset++)
            {
                if (i >= saveFile.Length)
                {
                    globalKey = data[offset];
                    break;
                }
                saveFile[i] = SetEncryptionBlock(saveFile[i], offset, data, regionSizes[i]);
                offset += regionSizes[i] + 1;
                i++;
            }

            // Encrypted Data is now in memory. Now, to decrypt it:


            string PassOrFail(bool checkingVal) => checkingVal == true ? "PASS" : "FAIL";

            for (i = 0; i < saveFile.Length; i++)
            {
                bool verified = DecryptSaveBlock(i);

                Debug.WriteLine("Verification for Save Block #{0}: " + PassOrFail(verified), i);
            }
        }

        public void WriteDecryptedSave(string fileName)
        {
            byte[] outData = new byte[SAVE_SIZE];

            var offset = 0;
            for (var i = 0; i < saveFile.Length; i++)
            {
                outData[offset] = saveFile[i].Key;
                offset++;

                var data = saveFile[i].Data;
                for (var j = 0; j < data.Length; j++)
                {
                    outData[offset] = data[j];
                    offset++;
                }

                outData[offset] = saveFile[i].Checksum;
                offset++;
            }
            outData[offset] = GetGlobalEncryptionKey();


            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(outData, 0, outData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
            }
        }

        public void WriteEncryptedSave(string fileName)
        {
            byte[] outData = new byte[SAVE_SIZE];

            int i = 0;
            foreach (EncryptionBlock eb in saveFile)
            {
                saveFile[i] = EncryptSaveBlock(eb);
                i++;
            }

            var offset = 0;
            for (i = 0; i < saveFile.Length; i++)
            {
                outData[offset] = saveFile[i].Key;
                offset++;

                var data = saveFile[i].Data;
                for (var j = 0; j < data.Length; j++)
                {
                    outData[offset] = data[j];
                    offset++;
                }

                outData[offset] = saveFile[i].Checksum;
                offset++;
            }
            outData[offset] = GetGlobalEncryptionKey();

            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(outData, 0, outData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
            }
        }

        private byte GetGlobalEncryptionKey()
        {
            //sum_of_checksums_and_keys_for_some_reason_i_think
            int result = 0;

            foreach (EncryptionBlock eb in saveFile)
            {
                result += (eb.Checksum + eb.Key);
            }

            return (byte) result;
        }

        private EncryptionBlock SetEncryptionBlock(EncryptionBlock saveRegion, int offset, byte[] data, int blockSize)
        {
            saveRegion.Key = data[offset];
            saveRegion.Data = new byte[blockSize];
            for (int i = 0; i < blockSize; i++)
            {
                saveRegion.Data[i] = data[offset + i + 1];
            }
            saveRegion.Checksum = data[offset + blockSize + 1];
            return saveRegion;
        }

        bool DecryptSaveBlock(int blockIndex)
        {
            byte state = saveFile[blockIndex].Key;
            byte checksum = 0x0;
            for (int i = 0; i < saveFile[blockIndex].Data.Length; i++)
            {
                state = (byte)(109 * state + 1021);
                saveFile[blockIndex].Data[i] ^= state;
                checksum += (byte)(i + saveFile[blockIndex].Data[i]);
            }

            return checksum == saveFile[blockIndex].Checksum;
        }
        private EncryptionBlock EncryptSaveBlock(EncryptionBlock eb)
        {
            // TODO: Do the exact opposite of the DecryptSaveBlock() function above
            return eb;
        }
    }
}
