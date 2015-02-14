using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lebot.PS2FileApi
{
    public class PS2Analyzer
    {
        public static string AnalyzePS2(string directory)
        {

            string assets = directory + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar;

            StringBuilder builder = new StringBuilder();

            foreach(string file in Directory.GetFiles(assets))
            {
                Console.WriteLine(file);
                using(BinaryReaderBigEndian reader = new BinaryReaderBigEndian(File.Open(file, FileMode.Open)))
                {

                    int nextOffset;

                    do
                    {
                        nextOffset = reader.ReadInt32();
                        int numFiles = reader.ReadInt32();

                        for (int i = 0; i < numFiles; i++)
                        {
                            int nameLength = reader.ReadInt32();
                            string name = new string(reader.ReadChars(nameLength));
                            int offset = reader.ReadInt32();
                            int length = reader.ReadInt32();
                            uint crc32 = reader.ReadUInt32();


                            builder.AppendLine(string.Format("{0}\t{1}\t{2}", name, length, (uint)crc32));

                        }

                        reader.BaseStream.Seek(nextOffset, SeekOrigin.Begin);
                       
                    } while (nextOffset != 0);

                }


            }
            return builder.ToString();
        }



    }
}
