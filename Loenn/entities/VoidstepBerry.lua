local VoidstepBerry = {}

VoidstepBerry.name = "BerryHelper/VoidstepBerry"
VoidstepBerry.depth = -100
VoidstepBerry.nodeLineRenderType = "fan"
VoidstepBerry.nodeLimits = {0, -1}
VoidstepBerry.justification = {0.5, 1.0}

VoidstepBerry.texture = "TestBerry/idle/normal00"

VoidstepBerry.placements = {
    {
        name = "Voidstep Berry",
        data = {
            checkpointID = -1,
            order = -1
        },
    }
}

return VoidstepBerry
