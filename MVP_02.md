# BalconyGarden - MVP 02

## 1. MVP Goal

MVP_02 builds on the completed MVP_01 core planting loop.

The goal is to make the loop slightly deeper without expanding the project into the full long-term vision.

MVP_02 should prove this loop:

Open game
-> Load save data
-> Plants grow by real-world time
-> Player harvests flowers or fruits
-> Fruits can be converted into seeds
-> Player plants seeds again
-> Simple collection records are updated
-> Save progress

MVP_02 is still a small prototype milestone. It should improve the core loop, not introduce monetization, platform SDKs, or complex collection systems.

## 2. Baseline From MVP_01

MVP_01 is considered complete.

The project already supports:

- One balcony scene
- 5 horizontal pot slots
- 1-slot and 2-slot pots
- Pot placement and occupancy
- Seed planting
- Real-world-time plant growth
- Mature plant harvest
- Seed inventory
- Harvest inventory
- Local JSON save/load through PlayerPrefs
- Offline growth after loading
- Simple temporary UI for testing

MVP_02 should preserve all MVP_01 behavior.

## 3. MVP_02 Core Loop

MVP_02 extends the loop from:

Plant seed -> wait -> harvest

to:

Plant seed -> wait -> harvest flower or fruit -> convert fruit to seed -> plant again

The player should be able to understand the difference between flowers and fruits, and fruit harvests should create a simple renewable seed loop.

## 4. Feature Scope

MVP_02 includes only:

- Flower / fruit harvest distinction
- Simple seed inventory loop
- Fruit-to-seed conversion
- Simple collection records
- Lightweight future-facing hooks for a bird system

MVP_02 does not include full bird gameplay, rare traits, shop systems, ads, WeChat SDK, or cloud save.

## 5. Harvest Type System

Plants should define what kind of item they produce when harvested.

Required harvest types:

- Flower
- Fruit
- Other or Unknown, only if useful for placeholder data

Suggested MVP_02 plant harvest mapping:

- plant_daisy -> Flower
- plant_tomato -> Fruit
- plant_mystery -> Flower for MVP_02, unless a clearer test use is needed

The harvest type should be data-driven. Gameplay logic should not check display names such as "Daisy" or "Tomato".

## 6. Fruit-To-Seed Conversion

Fruit harvest items can be converted into seeds.

Minimum required conversion:

- Tomato Fruit x1 -> Tomato Seed x1

Rules:

- Only Fruit-type harvest items can be converted into seeds.
- Flower-type harvest items cannot be converted into seeds.
- Conversion should reduce the fruit count and increase the matching seed count.
- Conversion should be data-driven through IDs or data references.
- No random chance in MVP_02.
- No multi-item recipes in MVP_02.
- No rare inheritance in MVP_02.
- No crafting UI beyond a simple temporary test button/list.

## 7. Seed Inventory Loop

The player should be able to continue planting after harvesting fruit.

The minimum loop to prove:

1. Plant Tomato Seed.
2. Wait until the Tomato reaches Mature.
3. Harvest Tomato Fruit.
4. Convert Tomato Fruit into Tomato Seed.
5. Plant Tomato Seed again.

This loop should work after save/load.

Flowers do not need to generate seeds in MVP_02.

## 8. Simple Collection Records

MVP_02 should add a simple record of what the player has collected.

Minimum record data:

- plantId or harvestItemId
- displayName
- firstCollected
- totalHarvestCount

Collection records should update when the player harvests a mature plant.

This is not a full album system.

MVP_02 collection records should not include:

- Art pages
- Specimen creation
- Rare colors
- Rare trait history
- Size records
- Completion rewards
- Sorting or filtering beyond what is needed for testing

The temporary UI may show the records as a simple list.

## 9. Bird System Preparation Only

MVP_02 may add lightweight hooks that future bird systems can listen to.

Acceptable examples:

- Event raised when a fruit is harvested
- Event raised when harvest inventory changes
- Event raised when fruit becomes available

These hooks should not implement bird behavior.

Do not add:

- Bird visits
- Bird AI
- Bird friendship
- Bird eating fruit
- Bird nest
- Eggs
- Baby birds
- Multiple bird types
- Bird collection records

