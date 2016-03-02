module tiles

[<Measure>] type tl
type tid = int<tl>

module detail =
    type tile_id =
        struct
            val x : tid
            val y : tid
            new(x : int, y : int) = { x = x * 1<tl>; y = y * 1<tl> }
        end

