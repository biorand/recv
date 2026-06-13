import csv, json, re, os

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_DIR = os.path.normpath(os.path.join(SCRIPT_DIR, ".."))

KEYS = [
    {"id": 4, "name": "Shotgun", "kind": "reusuable"},
    {"id": 33, "name": "Gold Lugers", "kind": "reusuable"},
    {"id": 39, "name": "Gas Mask", "kind": "reusuable"},
    {"id": 43, "name": "Alexander's Pierce", "kind": "reusuable"},
    {"id": 44, "name": "Alexander's Jewel", "kind": "reusuable"},
    {"id": 45, "name": "Alfred's Ring", "kind": "reusuable"},
    {"id": 46, "name": "Alfred's Jewel", "kind": "reusuable"},
    {"id": 50, "name": "Lockpick", "kind": "reusuable"},
    {"id": 51, "name": "Glass Eye", "kind": "reusuable"},
    {"id": 52, "name": "Piano Roll", "kind": "reusuable"},
    {"id": 53, "name": "Steering Wheel", "kind": "reusuable"},
    {"id": 54, "name": "Crane Key", "kind": "reusuable"},
    {"id": 55, "name": "Lighter", "kind": "reusuable"},
    {"id": 56, "name": "Eagle Plate", "kind": "consumable"},
    {"id": 59, "name": "Hawk Emblem", "kind": "reusuable"},
    {"id": 60, "name": "Queen Ant Object", "kind": "reusuable"},
    {"id": 61, "name": "King Ant Object", "kind": "reusuable"},
    {"id": 62, "name": "Biohazard Card", "kind": "reusuable"},
    {"id": 64, "name": "Detonator", "kind": "reusuable"},
    {"id": 65, "name": "Control Lever", "kind": "reusuable"},
    {"id": 66, "name": "Gold Dragonfly", "kind": "reusuable"},
    {"id": 67, "name": "Silver Key", "kind": "reusuable"},
    {"id": 68, "name": "Gold Key", "kind": "reusuable"},
    {"id": 69, "name": "Army Proof", "kind": "reusuable"},
    {"id": 70, "name": "Navy Proof", "kind": "reusuable"},
    {"id": 71, "name": "Air Force Proof", "kind": "reusuable"},
    {"id": 72, "name": "Key With Tag", "kind": "reusuable"},
    {"id": 73, "name": "ID Card", "kind": "reusuable"},
    {"id": 75, "name": "Airport Key", "kind": "reusuable"},
    {"id": 76, "name": "Emblem Card", "kind": "reusuable"},
    {"id": 77, "name": "Skeleton Picture", "kind": "reusuable"},
    {"id": 78, "name": "Music Box Plate", "kind": "consumable"},
    {"id": 79, "name": "Gold Dragonfly (No Wings)", "kind": "reusuable"},
    {"id": 80, "name": "Album", "kind": "reusuable"},
    {"id": 81, "name": "Halberd", "kind": "reusuable"},
    {"id": 82, "name": "Extinguisher", "kind": "reusuable"},
    {"id": 83, "name": "Briefcase", "kind": "reusuable"},
    {"id": 84, "name": "Padlock Key", "kind": "reusuable"},
    {"id": 85, "name": "TG-01", "kind": "reusuable"},
    {"id": 86, "name": "Sp. Alloy Emblem", "kind": "reusuable"},
    {"id": 87, "name": "Valve Handle", "kind": "reusuable"},
    {"id": 88, "name": "Octa Valve Handle", "kind": "consumable"},
    {"id": 89, "name": "Machine Room Key", "kind": "reusuable"},
    {"id": 90, "name": "Mining Room Key", "kind": "reusuable"},
    {"id": 91, "name": "Bar Code Sticker", "kind": "reusuable"},
    {"id": 92, "name": "Sterile Room Key", "kind": "reusuable"},
    {"id": 93, "name": "Door Knob", "kind": "reusuable"},
    {"id": 94, "name": "Battery Pack", "kind": "reusuable"},
    {"id": 95, "name": "Hemostatic Wire", "kind": "reusuable"},
    {"id": 96, "name": "Turn Table Key", "kind": "reusuable"},
    {"id": 97, "name": "Chem Storage Key", "kind": "reusuable"},
    {"id": 98, "name": "Clement Alpha", "kind": "reusuable"},
    {"id": 99, "name": "Clement Sigma", "kind": "reusuable"},
    {"id": 100, "name": "Tank Object", "kind": "reusuable"},
    {"id": 103, "name": "Rusted Sword", "kind": "reusuable"},
    {"id": 104, "name": "Hemostatic", "kind": "reusuable"},
    {"id": 105, "name": "Security Card", "kind": "reusuable"},
    {"id": 107, "name": "Alexia's Choker", "kind": "reusuable"},
    {"id": 108, "name": "Alexia's Jewel", "kind": "reusuable"},
    {"id": 109, "name": "Queen Ant Relief", "kind": "reusuable"},
    {"id": 110, "name": "King Ant Relief", "kind": "reusuable"},
    {"id": 111, "name": "Red Jewel", "kind": "reusuable"},
    {"id": 112, "name": "Blue Jewel", "kind": "reusuable"},
    {"id": 113, "name": "Socket", "kind": "reusuable"},
    {"id": 114, "name": "Sq Valve Handle", "kind": "reusuable"},
    {"id": 115, "name": "Serum", "kind": "reusuable"},
    {"id": 116, "name": "Earthenware Vase", "kind": "reusuable"},
    {"id": 117, "name": "Paper Weight", "kind": "reusuable"},
    {"id": 119, "name": "Silver Dragonfly", "kind": "reusuable"},
    {"id": 120, "name": "Wing Object", "kind": "consumable"},
    {"id": 121, "name": "Crystal", "kind": "reusuable"},
    {"id": 126, "name": "Plant Pot", "kind": "reusuable"},
    {"id": 142, "name": "M1P", "kind": "reusuable"},
    {"id": 152, "name": "Crest Key S", "kind": "reusuable"},
    {"id": 153, "name": "Crest Key G", "kind": "reusuable"},
]