The goal is only to avoid painting the future bird system into a corner.

## 10. Save / Load Requirements

MVP_02 save data should include:

- Existing MVP_01 pot layout data
- Existing plant growth data
- Existing seed inventory
- Harvest inventory with harvest item counts
- Collection records

Save/load must preserve:

- Current pot layout
- Plants inside pots
- Growth progress based on real-world time
- Seed counts
- Flower and fruit counts
- Fruit-to-seed conversion results
- Collection record counts

MVP_02 should continue to use local save only.

Do not add cloud save or WeChat SDK save in MVP_02.

## 11. Temporary UI Requirements

Temporary UI is acceptable for MVP_02.

The UI should allow testing:

- Current seed counts
- Current flower counts
- Current fruit counts
- Selected pot and plant state
- Harvest action
- Fruit-to-seed conversion action
- Simple collection record list

The UI does not need final art or final layout.

Avoid making the test UI too crowded. If needed, split it into simple sections:

- Garden
- Inventory
- Collection

## 12. Suggested Implementation Steps

1. Add harvest type data.
2. Mark existing harvest items as Flower or Fruit.
3. Add fruit-to-seed conversion data.
4. Add conversion logic that consumes fruit and adds seed.
5. Add simple collection records.
6. Save and load collection records.
7. Add temporary UI for conversion and records.
8. Add lightweight harvest/inventory events for future bird system use.
9. Run a full MVP_01 regression test.

Each step should be small and testable.

## 13. Explicitly Excluded From MVP_02

Do not implement the following in MVP_02:

- Ads
- WeChat SDK
- Cloud save
- Shop
- Purchases
- Complex crafting recipes
- Rare plant traits
- Rare flower colors
- Genetic inheritance
- Full album UI
- Flower specimen system
- Collection rewards
- Bird visits
- Multiple bird species
- Bird friendship
- Bird nest
- Eggs or baby birds
- Weather
- Seasons
- Multiple balcony areas
- Pot upgrades
- Full art pass

Simple placeholder visuals remain acceptable.

## 14. MVP_02 Success Criteria

MVP_02 is considered successful when:

1. Harvested items are categorized as Flower or Fruit.
2. Daisy harvests as a Flower item.
3. Tomato harvests as a Fruit item.
4. Flower items cannot be converted into seeds.
5. Fruit items can be converted into matching seeds.
6. Converting fruit reduces fruit count and increases seed count.
7. The Tomato loop works: Tomato Seed -> Tomato Plant -> Tomato Fruit -> Tomato Seed.
8. Collection records update after harvest.
9. Collection records show total harvest count.
10. Save/load restores seed counts, harvest counts, and collection records.
11. Save/load still restores pot layout and plant growth from MVP_01.
12. Offline growth still works after closing and reopening the game.
13. No ads, WeChat SDK, shop, cloud save, rare traits, or full bird system are implemented.

## 15. Manual Verification Checklist

Before marking MVP_02 complete, verify in Unity Play Mode:

1. Clear save and reset test seeds.
2. Place a small pot.
3. Plant Daisy Seed.
4. Wait until Daisy is Mature.
5. Harvest Daisy and confirm a Flower item is added.
6. Confirm Daisy Flower cannot be converted into a seed.
7. Plant Tomato Seed.
8. Wait until Tomato is Mature.
9. Harvest Tomato and confirm a Fruit item is added.
10. Convert Tomato Fruit into Tomato Seed.
11. Confirm Tomato Fruit count decreases.
12. Confirm Tomato Seed count increases.
13. Plant the newly created Tomato Seed.
14. Save the game.
15. Clear layout.
16. Load the game.
17. Confirm pots, plants, seed counts, harvest counts, and collection records restore correctly.
18. Leave one plant unharvested, wait real-world time, and confirm growth advances after load.
19. Confirm the Console has no red errors.

## 16. Notes For Future MVPs

After MVP_02, the project may consider:

- A cleaner non-IMGUI UI
- A more formal inventory panel
- First simple bird visit prototype
- Better placeholder art
- More plant types
- More harvest item types

These are intentionally outside MVP_02 unless explicitly requested later.
