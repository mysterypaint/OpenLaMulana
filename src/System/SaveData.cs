using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq.Expressions;
using static OpenLaMulana.Global;
using static OpenLaMulana.System.GameFlags;

namespace OpenLaMulana.System
{
    // Special thanks to worsety for info on this class!
    public struct EncryptionBlock
    {
        public byte Key;
        public byte[] Data;
        public byte Checksum;

        public EncryptionBlock(byte key, byte[] data, byte checksum)
        {
            Key = key;
            Data = data;
            Checksum = checksum;
        }

        public int GetFinalChecksum()
        {
            return Key + Checksum;
        }
    }

    public enum EncryptionBlocks
    {
        Flags,          // data[870]
        Treasures,      // data[60]
        TreasuresMenu,  // data[40]
        BlockD,         // data[24]
        BlockE,         // data[5]
        BlockF,         // data[24]
        SubWeapons,     // data[10]
        Ammo,           // data[20]
        MaxHP32,        // data[1]
        Coins,          // data[2]
        Weights,        // data[2]
        GameTime,       // data[4]
        BlockM,         // data[4]
        Roms,           // data[336] // One 32-bit integer per rom, which is either a 0 or 1
        BlockO,         // data[4]
        BlockP,         // data[20]
        MAX
    };

    public class SaveData
    {
        public EncryptionBlock[] SaveBlocks = new EncryptionBlock[(int)EncryptionBlocks.MAX];
        public byte GlobalChecksum; //sum_of_checksums_and_keys_for_some_reason_i_think;
        public bool IsDecrypted;

        public SaveData(bool isDecrypted, EncryptionBlock[] allBlocks, byte globalChecksum = 0)
        {
            IsDecrypted = isDecrypted;

            // All EncryptionBlocks are Encrypted, then decrypted in place, with the exception of the RomBlock.
            SaveBlocks = allBlocks;
            GlobalChecksum = globalChecksum;
        }
    }
}