ITEM_TYPES = {
    "1": {"name": "Rocket Launcher", "kind": "weapon/explosive"},
    "2": {"name": "Assault Rifle", "kind": "weapon/assault-rifle"},
    "3": {"name": "Sniper Rifle", "kind": "weapon/sniper-rifle"},
    "4": {"name": "Shotgun", "kind": "weapon/shotgun"},
    "5": {"name": "Handgun (Glock 17)", "kind": "weapon/handgun"},
    "6": {"name": "Grenade Launcher", "kind": "weapon/grenade-launcher"},
    "7": {"name": "Bow Gun", "kind": "weapon/bow-gun"},
    "8": {"name": "Combat Knife", "kind": "weapon/knife"},
    "9": {"name": "Handgun", "kind": "weapon/handgun"},
    "10": {"name": "Custom Handgun", "kind": "weapon/handgun"},
    "11": {"name": "Linear Launcher", "kind": "weapon/explosive"},
    "12": {"name": "Handgun Bullets", "kind": "ammo/handgun"},
    "13": {"name": "Magnum Bullets", "kind": "ammo/magnum"},
    "14": {"name": "Shotgun Shells", "kind": "ammo/shotgun"},
    "15": {"name": "Grenade Rounds", "kind": "ammo/grenade"},
    "16": {"name": "Acid Rounds", "kind": "ammo/grenade"},
    "17": {"name": "Flame Rounds", "kind": "ammo/grenade"},
    "18": {"name": "Bow Gun Arrows", "kind": "ammo/bow-gun"},
    "19": {"name": "M93R Part", "kind": "weapon/m93r"},
    "20": {"name": "FAid Spray", "kind": "heal"},
    "21": {"name": "Green Herb", "kind": "heal"},
    "22": {"name": "Red Herb", "kind": "heal"},
    "23": {"name": "Blue Herb", "kind": "heal"},
    "24": {"name": "Mixed Herb (2 Green)", "kind": "heal"},
    "25": {"name": "Mixed Herb (Red+Green)", "kind": "heal"},
    "26": {"name": "Mixed Herb (Blue+Green)", "kind": "heal"},
    "27": {"name": "Mixed Herb (2 Green+Blue)", "kind": "heal"},
    "28": {"name": "Mixed Herb (3 Green)", "kind": "heal"},
    "29": {"name": "Mixed Herb (Green+Blue+Red)", "kind": "heal"},
    "30": {"name": "Magnum Bullets (Case)", "kind": "ammo/magnum"},
    "31": {"name": "Ink Ribbon", "kind": "ink-ribbon"},
    "32": {"name": "Magnum", "kind": "weapon/magnum"},
    "33": {"name": "Gold Lugers", "kind": "weapon/handgun"},
    "34": {"name": "Sub Machine Gun", "kind": "weapon/sub-machine-gun"},
    "35": {"name": "Bow Gun Powder", "kind": "gunpowder"},
    "36": {"name": "Gun Powder Arrow", "kind": "gunpowder"},
    "37": {"name": "B.O.W. Gas Rounds", "kind": "ammo/grenade"},
    "38": {"name": "M Gun Bullets", "kind": "ammo/sub-machine-gun"},
    "39": {"name": "Gas Mask", "kind": "key/reusuable"},
    "40": {"name": "Rifle Bullets", "kind": "ammo/sniper-rifle"},
    "42": {"name": "A Rifle Bullets", "kind": "ammo/assault-rifle"},
    "47": {"name": "Prisoner's Diary", "kind": "document"},
    "48": {"name": "Director's Memo", "kind": "document"},
    "49": {"name": "Instructions", "kind": "document"},
    "57": {"name": "Side Pack", "kind": "special"},
    "58": {"name": "Map Roll", "kind": "special"},
    "63": {"name": "Duralumin Case (M93R Parts)", "kind": "weapon/m93r"},
    "74": {"name": "Map", "kind": "special"},
    "77": {"name": "Skeleton Picture", "kind": "quest"},
    "79": {"name": "Gold Dragonfly (No Wings)", "kind": "quest"},
    "80": {"name": "Album", "kind": "document"},
    "102": {"name": "Alfred's Memo", "kind": "document"},
    "106": {"name": "Security File", "kind": "document"},
    "125": {"name": "File", "kind": "special"},
    "126": {"name": "Plant Pot", "kind": "quest"},
    "127": {"name": "Picture B", "kind": "quest"},
    "128": {"name": "Duralumin Case (Bow Gun Powder)", "kind": "gunpowder"},
    "129": {"name": "Duralumin Case (Magnum Rounds)", "kind": "ammo/magnum"},
    "130": {"name": "Bow Gun Powder (Unused)", "kind": "gunpowder"},
    "131": {"name": "Enhanced Handgun", "kind": "weapon/handgun"},
    "132": {"name": "Memo", "kind": "document"},
    "133": {"name": "Board Clip", "kind": "document"},
    "134": {"name": "Card", "kind": "document"},
    "135": {"name": "Newspaper Clip", "kind": "document"},
    "136": {"name": "Luger Replica", "kind": "weapon/handgun"},
    "138": {"name": "Family Picture", "kind": "quest"},
    "139": {"name": "File Folders", "kind": "document"},
    "140": {"name": "Remote Controller", "kind": "quest"},
    "141": {"name": "Question A", "kind": "quest"},
    "142": {"name": "M1P", "kind": "weapon/m1p"},
    "143": {"name": "Calico Bullets", "kind": "ammo/sub-machine-gun"},
    "144": {"name": "Clement Mixture", "kind": "gunpowder"},
    "145": {"name": "Playing Manual", "kind": "document"},
    "146": {"name": "Question B", "kind": "quest"},
    "147": {"name": "Question C", "kind": "quest"},
    "148": {"name": "Question D", "kind": "quest"},
    "149": {"name": "Empty Extinguisher", "kind": "quest"},
    "150": {"name": "Square Socket", "kind": "quest"},
    "151": {"name": "Question E", "kind": "quest"},
}

