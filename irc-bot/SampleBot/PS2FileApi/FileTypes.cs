using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lebot.PS2FileApi
{
    public enum Types
    {
        ADR,
        AGR,
        CDT,
        CNK0,
        CNK1,
        CNK2,
        CNK3,
        CRC,
        DDS,
        DMA,
        DME,
        DMV,
        DSK,
        ECO,
        FSB,
        FXO,
        GFX,
        LST,
        NSA,
        TXT,
        XML,
        ZONE,
        Unknown
    };

    class FileTypes
    {
        public static List<Types> TextFiles = new List<Types> 
        {
            Types.ADR,
            Types.TXT,
            Types.XML,
        };

        public static List<Types> ViewableFiles = new List<Types>
         {
            Types.DDS,
            Types.FSB,
            Types.DME,
            
         };

        public static List<Types> InterestingTypes = new List<Types>
        {
            Types.ADR,
            Types.ZONE, 
        };


    }
}
