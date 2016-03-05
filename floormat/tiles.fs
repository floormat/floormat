module tiles

// block id global for worldspace
[<Measure>] type bcoord_u
type bcoord = int<bcoord_u>

[<Measure>] type bidx_u
type bidx = int<bidx_u>

// tile id local for block
// we don't store (x, y) in large arrays so int is used for ltl unit rather than short or byte
[<Measure>] type ltcoord_u
type ltcoord = int<ltcoord_u>

[<Measure>] type lidx_u
type lidx = int<lidx_u>

// tile id global for worldspace
[<Measure>] type gtl_u
type gtid = int<gtl_u>
[<Measure>] type gidx_u
type gidx = int<gidx_u>

// instantiated actor
[<Measure>] type actor_u
type actor_id = int<actor_u>

// static objects
[<Measure>] type stuff_u
[<Measure>] type floor_u
[<Measure>] type roof_u
type stuff_id = int16<stuff_u>
type floor_id = int16<floor_u>
type roof_id = int16<roof_u>

// blocks with sparse data
[<Literal>]
let BLOCK_SIZE = 80

module detail =
    type slist<'a, 'b> = System.Collections.Generic.SortedList<'a, 'b>
    type smap<'a, 'b> = System.Collections.Generic.SortedDictionary<'a, 'b>

    // actors are the most sparse, and move a lot hence more efficient insertion/deletion
    type block_actors = (lidx, actor_id) smap

    // stuff lying around on the ground, whether blocking movement or not
    // includes walls, trees, loot etc.
    type block_stuff = (lidx, stuff_id) slist
    // roofs and floors are the most sparse. we store them separately as short ints.
    // storing them together would require a boxed reference type.
    // floors also contain stuff like streets, that don't have any roofs.
    type block_roofs = (lidx, roof_id) slist
    type block_floors = (lidx, floor_id) slist

    [<NoComparison>]
    type Block() =
        let actors = block_actors()
        let stuff = block_stuff()
        let roofs = block_roofs()
        let floors = block_floors()
        let mutable rng_state = xorshift.prng_state()

    [<CustomEquality; CustomComparison>]
    type tile_xy =
        struct
            val public X : ltcoord
            val public Y : ltcoord
            val public Block : Block
            new(x, y, block) = { X = x; Y = y; Block = block }
            static member to_idx(x : ltcoord, y : ltcoord) =
                let idx = int y * BLOCK_SIZE + int x
                idx * 1<lidx_u>
            // structs containing reference types use equality with reflection.
            // see: http://dontcodetired.com/blog/post/Improving-Struct-Equality-Performance-in-C.aspx
            interface System.IComparable<tile_xy> with
                member this.CompareTo(other) =
                    tile_xy.to_idx(this.X, this.Y) - tile_xy.to_idx(other.X, other.Y)
                    |> int
            interface System.IEquatable<tile_xy> with
                member this.Equals(other) = this.X = other.X && this.Y = other.Y
            // TODO add accessors for tile contents
        end

// NOTE use jagged arrays for block array in worldspace class
// multidimensional arrays require multiply and add
