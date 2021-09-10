#pragma once
#include <Windows.h>
#include <stdint.h>

typedef int64_t int64;
typedef int32_t int32;
typedef int16_t int16;
typedef int8_t int8;
typedef uint64_t uint64;
typedef uint32_t uint32;
typedef uint16_t uint16;
typedef uint8_t uint8;

namespace GM {
	struct ItemResource;
	struct ItemSkin;
}
namespace Test {
	struct Struct;
}
namespace UI {
	struct SlotExtender;
}

#pragma pack(push,1)
namespace GM {
	struct ItemResource {
		struct VirtualTable {
			int(__thiscall*getSkin)(ItemResource*);
			void(__thiscall*drop)(ItemResource*);
		} *vt;
		int itemId;
		uint64 creatorId;
		GM::ItemSkin *itemSkin;
		UI::SlotExtender slotExtender;
	};
}

namespace Test {
}

namespace UI {
	struct SlotExtender {
		int number;
		int replacer;
		int data[0x100];
	};
}
#pragma pack(pop)
