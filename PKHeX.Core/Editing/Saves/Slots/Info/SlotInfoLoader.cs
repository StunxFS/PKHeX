using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace PKHeX.Core
{
    public static class SlotInfoLoader
    {
        // The "Add" method isn't shared for any interface... so we'll just do this twice.

        #region ConcurrentBag Implementation
        public static void AddFromSaveFile(SaveFile sav, ConcurrentBag<SlotCache> db)
        {
            if (sav.HasBox)
                AddBoxData(sav, db);

            if (sav.HasParty)
                AddPartyData(sav, db);

            AddExtraData(sav, db);
        }

        public static void AddFromLocalFile(string file, ConcurrentBag<SlotCache> db, ITrainerInfo dest, ICollection<string> validExtensions)
        {
            var fi = new FileInfo(file);
            if (!validExtensions.Contains(fi.Extension) || !PKX.IsPKM(fi.Length))
                return;

            var data = File.ReadAllBytes(file);
            var prefer = PKX.GetPKMFormatFromExtension(fi.Extension, dest.Generation);
            var pk = PKMConverter.GetPKMfromBytes(data, prefer);
            if (pk?.Species is not > 0)
                return;

            var info = new SlotInfoFile(file);
            var entry = new SlotCache(info, pk);
            db.Add(entry);
        }

        private static void AddBoxData(SaveFile sav, ConcurrentBag<SlotCache> db)
        {
            var bd = sav.BoxData;
            var bc = sav.BoxCount;
            var sc = sav.BoxSlotCount;
            int ctr = 0;
            for (int box = 0; box < bc; box++)
            {
                for (int slot = 0; slot < sc; slot++, ctr++)
                {
                    var ident = new SlotInfoBox(box, slot);
                    var result = new SlotCache(ident, bd[ctr], sav);
                    db.Add(result);
                }
            }
        }

        private static void AddPartyData(SaveFile sav, ConcurrentBag<SlotCache> db)
        {
            var pd = sav.PartyData;
            for (var index = 0; index < pd.Count; index++)
            {
                var pk = pd[index];
                if (pk.Species == 0)
                    continue;

                var ident = new SlotInfoParty(index);
                var result = new SlotCache(ident, pk, sav);
                db.Add(result);
            }
        }

        private static void AddExtraData(SaveFile sav, ConcurrentBag<SlotCache> db)
        {
            var extra = sav.GetExtraSlots(true);
            foreach (var x in extra)
            {
                var pk = x.Read(sav);
                if (pk.Species == 0)
                    continue;

                var result = new SlotCache(x, pk, sav);
                db.Add(result);
            }
        }
        #endregion

        #region ICollection Implementation
        public static void AddFromSaveFile(SaveFile sav, ICollection<SlotCache> db)
        {
            if (sav.HasBox)
                AddBoxData(sav, db);

            if (sav.HasParty)
                AddPartyData(sav, db);

            AddExtraData(sav, db);
        }

        public static void AddFromLocalFile(string file, ICollection<SlotCache> db, ITrainerInfo dest, ICollection<string> validExtensions)
        {
            var fi = new FileInfo(file);
            if (!validExtensions.Contains(fi.Extension) || !PKX.IsPKM(fi.Length))
                return;

            var data = File.ReadAllBytes(file);
            var prefer = PKX.GetPKMFormatFromExtension(fi.Extension, dest.Generation);
            var pk = PKMConverter.GetPKMfromBytes(data, prefer);
            if (pk?.Species is not > 0)
                return;

            var info = new SlotInfoFile(file);
            var entry = new SlotCache(info, pk);
            db.Add(entry);
        }

        public static void AddBoxData(SaveFile sav, ICollection<SlotCache> db)
        {
            var bd = sav.BoxData;
            var bc = sav.BoxCount;
            var sc = sav.BoxSlotCount;
            int ctr = 0;
            for (int box = 0; box < bc; box++)
            {
                for (int slot = 0; slot < sc; slot++, ctr++)
                {
                    var ident = new SlotInfoBox(box, slot);
                    var result = new SlotCache(ident, bd[ctr], sav);
                    db.Add(result);
                }
            }
        }

        public static void AddPartyData(SaveFile sav, ICollection<SlotCache> db)
        {
            var pd = sav.PartyData;
            for (var index = 0; index < pd.Count; index++)
            {
                var pk = pd[index];
                if (pk.Species == 0)
                    continue;

                var ident = new SlotInfoParty(index + 1);
                var result = new SlotCache(ident, pk, sav);
                db.Add(result);
            }
        }

        private static void AddExtraData(SaveFile sav, ICollection<SlotCache> db)
        {
            var extra = sav.GetExtraSlots(true);
            foreach (var x in extra)
            {
                var pk = x.Read(sav);
                if (pk.Species == 0)
                    continue;

                var result = new SlotCache(x, pk, sav);
                db.Add(result);
            }
        }
        #endregion
    }
}