RDT_JSON = os.path.join(REPO_DIR, "..", "biorand-classic",
    "IntelOrca.Biohazard.BioRand", "meta", "recv", "rdt.json")
CALLOUTS_CSV = os.path.join(REPO_DIR, "docs", "callouts.csv")
OUTPUT = os.path.join(REPO_DIR, "src", "BioRand.RECV", "data", "graph.json")


def load_rdt_json(path):
    with open(path, encoding="utf-8-sig") as f:
        content = f.read()
    content = re.sub(r"//.*", "", content)
    return json.loads(content)


def load_callouts(path):
    mapping = {}
    with open(path, newline="") as f:
        for row in csv.DictReader(f):
            name = row.get("Short Name", "").strip()
            if name:
                mapping[row["RDT #"].strip()] = name
    return mapping


def int_reqs_to_strs(requires_list, prefix="item"):
    return [f"{prefix}({r})" for r in requires_list]


def convert_edge(door):
    lock = door.get("lock")
    no_return = door.get("noReturn")
    randomize = door.get("randomize")
    is_bridge = door.get("isBridgeEdge")

    if lock == "side":
        return None
    if lock == "always" and not no_return:
        return None

    edge = {"target": door["target"]}

    if "id" in door:
        edge["id"] = door["id"]
    if "entranceId" in door:
        edge["entranceId"] = door["entranceId"]

    if no_return:
        edge["kind"] = "noReturn"
    elif lock == "unblock":
        edge["kind"] = "unblock"

    ereqs = []
    if "requires" in door:
        ereqs.extend(int_reqs_to_strs(door["requires"], "item"))
    if "requiresRoom" in door:
        for rr in door["requiresRoom"]:
            ereqs.append(f"node({rr})")
    if ereqs:
        edge["requires"] = ereqs

    if "condition" in door:
        edge["condition"] = door["condition"]

    if "offsets" in door:
        edge["offsets"] = [str(o) for o in door["offsets"]]

    tags = []
    if randomize is False:
        tags.append("preserve")
    if is_bridge:
        tags.append("bridge")
    if tags:
        edge["tags"] = tags

    return edge


