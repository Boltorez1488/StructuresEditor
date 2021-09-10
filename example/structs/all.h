#pragma once
#include "empty.h"

#pragma pack(push,1)
namespace GM {
	struct ItemResource;
	struct ItemSkin {
		int skinId;
		int textureFormat;
		enum Slot {
			Armor = 0,
			Head = 1,
			Weapon = 2,
			Boots = 3
		} slot;
	};
}

namespace Test {
	struct Struct {
		GM::ItemResource itemResource[0x2];
	};
}

namespace UI {
	struct SlotExtender;
}
#pragma pack(pop)