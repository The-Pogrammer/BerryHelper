local TreeBerry = {}

TreeBerry.name = "BerryHelper/TreeBerry"
TreeBerry.depth = -100
TreeBerry.nodeLineRenderType = "fan"
TreeBerry.nodeLimits = {0, -1}
TreeBerry.justification = {0.5, 1.0}

TreeBerry.texture = "TreeBerry/idle/normal00"

TreeBerry.placements = {
    {
        name = "Tree Berry",
        data = {
            checkpointID = -1,
            order = -1
        },
    }
}

return TreeBerry
