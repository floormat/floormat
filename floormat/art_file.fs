module art_file

module detail =
    open dat_file
    open System.IO.MemoryMappedFiles

    [<Literal>]
    let FLAG_STATIC = 0x00000001u
    [<Literal>]
    let FLAG_CRITTER = 0x00000002u
    [<Literal>]
    let FLAG_FONT = 0x00000004u
    [<Literal>]
    let FLAG_FACADE = 0x00000008u
    [<Literal>]
    let FLAG_UNKNOWN = 0x00000010u

    type File = System.IO.File

    type Frame = {
        width : int
        height : int
        offset_x : int // position relative to a tile
        offset_y : int
        delta_x : int // modifies position when moving
        delta_y : int
        mutable data : int32 [] [] // palette, pixel
    }

    type ArtFile = {
        framerate : int
        action_frame : int
        palette_count : int
        rotations : Frame [,] // rotation, frame no
        rotation_count : int
        frame_count : int
    }

    type Stream = System.IO.Stream
    type GC = System.GC
    type 'a thread_local = 'a System.Threading.ThreadLocal

    let palettes_ = new thread_local<_>(fun () -> Array2D.zeroCreate 8 256)
    let sizes_ = new thread_local<_>(fun () -> Array2D.zeroCreate 8 0)
    let palette_frame_ = new thread_local<_>(fun () -> Array.zeroCreate 0)

    type ArtFile with
        static member public from_file_entry(dat_file : DatFile, filename) =
            //let no_gc_p = GC.TryStartNoGCRegion(int64 (1024 * 1024 * 32))

            use stream = dat_file.[filename]
            let flags = stream.read_le_uint32()
            let frame_rate = stream.read_le_int32()
            let rotation_count =
                let rotation_count = stream.read_le_int32()
                if flags &&& FLAG_STATIC <> 0u then 1 else rotation_count
            let mutable palette_count = 0
            for _ in 1..4 do
                if stream.read_le_uint32() <> 0u then
                    palette_count <- palette_count + 1
            let action_frame = stream.read_le_int32()
            let frame_count = stream.read_le_int32()
            for _ in 1 .. 24 do
                stream.read_le_uint32() |> ignore

            let palettes = palettes_.Value

            for i in 0 .. palette_count - 1 do
                palettes.[i, 0] <- 0u
                stream.read_le_uint32() |> ignore
                for j in 1 .. 255 do
                    let color = stream.read_le_uint32()
                    palettes.[i, j] <- color ||| (255u <<< 24)
            let frames = Array2D.zeroCreate rotation_count frame_count
            
            if frame_count > sizes_.Value.GetUpperBound(1) then
                sizes_.Value <- Array2D.zeroCreate 8 frame_count

            let sizes = sizes_.Value

            for i in 0 .. rotation_count - 1 do
                for k in 0 .. frame_count - 1 do
                    let frame_width = stream.read_le_int32()
                    let frame_height = stream.read_le_int32()
                    let frame_size = stream.read_le_int32()
                    let offset_x = stream.read_le_int32()
                    let offset_y = stream.read_le_int32()
                    let delta_x = stream.read_le_int32()
                    let delta_y = stream.read_le_int32()
                    frames.[i, k] <- {
                        width = frame_width
                        height = frame_height
                        offset_x = offset_x
                        offset_y = offset_y
                        delta_x = delta_x
                        delta_y = delta_y
                        data = null
                    }

                    sizes.[i, k] <- frame_size

            for i in 0 .. rotation_count - 1 do
                for k in 0 .. frame_count - 1 do
                    let fr = frames.[i, k]
                    let len = fr.width * fr.height

                    if len > palette_frame_.Value.Length then
                        palette_frame_.Value <- Array.zeroCreate len

                    let palette_frame = palette_frame_.Value

                    let sz = sizes.[i, k]
                    if sz = len then
                        for x in 0 .. sz - 1 do
                            let idx = stream.read_byte()
                            palette_frame.[x] <- idx
                    else
                        let mutable pos = 0
                        while pos < len do
                            let repeat = stream.read_byte()
                            let flag = (repeat &&& 0x80uy) = 0uy
                            let cnt = repeat &&& 0x7fuy |> int
                            if flag then
                                let b = stream.read_byte()
                                for _ in 1 .. cnt do
                                    palette_frame.[pos] <- b
                                    pos <- pos + 1
                            else
                                for _ in 1 .. cnt do
                                    let idx = stream.read_byte()
                                    palette_frame.[pos] <- idx
                                    pos <- pos + 1

                    let frame_data = Array.zeroCreate palette_count

                    for j in 0 .. palette_count - 1 do
                        let img = Array.zeroCreate len
                        frame_data.[j] <- img
                        for i in 0 .. len - 1 do
                            let idx = palette_frame.[i] |> int
                            let pixel = palettes.[j, idx] |> int32 
                            img.[i] <- pixel
                    frames.[i, k].data <- frame_data

            //if no_gc_p then GC.EndNoGCRegion()

            {
                rotations = frames
                framerate = frame_rate
                action_frame = action_frame
                palette_count = palette_count
                rotation_count = rotation_count
                frame_count = frame_count
            }

type ArtFile = detail.ArtFile
type Frame = detail.Frame