def convert_item(item, key_ids=set()):
    slot = {
        "globalId": item["globalId"],
        "type": item["type"],
        "amount": item["amount"],
    }

    requires = []
    if item["type"] in key_ids:
        if "requires" in item:
            requires.extend(int_reqs_to_strs(item["requires"], "item"))
        if "requiresRoom" in item:
            for rr in item["requiresRoom"]:
                requires.append(f"node({rr})")
    if requires:
        slot["requires"] = requires

    if "condition" in item:
        slot["condition"] = item["condition"]

    if "offsets" in item:
        slot["offsets"] = [str(o) for o in item["offsets"]]

    priority = item.get("priority")
    tags = []
    if priority == "fixed":
        tags.append("preserve")
    elif priority == "low":
        tags.extend(["nokey", "nospecial"])
    if tags:
        slot["tags"] = tags

    return slot


def build_rooms(rooms_data, callout_map, key_ids):
    no_return_pairs = set()
    for rid, r in rooms_data.items():
        for door in r.get("doors", []):
            if door.get("noReturn"):
                no_return_pairs.add((rid, door["target"]))

    has_outgoing = set()
    has_incoming = set()
    for rid, r in rooms_data.items():
        for door in r.get("doors", []):
            has_outgoing.add(rid)
            has_incoming.add(door["target"])

    rooms = []
    for rid, r in rooms_data.items():
        room = {"id": rid}

        prefix = rid[:3]
        if prefix in callout_map:
            room["name"] = callout_map[prefix]

        rdt_names = r.get("rdtNames")
        if rdt_names:
            room["rdts"] = rdt_names
        else:
            room["rdts"] = [rid]

        edges = []
        for door in r.get("doors", []):
            edge = convert_edge(door)
            if edge is None:
                continue
            if (door["target"], rid) in no_return_pairs and not door.get("noReturn"):
                continue
            edges.append(edge)
        if edges:
            room["edges"] = edges

        has_room_edges = rid in has_outgoing or rid in has_incoming

        items = []
        for it in r.get("items", []):
            item = convert_item(it, key_ids)
            if has_room_edges and item.get("requires"):
                pass
            elif not has_room_edges:
                item.pop("requires", None)
            items.append(item)
        if items:
            room["items"] = items

        rooms.append(room)
    return rooms


def main():
    data = load_rdt_json(RDT_JSON)
    callout_map = load_callouts(CALLOUTS_CSV)

    ber = data.get("beginEndRooms", [])
    if ber:
        first = ber[0]
        start = first["start"]
        end = first["end"]
        start_dr = start
        end_dr = end
        if len(ber) > 1 and ber[1].get("doorRando"):
            end_dr = ber[1]["end"]
    else:
        start = "1000"
        end = "A090"
        start_dr = start
        end_dr = "A1E0"

    key_ids = {k["id"] for k in KEYS}
    rooms = build_rooms(data["rooms"], callout_map, key_ids)

    output = {
        "start": start,
        "end": end,
        "keys": KEYS,
        "itemTypes": ITEM_TYPES,
        "rooms": rooms,
        "startDoorRando": start_dr,
        "endDoorRando": end_dr,
    }

    with open(OUTPUT, "w") as f:
        json.dump(output, f, indent=4)
        f.write("\n")

    total_edges = sum(len(r.get("edges", [])) for r in rooms)
    total_items = sum(len(r.get("items", [])) for r in rooms)
    print(f"Rooms: {len(rooms)}")
    print(f"Edges: {total_edges}")
    print(f"Items: {total_items}")
    print(f"Named rooms: {sum(1 for r in rooms if 'name' in r)}")
    print(f"Written: {OUTPUT}")


if __name__ == "__main__":
    main()
