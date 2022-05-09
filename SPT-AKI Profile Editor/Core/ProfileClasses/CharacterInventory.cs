﻿using Newtonsoft.Json;
using SPT_AKI_Profile_Editor.Core.Enums;
using SPT_AKI_Profile_Editor.Core.ServerClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPT_AKI_Profile_Editor.Core.ProfileClasses
{
    public class CharacterInventory : BindableEntity
    {
        private InventoryItem[] items;
        private InventoryItemExtended[] inventoryItems;
        private string stash;
        private string equipment;

        [JsonProperty("items")]
        public InventoryItem[] Items
        {
            get => items;
            set
            {
                items = value;
                OnPropertyChanged("Items");
                GetInventoryItems();
                OnPropertyChanged("DollarsCount");
                OnPropertyChanged("RublesCount");
                OnPropertyChanged("EurosCount");
                OnPropertyChanged("ContainsModdedItems");
            }
        }

        [JsonProperty("stash")]
        public string Stash
        {
            get => stash;
            set
            {
                stash = value;
                OnPropertyChanged("Stash");
            }
        }

        [JsonProperty("equipment")]
        public string Equipment
        {
            get => equipment;
            set
            {
                equipment = value;
                OnPropertyChanged("Equipment");
            }
        }

        [JsonIgnore]
        public string Pockets
        {
            get => Items?
                .Where(x => x.IsPockets)
                .FirstOrDefault()?.Tpl;
            set
            {
                var pocketsSlot = Items?
                .Where(x => x.IsPockets)
                .FirstOrDefault();
                if (pocketsSlot != null)
                {
                    pocketsSlot.Tpl = value;
                    OnPropertyChanged("Pockets");
                }
            }
        }

        [JsonIgnore]
        public string DollarsCount => GetMoneyCountString(AppData.AppSettings.MoneysDollarsTpl);

        [JsonIgnore]
        public string RublesCount => GetMoneyCountString(AppData.AppSettings.MoneysRublesTpl);

        [JsonIgnore]
        public string EurosCount => GetMoneyCountString(AppData.AppSettings.MoneysEurosTpl);

        [JsonIgnore]
        public InventoryItemExtended[] InventoryItems
        {
            get
            {
                if (inventoryItems == null || (inventoryItems.Length == 0 && Items.Length != 0))
                    GetInventoryItems();
                return inventoryItems;
            }
            set
            {
                inventoryItems = value;
                OnPropertyChanged("InventoryItems");
            }
        }

        [JsonIgnore]
        public bool ContainsModdedItems => InventoryItems.Any(x => x.IsAddedByMods);

        [JsonIgnore]
        public bool InventoryHaveDuplicatedItems => GroupedInventory.Any();

        [JsonIgnore]
        public InventoryItem FirstPrimaryWeapon => GetEquipment(AppData.AppSettings.FirstPrimaryWeaponSlotId);

        [JsonIgnore]
        public InventoryItem Headwear => GetEquipment(AppData.AppSettings.HeadwearSlotId);

        [JsonIgnore]
        public InventoryItem TacticalVest => GetEquipment(AppData.AppSettings.TacticalVestSlotId);

        [JsonIgnore]
        public InventoryItem SecuredContainer => GetEquipment(AppData.AppSettings.SecuredContainerSlotId);

        [JsonIgnore]
        public InventoryItem Backpack => GetEquipment(AppData.AppSettings.BackpackSlotId);

        [JsonIgnore]
        public InventoryItem Earpiece => GetEquipment(AppData.AppSettings.EarpieceSlotId);

        private List<string> GroupedInventory => Items?
            .GroupBy(x => x.Id)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        public void RemoveDuplicatedItems() => FinalRemoveItems(GroupedInventory);

        public void RemoveItems(List<string> itemIds) => FinalRemoveItems(itemIds);

        public void RemoveAllItems() => FinalRemoveItems(GetCompleteItemsList(InventoryItems.Select(x => x.Id)));

        public void AddNewItems(string tpl, int count, bool fir)
        {
            var mItem = AppData.ServerDatabase.ItemsDB[tpl];
            int[,] Stash = GetPlayerStashSlotMap();
            List<string> iDs = Items.Select(x => x.Id).ToList();
            List<InventoryItem> items = Items.ToList();
            int stacks = count / mItem.Properties.StackMaxSize;
            if (mItem.Properties.StackMaxSize * stacks < count) stacks++;
            List<ItemLocation> NewItemsLocations = GetItemLocations(mItem, Stash, stacks);
            if (NewItemsLocations == null)
                throw new Exception(AppData.AppLocalization.GetLocalizedString("tab_stash_no_slots"));
            for (int i = 0; i < NewItemsLocations.Count; i++)
            {
                if (count <= 0) break;
                string id = ExtMethods.GenerateNewId(iDs.ToArray());
                iDs.Add(id);
                var newItem = new InventoryItem
                {
                    Id = id,
                    ParentId = this.Stash,
                    SlotId = "hideout",
                    Tpl = mItem.Id,
                    Location = new ItemLocation { R = NewItemsLocations[i].R, X = NewItemsLocations[i].X, Y = NewItemsLocations[i].Y, IsSearched = true },
                    Upd = new ItemUpd { StackObjectsCount = count > mItem.Properties.StackMaxSize ? mItem.Properties.StackMaxSize : count, SpawnedInSession = fir }
                };
                items.Add(newItem);
                count -= mItem.Properties.StackMaxSize;
            }
            Items = items.ToArray();
        }

        public int[,] GetPlayerStashSlotMap()
        {
            int[,] Stash2D = CreateStash2D();
            foreach (var item in InventoryItems)
            {
                (int itemWidth, int itemHeight) = GetSizeOfInventoryItem(item);
                int rotatedHeight = item.Location.R == ItemRotation.Vertical ? itemWidth : itemHeight;
                int rotatedWidth = item.Location.R == ItemRotation.Vertical ? itemHeight : itemWidth;
                for (int y = 0; y < rotatedHeight; y++)
                {
                    try
                    {
                        for (int z = item.Location.X; z < item.Location.X + rotatedWidth; z++)
                            Stash2D[item.Location.Y + y, z] = 1;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to insert item with id {item.Id} to Stash2D: {ex.Message}");
                    }
                }
            }
            return Stash2D;
        }

        private static List<ItemLocation> GetFreeSlots(int[,] Stash)
        {
            List<ItemLocation> locations = new();
            for (int y = 0; y < Stash.GetLength(0); y++)
                for (int x = 0; x < Stash.GetLength(1); x++)
                    if (Stash[y, x] == 0)
                        locations.Add(new ItemLocation { X = x, Y = y, R = ItemRotation.Horizontal });
            return locations;
        }

        private static List<ItemLocation> GetItemLocations(TarkovItem mItem, int[,] Stash, int stacks)
        {
            List<ItemLocation> freeSlots = GetFreeSlots(Stash);
            if (freeSlots.Count < mItem.Properties.Width * mItem.Properties.Height * stacks)
                return null;
            List<ItemLocation> NewItemsLocations = new();
            foreach (var slot in freeSlots)
            {
                if (mItem.Properties.Width == 1 && mItem.Properties.Height == 1)
                    NewItemsLocations.Add(slot);
                else
                {
                    ItemLocation itemLocation = GetItemLocation(mItem.Properties.Height, mItem.Properties.Width, Stash, slot);
                    if (itemLocation != null)
                        NewItemsLocations.Add(itemLocation);
                    if (NewItemsLocations.Count == stacks)
                        return NewItemsLocations;
                    itemLocation = GetItemLocation(mItem.Properties.Width, mItem.Properties.Height, Stash, slot);
                    if (itemLocation != null)
                    {
                        itemLocation.R = ItemRotation.Vertical;
                        NewItemsLocations.Add(itemLocation);
                    }
                }
                if (NewItemsLocations.Count == stacks)
                    return NewItemsLocations;
            }
            return null;
        }

        private static ItemLocation GetItemLocation(int Width, int Height, int[,] Stash, ItemLocation slot)
        {
            int size = 0;
            for (int y = 0; y < Width; y++)
            {
                if (slot.X + Height < Stash.GetLength(1) && slot.Y + y < Stash.GetLength(0))
                    for (int z = slot.X; z < slot.X + Height; z++)
                        if (Stash[slot.Y + y, z] == 0) size++;
            }
            if (size == Width * Height)
            {
                for (int y = 0; y < Width; y++)
                {
                    for (int z = slot.X; z < slot.X + Height; z++)
                        Stash[slot.Y + y, z] = 1;
                }
                return new ItemLocation { X = slot.X, Y = slot.Y };
            }
            return null;
        }

        private void GetInventoryItems()
        {
            InventoryItems = Items?
                .Where(x => x.ParentId == Stash && x.Location != null)?
                .Select(x => new InventoryItemExtended(x, Items))
                .ToArray();
        }

        private InventoryItem GetEquipment(string slotId)
        {
            return Items?.Where(x => x.ParentId == Equipment && x.SlotId == slotId)?.FirstOrDefault();
        }

        private List<string> GetCompleteItemsList(IEnumerable<string> items)
        {
            List<string> itemIds = new();
            foreach (var TargetItem in items)
            {
                List<string> toDo = new() { TargetItem };
                while (toDo.Count > 0)
                {
                    foreach (var item in Items.Where(x => x.ParentId == toDo.ElementAt(0)))
                        toDo.Add(item.Id);
                    itemIds.Add(toDo.ElementAt(0));
                    toDo.Remove(toDo.ElementAt(0));
                }
            }
            return itemIds;
        }

        private void FinalRemoveItems(List<string> itemIds)
        {
            List<InventoryItem> ItemsList = Items.ToList();
            while (itemIds.Count > 0)
            {
                var item = ItemsList.Where(x => x.Id == itemIds[0]).FirstOrDefault();
                if (item != null)
                    ItemsList.Remove(item);
                else
                    itemIds.RemoveAt(0);
            }
            Items = ItemsList.ToArray();
        }

        private int[,] CreateStash2D()
        {
            InventoryItem ProfileStash = Items.Where(x => x.Id == Stash).FirstOrDefault();
            Grid stashTPL = AppData.ServerDatabase.ItemsDB[ProfileStash.Tpl].Properties.Grids.First();
            int stashX = stashTPL.Props.CellsH == 0 ? 10 : stashTPL.Props.CellsH;
            int stashY = stashTPL.Props.CellsV == 0 ? 66 : stashTPL.Props.CellsV;
            return new int[stashY, stashX];
        }

        private (int iW, int iH) GetSizeOfInventoryItem(InventoryItem inventoryItem)
        {
            List<InventoryItem> toDo = new() { inventoryItem };
            TarkovItem tmpItem = AppData.ServerDatabase.ItemsDB[inventoryItem.Tpl];
            InventoryItem rootItem = Items.Where(x => x.ParentId == inventoryItem.Id).FirstOrDefault();
            bool FoldableWeapon = tmpItem.Properties.Foldable;
            string FoldedSlot = tmpItem.Properties.FoldedSlot;

            int SizeUp = 0;
            int SizeDown = 0;
            int SizeLeft = 0;
            int SizeRight = 0;

            int ForcedUp = 0;
            int ForcedDown = 0;
            int ForcedLeft = 0;
            int ForcedRight = 0;
            int outX = tmpItem.Properties.Width;
            int outY = tmpItem.Properties.Height;
            if (rootItem != null)
            {
                List<string> skipThisItems = new() { "5448e53e4bdc2d60728b4567", "566168634bdc2d144c8b456c", "5795f317245977243854e041" };
                bool rootFolded = rootItem.Upd != null && rootItem.Upd.Foldable != null && rootItem.Upd.Foldable.Folded;

                if (FoldableWeapon && string.IsNullOrEmpty(FoldedSlot) && rootFolded)
                    outX -= tmpItem.Properties.SizeReduceRight;

                if (!skipThisItems.Contains(tmpItem.Parent))
                {
                    while (toDo.Count > 0)
                    {
                        if (toDo.ElementAt(0) != null)
                        {
                            foreach (var item in Items.Where(x => x.ParentId == toDo.ElementAt(0).Id))
                            {
                                if (!item.SlotId.Contains("mod_"))
                                    continue;
                                toDo.Add(item);
                                TarkovItem itm = AppData.ServerDatabase.ItemsDB[item.Tpl];
                                bool childFoldable = itm.Properties.Foldable;
                                bool childFolded = item.Upd != null && item.Upd.Foldable != null && item.Upd.Foldable.Folded;
                                if (FoldableWeapon && FoldedSlot == item.SlotId && (rootFolded || childFolded))
                                    continue;
                                else if (childFoldable && rootFolded && childFolded)
                                    continue;
                                if (itm.Properties.ExtraSizeForceAdd)
                                {
                                    ForcedUp += itm.Properties.ExtraSizeUp;
                                    ForcedDown += itm.Properties.ExtraSizeDown;
                                    ForcedLeft += itm.Properties.ExtraSizeLeft;
                                    ForcedRight += itm.Properties.ExtraSizeRight;
                                }
                                else
                                {
                                    SizeUp = (SizeUp < itm.Properties.ExtraSizeUp) ? itm.Properties.ExtraSizeUp : SizeUp;
                                    SizeDown = (SizeDown < itm.Properties.ExtraSizeDown) ? itm.Properties.ExtraSizeDown : SizeDown;
                                    SizeLeft = (SizeLeft < itm.Properties.ExtraSizeLeft) ? itm.Properties.ExtraSizeLeft : SizeLeft;
                                    SizeRight = (SizeRight < itm.Properties.ExtraSizeRight) ? itm.Properties.ExtraSizeRight : SizeRight;
                                }
                            }
                        }
                        toDo.Remove(toDo.ElementAt(0));
                    }
                }
            }

            return (outX + SizeLeft + SizeRight + ForcedLeft + ForcedRight, outY + SizeUp + SizeDown + ForcedUp + ForcedDown);
        }

        private string GetMoneyCountString(string moneys) => (Items?
            .Where(x => x.Tpl == moneys)
            .Sum(x => x.Upd.StackObjectsCount ?? 0) ?? 0)
            .ToString("N0");
    }
}